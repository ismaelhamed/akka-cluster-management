using System;
using System.IO;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Configuration;

namespace Akka.Cluster.Management.Host
{
    public class Startup
    {
        private ActorSystem actorSystem;

        public void Start()
        {
            var actorSystemName = "akka-cluster";
            var hoconConfig = ConfigurationFactory.ParseString(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "application.conf"));

            var config = hoconConfig.GetConfig("cluster-management");
            if (config != null)
            {
                actorSystemName = config.GetString("actorsystem", actorSystemName);
            }

            actorSystem = ActorSystem.Create(actorSystemName, hoconConfig);

            Cluster.Get(actorSystem).RegisterOnMemberUp(() =>
            {
                ClusterSharding.Get(actorSystem).Start(
                    typeName: "my-actor",
                    entityProps: Props.Create<MyActor>(),
                    settings: ClusterShardingSettings.Create(actorSystem),
                    messageExtractor: new MessageExtractor());
            });

            // Akka Management hosts the HTTP routes used by bootstrap
            ClusterHttpManagement.Get(actorSystem).Start();
        }

        public void Stop()
        {
            ClusterHttpManagement.Get(actorSystem).Stop();
            CoordinatedShutdown.Get(actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance).Wait();
        }
    }

    internal sealed class ShardEnvelope
    {
        public readonly string EntityId;
        public readonly object Message;

        public ShardEnvelope(string entityId, object message)
        {
            EntityId = entityId;
            Message = message;
        }
    }

    internal sealed class MessageExtractor : HashCodeMessageExtractor
    {
        public MessageExtractor() : base(maxNumberOfShards: 100) { }
        
        public override string EntityId(object message)
        {
            switch(message)
            {
                case ShardEnvelope e: return e.EntityId;
                case ShardRegion.StartEntity start: return start.EntityId;
            }

            return null;
        }

        public new object EntityMessage(object message) => (message as ShardEnvelope)?.Message ?? message;
    }

    internal class MyActor : ReceiveActor
    { }
}

using System;
using System.IO;
using Akka.Actor;
using Akka.Cluster.Http.Management;
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
            ClusterSharding.Get(actorSystem).Start(
                typeName: "my-actor",
                entityProps: Props.Create<MyActor>(),
                settings: ClusterShardingSettings.Create(actorSystem),
                messageExtractor: new MessageExtractor());

            ClusterHttpManagement.Get(actorSystem).Start();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        public void Stop()
        {
            ClusterHttpManagement.Get(actorSystem).Stop();
            actorSystem.Terminate();
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

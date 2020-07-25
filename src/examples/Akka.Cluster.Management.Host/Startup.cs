using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Akka.Cluster.Sharding;
using Akka.Util.Internal;
using Microsoft.Extensions.Hosting;

namespace Akka.Cluster.Management.Host
{
    public class Startup : IHostedService
    {
        private ActorSystem actorSystem;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var actorSystemName = "akka-cluster";
            var hoconConfig = ConfigurationFactory.ParseString(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "application.conf"));

            var config = hoconConfig.GetConfig("cluster-management");
            if (config != null)
            {
                actorSystemName = config.GetString("actorsystem", actorSystemName);
            }

            actorSystem = ActorSystem.Create(actorSystemName, hoconConfig);

            // Akka Management hosts the HTTP routes used by bootstrap
            ClusterHttpManagement.Get(actorSystem).Start();

            Cluster.Get(actorSystem).RegisterOnMemberUp(() =>
            {
                var shardRegion = ClusterSharding.Get(actorSystem).Start(
                    typeName: "my-actor",
                    entityProps: Props.Create<MyActor>(),
                    settings: ClusterShardingSettings.Create(actorSystem),
                    messageExtractor: new MessageExtractor());

                Enumerable.Range(1, 100).ForEach(i => shardRegion.Tell(new ShardEnvelope(Guid.NewGuid().ToString(), "hello!")));
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            ClusterHttpManagement.Get(actorSystem).Stop();
            return CoordinatedShutdown.Get(actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
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
        public MessageExtractor()
            : base(maxNumberOfShards: 10)
        { }

        public override string EntityId(object message) =>
            message switch
            {
                ShardEnvelope e => e.EntityId,
                ShardRegion.StartEntity start => start.EntityId,
                _ => null
            };

        public new object EntityMessage(object message) => ( message as ShardEnvelope )?.Message ?? message;
    }

    internal class MyActor : ReceiveActor
    {
    }
}
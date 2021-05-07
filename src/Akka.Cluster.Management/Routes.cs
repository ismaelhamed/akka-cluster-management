using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Cluster.Sharding;
using Akka.Http.Dsl.Model;
using Akka.IO;

namespace Akka.Cluster.Management
{
    public class Routes
    {
        private readonly Cluster cluster;
        //private readonly IActorRef routeActor;

        private static readonly HttpResponse NotFound = HttpResponse.Create()
            .WithStatus(404)
            .WithEntity("Unknown resource!");

        /// <summary>
        /// Creates an instance of [[ClusterHttpManagementRoutes]] to manage the specified
        /// [[akka.cluster.Cluster]] instance. This version does not provide Basic Authentication.
        /// </summary>
        public static Routes Create(Cluster cluster) => new(cluster);

        /// <summary>
        /// Creates an instance of [[ClusterHttpManagementRoutes]] to manage the specified
        /// [[akka.cluster.Cluster]] instance. This version does not provide Basic Authentication.
        /// </summary>
        // private Routes(ActorSystem system) =>
        //     routeActor = system.ActorOf(ClusterHttpManagementRoutes.Props(Cluster.Get(system)), "routes");

        private Routes(Cluster cluster) => this.cluster = cluster;

        public Task<HttpResponse> RequestHandler(HttpRequest request) => request.Path switch
        {
            "/members" => Task.FromResult(NotFound),
            "/shards/{name}" => RouteGetShardInfo(""),
            _ => Task.FromResult(NotFound)
        };

        private async Task<HttpResponse> RouteGetShardInfo(string shardRegionName)
        {
            try
            {
                var stats = await ClusterSharding.Get(cluster.System)
                    .ShardRegion(shardRegionName)
                    .Ask<ShardRegionStats>(GetShardRegionStats.Instance);

                var entity = new ShardDetails(stats.Stats.Select(stat =>
                    new ShardRegionInfo(stat.Key, stat.Value)).ToArray());

                // TODO
                return HttpResponse.Create(200, HttpEntity.Create(ByteString.FromBytes(entity)));
            }
            catch (AskTimeoutException)
            {
                // var entity = new ClusterHttpManagementMessage($"Shard Region [{shardRegionName}] not responding, may have been terminated");

                return HttpResponse.Create(404, new ResponseEntity(
                    "text/plain(UTF-8)",
                    ByteString.FromString($"Shard Region [{shardRegionName}] not responding, may have been terminated")));
            }
            catch (Exception)
            {
                // var entity = new ClusterHttpManagementMessage($"Shard Region [{shardRegionName}] is not started");

                return HttpResponse.Create(404, new ResponseEntity(
                    "text/plain(UTF-8)",
                    ByteString.FromString($"Shard Region [{shardRegionName}] is not started")));
            }
        }

        // private async Task<HttpResponse> HandleGetShardInfo(string name)
        // {
        //     try
        //     {
        //         var response = await routeActor.Ask<Complete>(new GetShardInfo(name), TimeSpan.FromSeconds(3));
        //         return response switch
        //         {
        //             Complete.Success => HttpResponse.Create(), // TODO
        //             Complete.Failure failure => HttpResponse.Create(404, new ResponseEntity("text/plain(UTF-8)", ByteString.FromString(failure.Reason))),
        //             _ => throw new InvalidOperationException("Something went wrong. Cluster might be shutdown.")
        //         };
        //     }
        //     catch
        //     {
        //         return HttpResponse.Create().WithStatus(500);
        //     }
        // }

        private static ClusterMember MemberToClusterMember(Member member) =>
            new(member.UniqueAddress.Address.ToString(), member.UniqueAddress.Uid.ToString(), member.Status.ToString(), member.Roles.ToArray());
    }
}
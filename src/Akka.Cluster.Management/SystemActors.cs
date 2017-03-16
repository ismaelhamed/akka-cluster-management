using Akka.Actor;

namespace Akka.Cluster.Http.Management
{
    public static class SystemActors
    {
        public static IActorRef RoutesHandler = ActorRefs.Nobody;
    }
}
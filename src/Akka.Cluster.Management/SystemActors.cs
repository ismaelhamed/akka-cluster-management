using Akka.Actor;

namespace Akka.Cluster.Management
{
    public static class SystemActors
    {
        public static IActorRef RoutesHandler = ActorRefs.Nobody;
    }
}
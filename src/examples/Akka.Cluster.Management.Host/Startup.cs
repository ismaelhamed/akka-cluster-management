using System;
using System.Configuration;
using Akka.Actor;
using Akka.Cluster.Http.Management;
using Akka.Configuration.Hocon;

namespace Akka.Cluster.Management.Host
{
    public class Startup
    {
        private ActorSystem actorSystem;

        public void Start()
        {
            var actorSystemName = "akka-cluster-management";

            var config = ((AkkaConfigurationSection)ConfigurationManager.GetSection("akka")).AkkaConfig.GetConfig("cluster-management");
            if (config != null)
            {
                actorSystemName = config.GetString("actorsystem", actorSystemName);
            }

            actorSystem = ActorSystem.Create(actorSystemName);
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
}

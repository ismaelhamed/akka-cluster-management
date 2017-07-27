using System;
using System.Configuration;
using Akka.Actor;
using Akka.Cluster.Http.Management;
using Akka.Configuration.Hocon;

namespace Akka.Cluster.Management.Host
{
    public class Startup
    {
        private ActorSystem system;

        public void Start()
        {
            var str = "cluster-management-system";
            var config = ((AkkaConfigurationSection)ConfigurationManager.GetSection("akka")).AkkaConfig.GetConfig("cluster-management");
            if (config != null)
                str = config.GetString("actorsystem", str);
            system = ActorSystem.Create(str);
            ClusterHttpManagement.Get(system).Start();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        public void Stop()
        {
            ClusterHttpManagement.Get(system).Stop();
            system.Terminate();
        }
    }
}

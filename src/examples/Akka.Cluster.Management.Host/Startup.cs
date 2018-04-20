using System;
using System.IO;
using Akka.Actor;
using Akka.Cluster.Http.Management;
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

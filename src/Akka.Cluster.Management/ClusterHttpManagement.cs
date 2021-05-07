using System;
using System.Net;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Akka.Cluster.Management
{
    using Http = Http.Dsl.Http;

    /// <summary>
    /// Class to instantiate a <see cref="ClusterHttpManagement"/> to provide an HTTP management interface for Akka <seealso cref="Cluster"/>.
    /// </summary>
    public class ClusterHttpManagement : IExtension
    {
        private readonly ActorSystem system;
        private readonly string pathPrefix;

        public ClusterHttpManagementSettings Settings { get; }

        /// <summary>
        /// Creates an instance of <see cref="ClusterHttpManagement"/> to manage the specified <seealso cref="Cluster"/> instance.
        /// This version does not provide security (Basic Authentication or SSL) and uses the default path "members".
        /// </summary>
        public ClusterHttpManagement(ExtendedActorSystem system)
            : this(system, "/cluster")
        { }

        /// <summary>
        /// Creates an instance of <see cref="ClusterHttpManagement"/> to manage the specified <seealso cref="Cluster"/> instance.
        /// This version does not provide security (Basic Authentication or SSL) and it uses the specified path "pathPrefix".
        /// </summary>
        public ClusterHttpManagement(ExtendedActorSystem system, string pathPrefix)
        {
            this.system = system;
            this.pathPrefix = pathPrefix;

            system.Settings.InjectTopLevelFallback(DefaultConfiguration());
            Settings = new ClusterHttpManagementSettings(system.Settings.Config.GetConfig(ClusterHttpManagementSettings.ConfigPath));
        }

        public static ClusterHttpManagement Get(ActorSystem system) =>
            system.WithExtension<ClusterHttpManagement, ClusterHttpManagementExtensionProvider>();

        public static Config DefaultConfiguration() =>
            ConfigurationFactory.FromResource<ClusterHttpManagement>("Akka.Cluster.Management.reference.conf");

        public void Start()
        {
            var routes = Routes.Create(Cluster.Get(system));

            var bindingTask = Http.Get(system).NewServerAt("localhost", 8085).Bind(routes.RequestHandler);
            bindingTask.WhenComplete((binding, exception) =>
            {
                if (binding != null)
                {
                    var address = (DnsEndPoint)binding.LocalAddress;
                    system.Log.Info("Server online at http://{0}:{1}/",
                        address.Host,
                        address.Port);

                    // make sure Akka HTTP is shut down in a proper way
                    binding.AddToCoordinatedShutdown(TimeSpan.FromSeconds(10), system);
                }
                else
                {
                    system.Log.Error(exception, "Failed to bind HTTP endpoint, terminating system...");
                    system.Terminate();
                }
            });
        }
    }

    public class ClusterHttpManagementExtensionProvider : ExtensionIdProvider<ClusterHttpManagement>
    {
        public override ClusterHttpManagement CreateExtension(ExtendedActorSystem system) => new(system);
    }
}
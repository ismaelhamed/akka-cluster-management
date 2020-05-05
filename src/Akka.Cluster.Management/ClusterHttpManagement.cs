using Akka.Actor;
using Akka.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Akka.Cluster.Management
{
    /// <inheritdoc />
    /// <summary>
    /// Class to instantiate an {Akka.Cluster.Http.Management.ClusterHttpManagement} to provide an HTTP management interface for {Akka.Cluster.Cluster}.
    /// </summary>
    public class ClusterHttpManagement : IExtension
    {
        private IWebHost host;
        private readonly string pathPrefix;

        public ClusterHttpManagementSettings Settings { get; }

        /// <inheritdoc />
        /// <summary>
        /// Creates an instance of {Akka.Cluster.Http.Management.ClusterHttpManagement} to manage the specified {Akka.Cluster.Cluster} instance. 
        /// This version does not provide security (Basic Authentication or SSL) and uses the default path "members".
        /// </summary>
        public ClusterHttpManagement(ExtendedActorSystem system)
            : this(system, "/cluster")
        { }

        /// <summary>
        /// Creates an instance of {Akka.Cluster.Http.Management.ClusterHttpManagement} to manage the specified {Akka.Cluster.Cluster} instance. 
        /// This version does not provide security (Basic Authentication or SSL) and it uses the specified path "pathPrefix".
        /// </summary>
        public ClusterHttpManagement(ExtendedActorSystem system, string pathPrefix)
        {
            this.pathPrefix = pathPrefix;

            system.Settings.InjectTopLevelFallback(DefaultConfiguration());
            Settings = new ClusterHttpManagementSettings(system.Settings.Config.GetConfig(ClusterHttpManagementSettings.ConfigPath));
            SystemActors.RoutesHandler = system.ActorOf(ClusterHttpManagementRoutes.Props(Cluster.Get(system)), "routes");
        }

        public static ClusterHttpManagement Get(ActorSystem system) =>
            system.WithExtension<ClusterHttpManagement, ClusterHttpManagementExtensionProvider>();

        public static Config DefaultConfiguration() =>
            ConfigurationFactory.FromResource<ClusterHttpManagement>("Akka.Cluster.Management.reference.conf");

        public void Start()
        {
            host = WebHost.CreateDefaultBuilder()
                .SuppressStatusMessages(true)
                .ConfigureServices(services =>
                {
                    services.AddMvcCore()
                        .AddFormatterMappings()
                        .AddJsonFormatters()
                        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

                    services.AddLogging(builder => builder.AddFilter("Microsoft", LogLevel.Warning));
                })
                .Configure(app =>
                {
                    app.UsePathBase(new PathString(pathPrefix));
                    app.UseMvc();
                })
                // NOTE: This module does not provide security by default. It's the developer's choice to add security to this API.
                .UseUrls($"http://{Settings.ClusterHttpManagementHostname}:{Settings.ClusterHttpManagementPort}")
                .Build();

            host.Start();
        }

        public void Stop() => host.StopAsync().Wait();
    }

    public class ClusterHttpManagementExtensionProvider : ExtensionIdProvider<ClusterHttpManagement>
    {
        public override ClusterHttpManagement CreateExtension(ExtendedActorSystem system) => new ClusterHttpManagement(system);
    }
}
using System;
using System.Web.Http;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace Akka.Cluster.Http.Management
{
    /// <summary>
    /// Class to instantiate an {Akka.Cluster.Http.Management.ClusterHttpManagement} to provide an HTTP management interface for {Akka.Cluster.Cluster}.
    /// </summary>
    public class ClusterHttpManagement : IExtension
    {
        private IDisposable server;
        private readonly string pathPrefix;

        public ClusterHttpManagementSettings Settings { get; }

        /// <summary>
        /// Creates an instance of {Akka.Cluster.Http.Management.ClusterHttpManagement} to manage the specified {Akka.Cluster.Cluster} instance. 
        /// This version does not provide security (Basic Authentication or SSL) and uses the default path "members".
        /// </summary>
        public ClusterHttpManagement(ExtendedActorSystem system)
            : this(system, "/members")
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

        public static ClusterHttpManagement Get(ActorSystem system)
        {
            return system.WithExtension<ClusterHttpManagement, ClusterHttpManagementExtensionProvider>();
        }

        public static Config DefaultConfiguration()
        {
            return ConfigurationFactory.FromResource<ClusterHttpManagement>("Akka.Cluster.Http.Management.reference.conf");
        }

        public void Start()
        {
            // NOTE: This module does not provide security by default. It's the developer's choice to add security to this API.
            var url = $"http://{Settings.ClusterHttpManagementHostname}:{Settings.ClusterHttpManagementPort}";

            server = WebApp.Start(url, app =>
            {
                app.Map(pathPrefix, api =>
                {
                    var config = new HttpConfiguration();
                    config.MapHttpAttributeRoutes();

                    var settings = config.Formatters.JsonFormatter.SerializerSettings;
                    settings.Formatting = Formatting.Indented;
                    settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                    // Disable default XmlFormatter
                    config.Formatters.Remove(config.Formatters.XmlFormatter);

                    // Now add in the WebAPI middleware
                    api.UseWebApi(config);
                });
            });
        }

        public void Stop()
        {
            server.Dispose();
        }
    }

    public class ClusterHttpManagementExtensionProvider : ExtensionIdProvider<ClusterHttpManagement>
    {
        public override ClusterHttpManagement CreateExtension(ExtendedActorSystem system)
        {
            return new ClusterHttpManagement(system);
        }
    }
}
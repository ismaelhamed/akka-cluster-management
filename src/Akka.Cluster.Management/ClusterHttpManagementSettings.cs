using Akka.Configuration;

namespace Akka.Cluster.Management
{
    public class ClusterHttpManagementSettings
    {
        public static readonly string ConfigPath = "akka.cluster.http.management";

        public int ClusterHttpManagementPort { get; }
        public string ClusterHttpManagementHostname { get; }

        public ClusterHttpManagementSettings(Config config)
        {
            ClusterHttpManagementPort = config.GetInt("port", 8558);
            ClusterHttpManagementHostname = config.GetString("hostname", "localhost");
        }
    }
}

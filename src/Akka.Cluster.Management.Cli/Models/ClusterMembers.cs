namespace Akka.Cluster.Management.Cli.Models
{
    public class ClusterMember
    {
        public string Node { get; set; }
        public string NodeUid { get; set; }
        public string Status { get; set; }
        public string[] Roles { get; set; }
    }

    public class ClusterUnreachableMember
    {
        public string Node { get; set; }
        public string[] ObservedBy { get; set; }
    }

    public class ClusterMembers
    {
        public string SelfNode { get; set; }
        public ClusterMember[] Members { get; set; }
        public ClusterUnreachableMember[] Unreachable { get; set; }
        public string Leader { get; set; }
    }

    public class ClusterHttpManagementMessage
    {
        public string Message { get; set; }
    }
}

namespace Akka.Cluster.Management
{
    public class ClusterMember
    {
        public readonly string Node;
        public readonly string NodeUid;
        public readonly string Status;
        public readonly string[] Roles;

        public static ClusterMember Empty => new ClusterMember();

        public ClusterMember()
        { }

        public ClusterMember(string node, string nodeUid, string status, string[] roles)
        {
            Node = node;
            NodeUid = nodeUid;
            Status = status;
            Roles = roles;
        }
    }

    public class ClusterUnreachableMember
    {
        public readonly string Node;
        public readonly string[] ObservedBy;

        public ClusterUnreachableMember(string node, string[] observedBy)
        {
            Node = node;
            ObservedBy = observedBy;
        }
    }

    public class ClusterMembers
    {
        public readonly string SelfNode;
        public readonly ClusterMember[] Members;
        public readonly ClusterUnreachableMember[] Unreachable;
        public readonly string Leader;
        public readonly string Oldest;

        public ClusterMembers(string selfNode, ClusterMember[] members = null, ClusterUnreachableMember[] unreachable = null, string leader = null, string oldest = null)
        {
            SelfNode = selfNode;
            Members = members;
            Unreachable = unreachable;
            Leader = leader;
            Oldest = oldest;
        }
    }

    public class ClusterHttpManagementMessage
    {
        public readonly string Message;

        public ClusterHttpManagementMessage(string message) => Message = message;
    }

    public class ShardDetails
    {
        public readonly ShardRegionInfo[] Regions;

        public ShardDetails(ShardRegionInfo[] regions) => Regions = regions;
    }

    public class ShardRegionInfo
    {
        public readonly string ShardId;
        public readonly int NumEntities;

        public ShardRegionInfo(string shardId, int numEntities)
        {
            ShardId = shardId;
            NumEntities = numEntities;
        }
    }
}

using System.Linq;
using Akka.Actor;

namespace Akka.Cluster.Http.Management
{
    public class GetMembers
    { }

    public class GetMember
    {
        public readonly Address Address;

        public GetMember(Address address)
        {
            Address = address;
        }
    }

    public class JoinMember
    {
        public readonly Address Address;

        public JoinMember(Address address)
        {
            Address = address;
        }
    }

    public class DownMember
    {
        public readonly Address Address;

        public DownMember(Address address)
        {
            Address = address;
        }
    }

    public class LeaveMember
    {
        public readonly Address Address;

        public LeaveMember(Address address)
        {
            Address = address;
        }
    }

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

        public ClusterMembers(string selfNode, ClusterMember[] members = null, ClusterUnreachableMember[] unreachable = null)
        {
            SelfNode = selfNode;
            Members = members;
            Unreachable = unreachable;
        }
    }

    public class ClusterHttpManagementMessage
    {
        public readonly string Message;

        public ClusterHttpManagementMessage(string message)
        {
            Message = message;
        }
    }

    public abstract class Complete
    {
        public class Success : Complete
        {
            public readonly object Result;

            public Success(object result)
            {
                Result = result;
            }
        }

        public class Failure : Complete
        {
            public readonly string Reason;

            public Failure(string reason = null)
            {
                Reason = reason;
            }
        }
    }

    public class ClusterHttpManagementRoutes : UntypedActor
    {
        private readonly Cluster cluster;

        public static Props Props(Cluster cluster) =>
            Actor.Props.Create(() => new ClusterHttpManagementRoutes(cluster));

        public ClusterHttpManagementRoutes(Cluster cluster)
        {
            this.cluster = cluster;
        }

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<GetMembers>(_ =>
                {
                    var members = cluster.ReadView.State.Members.Select(MemberToClusterMember);
                    var unreachable = cluster.ReadView.Reachability.ObserversGroupedByUnreachable.Select(pair =>
                    {
                        return new ClusterUnreachableMember(pair.Key.Address.ToString(), pair.Value.Select(address => address.Address.ToString()).ToArray());
                    });

                    Sender.Tell(new Complete.Success(new ClusterMembers(cluster.SelfAddress.ToString(), members.ToArray(), unreachable.ToArray())));
                })
                .With<GetMember>(msg =>
                {
                    var member = cluster.ReadView.Members.SingleOrDefault(m => m.Address == msg.Address);
                    if (member != null)
                    {
                        Sender.Tell(new Complete.Success(MemberToClusterMember(member)));
                        return;
                    }

                    Sender.Tell(new Complete.Failure($"Member {msg.Address.ToString()} not found"));
                })
                .With<JoinMember>(msg =>
                {
                    cluster.Join(msg.Address);
                    Sender.Tell(new Complete.Success($"Joining {msg.Address.ToString()}"));
                })
                .With<DownMember>(msg =>
                {
                    var member = cluster.ReadView.Members.SingleOrDefault(m => m.Address == msg.Address);
                    if (member != null)
                    {
                        cluster.Down(msg.Address);
                        Sender.Tell(new Complete.Success($"Downing {msg.Address.ToString()}"));
                        return;
                    }

                    Sender.Tell(new Complete.Failure($"Member {msg.Address.ToString()} not found"));
                })
                .With<LeaveMember>(msg =>
                {
                    var member = cluster.ReadView.Members.SingleOrDefault(m => m.Address == msg.Address);
                    if (member != null)
                    {
                        cluster.Leave(msg.Address);
                        Sender.Tell(new Complete.Success($"Leaving {msg.Address.ToString()}"));
                        return;
                    }

                    Sender.Tell(new Complete.Failure($"Member {msg.Address.ToString()} not found"));
                });
        }

        private static ClusterMember MemberToClusterMember(Member member) =>
            new ClusterMember(member.UniqueAddress.Address.ToString(), member.UniqueAddress.Uid.ToString(), member.Status.ToString(), member.Roles.ToArray());
    }
}
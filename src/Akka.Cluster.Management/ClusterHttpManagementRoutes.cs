using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Akka.Actor;
using Akka.Cluster.Sharding;

namespace Akka.Cluster.Management
{
    #region Messages

    public class GetMembers
    { }

    public class GetMember
    {
        public readonly Address Address;

        public GetMember(Address address) => Address = address;
    }

    public class JoinMember
    {
        public readonly Address Address;

        public JoinMember(Address address) => Address = address;
    }

    public class DownMember
    {
        public readonly Address Address;

        public DownMember(Address address) => Address = address;
    }

    public class LeaveMember
    {
        public readonly Address Address;

        public LeaveMember(Address address) => Address = address;
    }

    public class GetShardInfo
    {
        public readonly string Name;

        public GetShardInfo(string name) => Name = name;
    }

    public abstract class Complete
    {
        public class Success : Complete
        {
            public readonly object Result;

            public Success(object result) => Result = result;
        }

        public class Failure : Complete
        {
            public readonly string Reason;

            public Failure(string reason = null) => Reason = reason;
        }
    }

    #endregion

    public class ClusterHttpManagementRoutes : UntypedActor
    {
        private readonly Cluster cluster;

        public static Props Props(Cluster cluster) =>
            Actor.Props.Create(() => new ClusterHttpManagementRoutes(cluster));

        public ClusterHttpManagementRoutes(Cluster cluster) => this.cluster = cluster;

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case GetMembers:
                    {
                        dynamic c = new Dynamitey.DynamicObjects.Get(cluster);
                        dynamic readView = new Dynamitey.DynamicObjects.Get(c.ReadView);
                        dynamic reachability = new Dynamitey.DynamicObjects.Get(readView.Reachability);
                        dynamic state = new Dynamitey.DynamicObjects.Get(readView.State);

                        var members = ((IEnumerable)readView.State.Members).Cast<dynamic>()
                            .Select(m => MemberToClusterMember((Member)m));

                        var unreachable = ((IEnumerable)reachability.ObserversGroupedByUnreachable).Cast<dynamic>()
                            .Select(o =>
                            {
                                var pair = (KeyValuePair<UniqueAddress, ImmutableHashSet<UniqueAddress>>)o;
                                return new ClusterUnreachableMember(pair.Key.Address.ToString(), pair.Value.Select(address => address.Address.ToString()).ToArray());
                            });

                        var leader = ((Address)state.Leader).ToString();
                        var oldest = cluster.State.Members.Where(node => node.Status == MemberStatus.Up)
                            .OrderBy(member => member, Member.AgeOrdering)
                            .Select(m => m.Address.ToString())
                            .Last(); // we are only interested in the oldest one that is still Up

                        Sender.Tell(new Complete.Success(new ClusterMembers(cluster.SelfAddress.ToString(), members.ToArray(), unreachable.ToArray(), leader, oldest)));
                        break;
                    }
                case GetMember msg:
                    {
                        dynamic c = new Dynamitey.DynamicObjects.Get(cluster);
                        dynamic readView = new Dynamitey.DynamicObjects.Get(c.ReadView);

                        var member = ((IEnumerable)readView.Members).Cast<dynamic>().SingleOrDefault(m => ((Member)m).Address == msg.Address);
                        if (member != null)
                        {
                            Sender.Tell(new Complete.Success(MemberToClusterMember(member)));
                            return;
                        }

                        Sender.Tell(new Complete.Failure($"Member {msg.Address} not found"));
                        break;
                    }
                case JoinMember msg:
                    {
                        cluster.Join(msg.Address);
                        Sender.Tell(new Complete.Success($"Joining {msg.Address}"));
                        break;
                    }
                case DownMember msg:
                    {
                        dynamic c = new Dynamitey.DynamicObjects.Get(cluster);
                        dynamic readView = new Dynamitey.DynamicObjects.Get(c.ReadView);

                        var member = ((IEnumerable)readView.Members).Cast<dynamic>().SingleOrDefault(m => ((Member)m).Address == msg.Address);
                        if (member != null)
                        {
                            cluster.Down(msg.Address);
                            Sender.Tell(new Complete.Success($"Downing {msg.Address}"));
                            return;
                        }

                        Sender.Tell(new Complete.Failure($"Member {msg.Address} not found"));
                        break;
                    }
                case LeaveMember msg:
                    {
                        dynamic c = new Dynamitey.DynamicObjects.Get(cluster);
                        dynamic readView = new Dynamitey.DynamicObjects.Get(c.ReadView);

                        var member = ((IEnumerable)readView.Members).Cast<dynamic>().SingleOrDefault(m => ((Member)m).Address == msg.Address);
                        if (member != null)
                        {
                            cluster.Leave(msg.Address);
                            Sender.Tell(new Complete.Success($"Leaving {msg.Address}"));
                            return;
                        }

                        Sender.Tell(new Complete.Failure($"Member {msg.Address} not found"));
                        break;
                    }
                case GetShardInfo shardInfo:
                    {
                        try
                        {
                            ClusterSharding.Get(cluster.System)
                                .ShardRegion(shardInfo.Name)
                                .Ask<ShardRegionStats>(GetShardRegionStats.Instance)
                                .PipeTo(Sender, success: stats => new Complete.Success(new ShardDetails(stats.Stats.Select(stat => new ShardRegionInfo(stat.Key, stat.Value)).ToArray())));
                        }
                        catch (AskTimeoutException)
                        {
                            Sender.Tell(new Complete.Failure($"Shard Region [{shardInfo.Name}] not responding, may have been terminated"));
                        }
                        catch (Exception)
                        {
                            Sender.Tell(new Complete.Failure($"Shard type [{shardInfo.Name}] must be started first"));
                        }
                        break;
                    }
            }
        }

        private static ClusterMember MemberToClusterMember(Member member) =>
            new(member.UniqueAddress.Address.ToString(), member.UniqueAddress.Uid.ToString(), member.Status.ToString(), member.Roles.ToArray());
    }

    public class MemberExt
    {
        /// <summary>
        /// Compares members by their upNumber to determine which is oldest / youngest.
        /// </summary>
        public static readonly AgeComparer AgeOrdering = new();

        public class AgeComparer : IComparer<Member>
        {
            public int Compare(Member x, Member y)
            {
                if (x != null && x.Equals(y)) return 0;
                if (x != null && x.IsOlderThan(y)) return -1;
                return 1;
            }
        }
    }
}
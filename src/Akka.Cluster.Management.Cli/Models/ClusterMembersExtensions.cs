using System.Linq;

namespace Akka.Cluster.Management.Cli.Models
{
    public static class ClusterMembersExtensions
    {
        public static string[] CuratedUnreachable(this ClusterMembers members)
        {
            var unreachable = members.Unreachable.Select(u => u.Node).ToArray();
            return members.Unreachable
                .Select(current => new
                {
                    current,
                    count = current.ObservedBy.Count(node => !unreachable.Contains(node)) // filter out observed-by other unreachable nodes
                })
                .Where(t =>
                {
                    var minQuorumMembers = members.Members.Length / 2;
                    return minQuorumMembers <= 1 || t.count >= 1;
                })
                .Select(t => t.current.Node).ToArray();
        }
    }
}

using System;
using System.Linq;

namespace Akka.Cluster.Management.Cli.Models
{
    public static class ClusterMembersExtensions
    {
        public static string[] CuratedUnreachable(this ClusterMembers members)
        {
            var unreachable = members.Unreachable.Select(u => u.Node).ToArray();
            return members.Unreachable
                .Select(current => new { current, count = current.ObservedBy.Count(node => !unreachable.Contains(node)) })
                .Where(t => t.count >= Math.Min(5, Math.Min(1, members.Members.Length / 2 + 1)))
                .Select(t => t.current.Node).ToArray();
        }
    }
}

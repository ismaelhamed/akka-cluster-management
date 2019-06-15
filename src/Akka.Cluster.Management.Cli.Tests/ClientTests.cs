using Akka.Cluster.Management.Cli.Models;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Akka.Cluster.Management.Cli.Tests
{
    public class ClientTests
    {
        [Fact]
        public void Curate_Unreachable_Nodes_On_ClusterStatus_Based_On_The_Observability_Collection()
        {
            // Arrange
            var result = JsonConvert.DeserializeObject<ClusterMembers>(File.ReadAllText(@"stubs\members.json"));

            // Act
            var unreachable = result.Unreachable.Select(u => u.Node).ToArray();

            var curated = result.Unreachable
                .Select(current => new { current, count = current.ObservedBy.Count(node => !unreachable.Contains(node)) })
                .Where(t => t.count >= Math.Min(5, result.Members.Length / 2 + 1))
                .Select(t => t.current.Node).ToList();

            var members = result.Members.Select(re => curated.Contains(re.Node)
                ? new { re.Node, Status = "Unreachable", Roles = string.Join(", ", re.Roles), Leader = re.Node == result.Leader ? "(leader)" : string.Empty }
                : new { re.Node, re.Status, Roles = string.Join(", ", re.Roles), Leader = re.Node == result.Leader ? "(leader)" : string.Empty });

            // Assert
            unreachable.Should().NotBeEquivalentTo(curated);
        }

        [Fact]
        public void Curate_Unreachable_Nodes_Based_On_The_Observability_Collection()
        {
            // Arrange
            var result = JsonConvert.DeserializeObject<ClusterMembers>(File.ReadAllText(@"stubs\members.json"));

            // Act
            var unreachable = result.Unreachable.Select(u => u.Node).ToArray();
            var curated = result.Unreachable
                .Select(current => new { current, count = current.ObservedBy.Count(node => !unreachable.Contains(node)) })
                .Where(t => t.count >= Math.Min(5, result.Members.Length / 2 + 1))
                .Select(t => t.current.Node).ToArray();

            // Assert
            unreachable.Should().NotBeEquivalentTo(curated);
        }
    }
}

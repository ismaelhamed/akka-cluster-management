using System.IO;
using System.Linq;
using Akka.Cluster.Management.Cli.Models;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Akka.Cluster.Management.Cli.Tests
{
    public class ClientTests
    {
        [Fact]
        public void Curated_nodes_based_on_the_observability_should_not_be_equivalent_to_the_actual_unreachable()
        {
            // Arrange
            var result = JsonConvert.DeserializeObject<ClusterMembers>(File.ReadAllText(@"stubs\members.json"));

            // Act
            var unreachable = result.Unreachable.Select(u => u.Node).ToArray();
            var curated = result.CuratedUnreachable();

            // Assert
            unreachable.Should().NotBeEquivalentTo(curated);
        }

        [Fact]
        public void Curated_nodes_based_on_the_observability_should_work_for_simple_clusters()
        {
            // Arrange
            var result = JsonConvert.DeserializeObject<ClusterMembers>(File.ReadAllText(@"stubs\members_sigle_node.json"));

            // Act
            var unreachable = result.Unreachable.Select(u => u.Node).ToArray();
            var curated = result.CuratedUnreachable();

            // Assert
            unreachable.Should().BeEquivalentTo(curated);
        }
    }
}

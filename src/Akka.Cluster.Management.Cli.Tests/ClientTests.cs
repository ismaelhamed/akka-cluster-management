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
            curated.Should().HaveCountGreaterThan(0);
            unreachable.Should().NotBeEquivalentTo(curated);
        }

        [Fact]
        public void Curated_nodes_based_on_the_observability_should_have_specific_count()
        {
            // Arrange
            var result = JsonConvert.DeserializeObject<ClusterMembers>(File.ReadAllText(@"stubs\members_complex.json"));

            // Act
            var unreachable = result.Unreachable.Select(u => u.Node).ToArray();
            var curated = result.CuratedUnreachable();

            // Assert
            curated.Should().HaveCount(3);
            unreachable.Should().NotBeEquivalentTo(curated);
        }

        [Theory]
        [InlineData(@"stubs\members_single_node_cluster.json")]
        [InlineData(@"stubs\members_two_node_cluster.json")]
        public void Curated_nodes_based_on_the_observability_should_be_equivalent_to_the_actual_unreachable_in_small_clusters(string filePath)
        {
            // Arrange
            var result = JsonConvert.DeserializeObject<ClusterMembers>(File.ReadAllText(filePath));

            // Act
            var unreachable = result.Unreachable.Select(u => u.Node).ToArray();
            var curated = result.CuratedUnreachable();

            // Assert
            unreachable.Should().BeEquivalentTo(curated);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Akka.Cluster.Management.Cli.Models;
using Akka.Cluster.Management.Cli.Utils;
using Newtonsoft.Json;
using Serilog;
using Console = Colorful.Console;

namespace Akka.Cluster.Management.Cli
{
    internal class ClusterClient
    {
        internal static int JoinMember(string scheme, string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var parameters = new Dictionary<string, string> { { "address", nodeUrl } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    var response = await client.PostAsync("/cluster/members", encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterHttpManagementMessage>(data);
                        Console.WriteLine(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        internal static int DownMember(string scheme, string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var parameters = new Dictionary<string, string> { { "address", nodeUrl }, { "operation", "down" } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    var response = await client.PutAsync("/cluster/members", encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterHttpManagementMessage>(data);
                        Console.WriteLine(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        internal static int LeaveMember(string scheme, string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var parameters = new Dictionary<string, string> { { "address", nodeUrl }, { "operation", "leave" } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    var response = await client.PutAsync("/cluster/members", encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterHttpManagementMessage>(data);
                        Console.WriteLine(result.Message);
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        internal static int GetClusterStatus(string scheme, string hostname, int port, bool useFilter = false) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient()
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync("/cluster/members");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterMembers>(data);

                        var unreachable = useFilter
                            ? result.CuratedUnreachable()
                            : result.Unreachable.Select(u => u.Node).ToArray();

                        var members = result.Members.Select(re => unreachable.Contains(re.Node)
                            ? new { re.Node, Status = "Unreachable", Roles = string.Join(", ", re.Roles), Leader = re.Node == result.Leader ? "(leader)" : string.Empty }
                            : new { re.Node, re.Status, Roles = string.Join(", ", re.Roles), Leader = re.Node == result.Leader ? "(leader)" : string.Empty });

                        var table = new ConsoleTable(new[] { "NODE", "STATUS", "ROLES", "" }, new ConsoleTableSettings());
                        members.ToList().ForEach(member => table.AddRow(new[] { member.Node, member.Status, member.Roles, member.Leader }));
                        table.WriteToConsole();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        internal static int GetMemberStatus(string scheme, string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync($"/cluster/members/?address={nodeUrl}");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterMember>(data);

                        var table = new ConsoleTable(new[] { "NODE", "STATUS", "ROLES" }, new ConsoleTableSettings());
                        table.AddRow(new[] { result.Node, result.Status, string.Join(", ", result.Roles) });
                        table.WriteToConsole();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        internal static int GetMembers(string scheme, string hostname, int port) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync("/cluster/members");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterMembers>(data);

                        var table = new ConsoleTable(new[] { "NODE" }, new ConsoleTableSettings());
                        Array.ForEach(result.Members, member => table.AddRow(new[] { member.Node }));
                        table.WriteToConsole();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        internal static int GetUnreachable(string scheme, string hostname, int port, bool useFilter = false) =>
            Execute(async () =>
            {
                try
                {
                    using var client = new HttpClient
                    {
                        BaseAddress = new Uri($"{scheme}://{hostname}:{port}")
                    };
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync("/cluster/members");
                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<ClusterMembers>(data);

                        var unreachable = useFilter
                            ? result.CuratedUnreachable()
                            : result.Unreachable.Select(u => u.Node).ToArray();

                        if (unreachable.Any())
                        {
                            var table = new ConsoleTable(new[] { "NODE" }, new ConsoleTableSettings());
                            Array.ForEach(result.Unreachable, member => table.AddRow(new[] { member.Node }));
                            table.WriteToConsole();
                        }
                        else
                        {
                            Console.WriteLine("All nodes seem to be Up :)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int Execute(Func<Task> action)
        {
            try
            {
                action().Wait();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}

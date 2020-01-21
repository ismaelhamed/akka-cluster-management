using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Akka.Cluster.Management.Cli.Models;
using Akka.Cluster.Management.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Serilog;
using Console = Colorful.Console;

namespace Akka.Cluster.Management.Cli
{
    internal static class Program
    {
        private static readonly string DefaultHostname = ConfigurationManager.AppSettings["hostname"] ?? "127.0.0.1";
        private static readonly string DefaultPort = ConfigurationManager.AppSettings["port"] ?? "19999";

        private static int Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, __) => cancellationTokenSource.Cancel();

            // Set up logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "akka-cluster",
                FullName = "Akka Management Cluster HTTP",
                ShortVersionGetter = () => "0.7.2",
                ExtendedHelpText = @"
Examples: 
  akka-cluster cluster-status
  akka-cluster down <node-url>

Where the <node-url> should be on the format of
  'akka.<protocol>://<actor-system-name>@<hostname>:<port>'
            "
            };

            app.Command("join", command =>
            {
                command.Description = "Sends request a JOIN node with the specified URL";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return JoinMember(nodeHostname, nodePort, nodeUrl);
                });
            });

            app.Command("leave", command =>
            {
                command.Description = "Sends a request for node with URL to LEAVE the cluster";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return LeaveMember(nodeHostname, nodePort, nodeUrl);
                });
            });

            app.Command("down", command =>
            {
                command.Description = "Sends a request for marking node with URL as DOWN";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return DownMember(nodeHostname, nodePort, nodeUrl);
                });
            });

            app.Command("cluster-status", command =>
            {
                command.Description = "Asks the cluster for its current status (member ring, unavailable nodes, meta data, etc.)";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);
                var curateArgument = command.Option("--curate", "Simple best-effort attempt to curate the unreachable nodes", CommandOptionType.NoValue);

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);
                    var useFilter = curateArgument.HasValue();

                    return GetClusterStatus(nodeHostname, nodePort, useFilter);
                });
            });

            app.Command("member-status", command =>
            {
                command.Description = "Asks the member node for its current status";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return GetMemberStatus(nodeHostname, nodePort, nodeUrl);
                });
            });

            app.Command("members", command =>
            {
                command.Description = "Asks the cluster for addresses of current members";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);

                    return GetMembers(nodeHostname, nodePort);
                });
            });

            app.Command("unreachable", command =>
            {
                command.Description = "Asks the cluster for addresses of unreachable members";

                var nodeHostnameArgument = command.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
                var nodePortArgument = command.Option("--port <node-port>", "", CommandOptionType.SingleValue);
                var curateArgument = command.Option("--curate", "Simple best-effort attempt to curate the unreachable nodes", CommandOptionType.NoValue);

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);
                    var useFilter = curateArgument.HasValue();

                    return GetUnreachable(nodeHostname, nodePort, useFilter);
                });
            });

            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
                app.DisplayAsciiArt();
                app.ShowHelp();
                return 1;
            });

            // invalid args syntax
            return app.Execute(args);
        }

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

        private static int JoinMember(string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int DownMember(string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int LeaveMember(string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int GetClusterStatus(string hostname, int port, bool useFilter = false) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int GetMemberStatus(string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int GetMembers(string hostname, int port) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });

        private static int GetUnreachable(string hostname, int port, bool useFilter = false) =>
            Execute(async () =>
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri($"http://{hostname}:{port}");
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
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Something went wrong");
                    Console.WriteLine("Something went wrong");
                }
            });
    }
}

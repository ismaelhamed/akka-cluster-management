using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Akka.Cluster.Management.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;

namespace Akka.Cluster.Management.Cli
{
    internal class Program
    {
        private const string DefaultHostname = "127.0.0.1";
        private const string DefaultPort = "19999";

        private static int Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, __) => cancellationTokenSource.Cancel();

            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "akka-cluster",
                FullName = "Akka Cluster Http Management CLI",
                ShortVersionGetter = () => "0.2.0",
                ExtendedHelpText = @"
Examples: 
    akka-cluster join <node-url>
    akka-cluster members

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

                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var nodeHostname = nodeHostnameArgument.Value() ?? DefaultHostname;
                    var nodePort = int.Parse(nodePortArgument.Value() ?? DefaultPort);

                    return GetUnreachable(nodeHostname, nodePort);
                });
            });

            app.HelpOption("-?|-h|--help");
            app.OnExecute(() =>
            {
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
                Console.WriteLine($"{hostname} is JOINING cluster node {nodeUrl}");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostname}:{port}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var parameters = new Dictionary<string, string> { { "address", nodeUrl } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    var response = await client.PostAsync("/members", encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<Models.ClusterHttpManagementMessage>();
                        Console.WriteLine(result.Message);
                    }

                    Console.WriteLine("Something went wrong.");
                }
            });

        private static int DownMember(string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                Console.WriteLine($"Marking {nodeUrl} as DOWN");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostname}:{port}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var parameters = new Dictionary<string, string> { { "address", nodeUrl }, { "operation", "down" } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    var response = await client.PutAsync("/members", encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<Models.ClusterHttpManagementMessage>();
                        Console.WriteLine(result.Message);
                    }

                    Console.WriteLine("Something went wrong.");
                }
            });

        private static int LeaveMember(string hostname, int port, string nodeUrl) =>
            Execute(async () =>
            {
                Console.WriteLine($"Scheduling {nodeUrl} to LEAVE cluster");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostname}:{port}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var parameters = new Dictionary<string, string> { { "address", nodeUrl }, { "operation", "leave" } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    var response = await client.PutAsync("/members", encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<Models.ClusterHttpManagementMessage>();
                        Console.WriteLine(result.Message);
                    }
                }
            });

        private static int GetMembers(string hostname, int port) =>
            Execute(async () =>
            {
                Console.WriteLine("Querying members");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostname}:{port}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync("/members");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<Models.ClusterMembers>();

                        var members = result.Unreachable.Select(member => new Tuple<string, string, string>(member.Node, "Unreachable", "")).ToList();
                        foreach (var re in result.Members.Select(member => new Tuple<string, string, string>(member.Node, member.Status, string.Join(",", member.Roles))))
                        {
                            if (members.FirstOrDefault(m => m.Item1 == re.Item1) == null)
                                members.Add(new Tuple<string, string, string>(re.Item1, re.Item2, re.Item3));
                        }

                        var table = new ConsoleTable(new[] { "Node", "Status", "Roles" }, new ConsoleTableSettings());
                        members.OrderBy(m => m.Item1).ToList().ForEach(member => table.AddRow(new[] { member.Item1, member.Item2, member.Item3 }));
                        table.WriteToConsole();
                    }
                }
            });

        private static int GetUnreachable(string hostname, int port) =>
            Execute(async () =>
            {
                Console.WriteLine("Querying unreachable members");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"http://{hostname}:{port}");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync("/members");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsAsync<Models.ClusterMembers>();
                        if (result.Unreachable.Any())
                        {
                            var table = new ConsoleTable(new[] { "Node" }, new ConsoleTableSettings());
                            Array.ForEach(result.Unreachable, member => table.AddRow(new[] { member.Node }));
                            table.WriteToConsole();
                        }
                        else
                        {
                            Console.WriteLine("All nodes seem to be Up :)");
                        }
                    }
                }
            });
    }
}

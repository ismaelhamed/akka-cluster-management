using System.Configuration;
using System.Reflection;
using System.Threading;
using Akka.Cluster.Management.Cli.Utils;
using Microsoft.Extensions.CommandLineUtils;
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
                ShortVersionGetter = () => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version,
                ExtendedHelpText = @"
Where the <node-url> should be on the format of
  'akka.<protocol>://<actor-system-name>@<hostname>:<port>'

Examples: .\akka-cluster --hostname localhost --port 19999 unreachable
          .\akka-cluster --hostname localhost --port 19999 down akka.tcp://MySystem@darkstar:2552
          .\akka-cluster --hostname localhost --port 19999 cluster-status
            "
            };

            var hostNameArgument = app.Option("--hostname <node-hostname>", "", CommandOptionType.SingleValue);
            var portArgument = app.Option("--port <node-port>", "", CommandOptionType.SingleValue);

            app.Command("join", command =>
            {
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.Description = "Sends request a JOIN node with the specified URL";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return ClusterClient.JoinMember(hostName, port, nodeUrl);
                });
            });

            app.Command("leave", command =>
            {
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.Description = "Sends a request for node with URL to LEAVE the cluster";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return ClusterClient.LeaveMember(hostName, port, nodeUrl);
                });
            });

            app.Command("down", command =>
            {
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.Description = "Sends a request for marking node with URL as DOWN";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return ClusterClient.DownMember(hostName, port, nodeUrl);
                });
            });

            app.Command("cluster-status", command =>
            {
                var curateArgument = command.Option("--curate", "Simple best-effort attempt to curate the unreachable nodes", CommandOptionType.NoValue);

                command.Description = "Asks the cluster for its current status (member ring, unavailable nodes, meta data, etc.)";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);
                    var useFilter = curateArgument.HasValue();

                    return ClusterClient.GetClusterStatus(hostName, port, useFilter);
                });
            });

            app.Command("member-status", command =>
            {
                var nodeUrlArgument = command.Argument("[node-url]", "");

                command.Description = "Asks the member node for its current status";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);
                    var nodeUrl = nodeUrlArgument.Value;

                    return ClusterClient.GetMemberStatus(hostName, port, nodeUrl);
                });
            });

            app.Command("members", command =>
            {
                command.Description = "Asks the cluster for addresses of current members";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);

                    return ClusterClient.GetMembers(hostName, port);
                });
            });

            app.Command("unreachable", command =>
            {
                var curateArgument = command.Option("--curate", "Simple best-effort attempt to curate the unreachable nodes", CommandOptionType.NoValue);

                command.Description = "Asks the cluster for addresses of unreachable members";
                command.HelpOption("-?|-h|--help");
                command.OnExecute(() =>
                {
                    var hostName = hostNameArgument.Value() ?? DefaultHostname;
                    var port = int.Parse(portArgument.Value() ?? DefaultPort);
                    var useFilter = curateArgument.HasValue();

                    return ClusterClient.GetUnreachable(hostName, port, useFilter);
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

        //public string ShowVersion()
        //{
        //    var version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        //}
    }
}

using Topshelf;

namespace Akka.Cluster.Management.Host
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            return (int)HostFactory.Run(configurator =>
            {
                configurator.Service<Startup>(s =>
                {
                    s.ConstructUsing(() => new Startup());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });
                configurator.RunAsLocalSystem();
                configurator.SetServiceName("Akka Cluster Http Management");
                configurator.SetDisplayName("Akka Cluster Http Management");
                configurator.SetDescription("Akka Cluster Http Management allows you interaction with an Akka cluster through an HTTP interface.");
            });
        }
    }
}

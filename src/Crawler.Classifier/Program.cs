using System.Runtime.InteropServices;
using Crawler.Classifier;
using Microsoft.Extensions.Configuration;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

HostFactory.Run(h =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        h.UseEnvironmentBuilder(target => new DotNetCoreEnvironmentBuilder(target));

    h.Service<ClassifierService>(s =>
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json", false);

        IConfiguration config = builder.Build();


        s.ConstructUsing(_ => new ClassifierService("classified-service", config));
        s.WhenStarted(tc => tc.Start(null));
        s.WhenStopped(tc => tc.Stop(null));
    });
    h.RunAsLocalSystem();
    h.SetDescription("Classifier Service Description");
    h.SetDisplayName("Classifier Service");
    h.SetServiceName("ClassifierService");
});
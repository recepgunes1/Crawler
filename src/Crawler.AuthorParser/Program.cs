using System.Runtime.InteropServices;
using Crawler.AuthorParser;
using Microsoft.Extensions.Configuration;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

HostFactory.Run(h =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        h.UseEnvironmentBuilder(target => new DotNetCoreEnvironmentBuilder(target));

    h.Service<AuthorParserService>(s =>
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json", false);

        IConfiguration config = builder.Build();

        s.ConstructUsing(_ => new AuthorParserService("author-parsed-service", config));
        s.WhenStarted(tc => tc.Start(null));
        s.WhenStopped(tc => tc.Stop(null));
    });
    h.RunAsLocalSystem();
    h.SetDescription("Author Parser Service Description");
    h.SetDisplayName("Author Parser Service");
    h.SetServiceName("AuthorParserService");
});
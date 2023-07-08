using System.Runtime.InteropServices;
using Crawler.BookParser;
using Microsoft.Extensions.Configuration;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

HostFactory.Run(h =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        h.UseEnvironmentBuilder(target => new DotNetCoreEnvironmentBuilder(target));

    h.Service<BookParserService>(s =>
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json", false);

        IConfiguration config = builder.Build();

        s.ConstructUsing(_ => new BookParserService("book-parsed-service", config));
        s.WhenStarted(tc => tc.Start(null));
        s.WhenStopped(tc => tc.Stop(null));
    });
    h.RunAsLocalSystem();
    h.SetDescription("Book Parser Service Description");
    h.SetDisplayName("Book Parser Service");
    h.SetServiceName("BookParserService");
});
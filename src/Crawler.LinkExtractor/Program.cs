using System.Runtime.InteropServices;
using Crawler.LinkExtractor;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

HostFactory.Run(h =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        h.UseEnvironmentBuilder(target => new DotNetCoreEnvironmentBuilder(target));
    }

    h.Service<LinkExtractorService>(s =>
    {
        s.ConstructUsing(_ => new LinkExtractorService("link-extracted-service"));
        s.WhenStarted(tc => tc.Start(null));
        s.WhenStopped(tc => tc.Stop(null));
    });
    h.RunAsLocalSystem();
    h.SetDescription("Link Extractor Service Description");
    h.SetDisplayName("Link Extractor Service");
    h.SetServiceName("LinkExtractorService");
});
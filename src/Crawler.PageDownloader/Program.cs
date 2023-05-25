using System.Runtime.InteropServices;
using Crawler.PageDownloader;
using Topshelf;
using Topshelf.Runtime.DotNetCore;

HostFactory.Run(h =>
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        h.UseEnvironmentBuilder(target => new DotNetCoreEnvironmentBuilder(target));
    }

    h.Service<PageDownloaderService>(s =>
    {
        s.ConstructUsing(_ => new PageDownloaderService("page-downloaded-service"));
        s.WhenStarted(tc => tc.Start(null));
        s.WhenStopped(tc => tc.Stop(null));
    });
    h.RunAsLocalSystem();
    h.SetDescription("Page Downloader Service Description");
    h.SetDisplayName("Page Downloader Service");
    h.SetServiceName("PageDownloaderService");
});

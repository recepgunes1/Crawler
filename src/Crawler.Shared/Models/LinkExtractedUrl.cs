namespace Crawler.Shared.Models;

public class LinkExtractedUrl
{
    public string LinkId { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string SourceCode { get; set; } = null!;
}
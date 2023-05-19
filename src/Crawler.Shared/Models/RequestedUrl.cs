namespace Crawler.Shared.Models;

public class RequestedUrl
{
    public string Url { get; set; } = null!;
    public DateTime RequestedDatetime { get; set; } = DateTime.UtcNow;
}
namespace Crawler.Data.Entities;

public class Link
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SourceId { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}
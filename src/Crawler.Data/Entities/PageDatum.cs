namespace Crawler.Data.Entities;

public class PageDatum
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LinkId { get; set; } = null!;
    public string SourceCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}
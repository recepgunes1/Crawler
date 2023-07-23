namespace Crawler.Data.Entities;

public class Author
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LinkId { get; set; } = null!;
    public string? ImageLink { get; set; }
    public string? FullName { get; set; }
    public string? Description { get; set; }
    public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
}
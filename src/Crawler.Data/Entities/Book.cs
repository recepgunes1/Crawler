namespace Crawler.Data.Entities;

public class Book
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? ImageLink { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? Description { get; set; }
    public string? Isbn { get; set; }
    public string? Pages { get; set; }
    public string? Category { get; set; }
}
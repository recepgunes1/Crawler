using Newtonsoft.Json;

namespace Crawler.BookParser.JsonModels;

public class JsonBook
{
    [JsonProperty("@context")] public string Context { get; set; } = null!;

    [JsonProperty("@type")] public string Type { get; set; } = null!;

    public string Name { get; set; } = null!; //
    public string Image { get; set; } = null!; //
    public string Isbn { get; set; } = null!;
    public string NumberOfPages { get; set; } = null!;
    public Person Author { get; set; } = null!;
    public Organization Publisher { get; set; } = null!;
    public string Description { get; set; } = null!;
}
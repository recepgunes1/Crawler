using Newtonsoft.Json;

namespace Crawler.BookParser.JsonModels;

public class Organization
{
    [JsonProperty("@type")] public string Type { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
}
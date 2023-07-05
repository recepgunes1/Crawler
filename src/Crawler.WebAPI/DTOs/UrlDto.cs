using System.ComponentModel;

namespace Crawler.WebAPI.DTOs;

public class UrlDto
{
    [DefaultValue("https://www.kitapyurdu.com/")]
    public string Url { get; set; } = null!;
}
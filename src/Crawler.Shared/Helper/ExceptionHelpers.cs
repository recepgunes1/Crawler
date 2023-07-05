using System.Text.Json;

namespace Crawler.Shared.Helper;

public static class ExceptionHelpers
{
    public static string ToJsonString<T>(this T exception)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(exception, options);
        return jsonString;
    }
}
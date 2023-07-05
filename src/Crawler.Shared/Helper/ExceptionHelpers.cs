using System.Text.Json;
using MassTransit;

namespace Crawler.Shared.Helper;

public static class ExceptionHelpers
{
    public static string ToJsonString(this Exception exception)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(exception, options);
        return jsonString;
    }

    public static string ToJsonString(this ExceptionInfo exception)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(exception, options);
        return jsonString;
    }
}
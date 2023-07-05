using Nager.PublicSuffix;

namespace Crawler.Shared.Helper;

public static class UrlHelpers
{
    private static readonly DomainParser DomainParser = new(new WebTldRuleProvider());

    public static string GetRegistrableDomain(string url)
    {
        var domainName = DomainParser.Parse(url);
        return domainName.RegistrableDomain;
    }

    public static string GetSubDomain(string url)
    {
        var domainName = DomainParser.Parse(url);
        return domainName.SubDomain;
    }

    public static bool IsValidUrl(string url)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                     && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        return result;
    }
}
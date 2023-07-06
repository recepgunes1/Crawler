using System.Text.RegularExpressions;
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

    public static string RemoveSpaces(this string url)
    {
        return Regex.Replace(url, @"\s+", string.Empty);
    }
}
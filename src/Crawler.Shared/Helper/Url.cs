using Nager.PublicSuffix;

namespace Crawler.Shared.Helper;

public static class Url
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

}
using Nager.PublicSuffix;

namespace Crawler.LinkExtractor;

public static class Helper
{
    public static string GetRegistrableDomain(string url)
    {
        var domainParser = new DomainParser(new WebTldRuleProvider());
        var domainName = domainParser.Parse(url);
        return domainName.RegistrableDomain;
    }

}
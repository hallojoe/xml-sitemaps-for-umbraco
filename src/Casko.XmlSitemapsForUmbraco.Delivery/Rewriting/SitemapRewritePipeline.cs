using Casko.XmlSitemapsForUmbraco.Common.Configuration;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;

public static class SitemapRewritePipeline
{
    public static bool ShouldRegister(XmlSitemapsOptions options)
    {
        return options.RewritesEnabled;
    }
}

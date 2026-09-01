using Microsoft.AspNetCore.Http;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;

public interface ISitemapRewriteDefinitionService
{
    public IReadOnlyCollection<SitemapRewriteDefinition> GetDefinitions();

    public bool TryMatch(PathString requestPath, HostString requestHost, out SitemapRewriteDefinition? definition);
}
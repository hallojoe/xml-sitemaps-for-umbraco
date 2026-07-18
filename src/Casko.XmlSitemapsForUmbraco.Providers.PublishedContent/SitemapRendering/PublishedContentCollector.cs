using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentCollector : IPublishedContentCollector
{
    public IEnumerable<IPublishedContent> Collect(PublishedContentRenderContext context)
    {
        return context.RootContents
            .SelectMany(rootContent => rootContent.Descendants().Prepend(rootContent));
    }
}
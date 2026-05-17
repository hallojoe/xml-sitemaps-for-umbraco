using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class XmlSitemapContentCollector : IXmlSitemapContentCollector
{
    public IEnumerable<IPublishedContent> Collect(XmlSitemapRenderContext context)
    {
        return context.RootContents
            .SelectMany(rootContent => rootContent.Descendants().Prepend(rootContent));
    }
}
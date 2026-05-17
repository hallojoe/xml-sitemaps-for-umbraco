using Casko.XmlSitemapsForUmbraco.Common.Exceptions;
using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class XmlSitemapRenderer(
    IXmlSitemapContentCollector contentCollector,
    IXmlSitemapUrlRenderer urlRenderer) : IXmlSitemapRenderer
{
    public XmlSiteMap Render(XmlSitemapRenderContext context)
    {
        var includedContentItems = contentCollector
            .Collect(context)
            .Where(content => context.ShouldIncludeContent?.Invoke(content) is not false)
            .ToList();

        if (includedContentItems.Count == 0)
        {
            throw new RootContentHasNoContentException();
        }

        var urlContext = new XmlSitemapUrlRenderContext(
            context.DefaultLanguageCode,
            context.AlternativeLanguageCodes,
            context.Hostname,
            context.RenderAlternateLinks);

        var urlsByLocation = new Dictionary<string, XmlSiteMapUrl>(StringComparer.OrdinalIgnoreCase);
        foreach (var sitemapUrl in includedContentItems.Select(content => urlRenderer.Render(content, urlContext)))
        {
            urlsByLocation[sitemapUrl.Location] = sitemapUrl;
        }

        return new XmlSiteMap
        {
            Urls = urlsByLocation.Values.ToList()
        };
    }
}
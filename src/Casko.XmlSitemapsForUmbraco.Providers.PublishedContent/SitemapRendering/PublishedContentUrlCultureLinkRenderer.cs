using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Contexts;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentUrlCultureLinkRenderer(IPublishedContentUrlBuilder urlBuilder) : IPublishedContentUrlCultureLinkRenderer
{
    public List<XHtmlLink> Render(IPublishedContent content, XmlSitemapUrlRenderContext context)
    {
        if (context.RenderAlternateLinks is false)
        {
            return [];
        }

        var orderedLanguageCodes = context.AlternativeLanguageCodes.Except([context.DefaultLanguageCode]).ToList();
        orderedLanguageCodes.Insert(0, context.DefaultLanguageCode);

        return orderedLanguageCodes
            .Select(languageCode => new XHtmlLink
            {
                Href = urlBuilder.BuildContentUrl(content, languageCode, context.Hostname),
                HrefLang = languageCode
            })
            .Where(cultureLink => !cultureLink.Href.Contains('#'))
            .ToList();
    }
}
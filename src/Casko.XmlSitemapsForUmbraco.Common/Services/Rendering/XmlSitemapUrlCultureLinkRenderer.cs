using Casko.XmlSitemapsForUmbraco.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed class XmlSitemapUrlCultureLinkRenderer(ISitemapUrlBuilder urlBuilder) : IXmlSitemapUrlCultureLinkRenderer
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
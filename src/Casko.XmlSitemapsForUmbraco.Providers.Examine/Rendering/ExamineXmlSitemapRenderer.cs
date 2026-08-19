using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;

public sealed class ExamineXmlSitemapRenderer(
    IExamineUrlRenderer urlRenderer,
    IXmlSitemapUrlSetRenderer urlSetRenderer) : IExamineXmlSitemapRenderer
{
    public XmlSitemap Render(ExamineXmlSitemapRenderContext context)
    {
        return urlSetRenderer.Render(urlRenderer.Render(
            context.Urls,
            context.DefaultLanguageCode,
            context.AlternativeLanguageCodes,
            context.Hostname,
            context.RenderAlternateLinks,
            context.UseHostnameForCultureLinks));
}
}

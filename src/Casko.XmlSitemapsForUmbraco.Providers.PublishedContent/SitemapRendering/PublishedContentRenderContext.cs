using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public record PublishedContentRenderContext(
    IReadOnlyCollection<IPublishedContent> RootContents,
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true,
    Func<IPublishedContent, bool>? ShouldIncludeContent = null);
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;

public sealed record XmlSitemapRenderContext(
    IReadOnlyCollection<IPublishedContent> RootContents,
    string DefaultLanguageCode,
    IReadOnlyCollection<string> AlternativeLanguageCodes,
    string? Hostname,
    bool RenderAlternateLinks = true,
    Func<IPublishedContent, bool>? ShouldIncludeContent = null);

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Cms;

public interface ICmsContentService
{
    /// <summary>
    /// Languages installed in CMS.
    /// </summary>
    public Task<string[]> GetLanguagesAsync();

    /// <summary>
    /// Gets content by key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public IPublishedContent? GetContent(Guid key);

    /// <summary>
    /// Gets the first root item in the content tree.
    /// </summary>
    /// <param name="contentTypeAlias">Optional content type alias to filter by.</param>
    /// <returns></returns>
    public IPublishedContent? GetRootContent(string? contentTypeAlias = null);


    /// <summary>
    /// Gets all root items in the content tree.
    /// </summary>
    /// <param name="contentTypeAlias">Optional content type alias to filter by.</param>
    /// <returns></returns>
    public IEnumerable<IPublishedContent>? GetRootContents(string? contentTypeAlias = null);

    /// <summary>
    /// Gets a content item by path.
    /// </summary>
    /// <param name="path">Path to content.</param>
    /// <param name="hostname">Optional hostname</param>
    /// <param name="culture">Optional culture of content.</param>
    /// <param name="preview">Flag indicating if content should be delivered in preview mode.</param>
    /// <returns></returns>
    public IPublishedContent? GetContentByPath(
        string path,
        string? hostname = null,
        string? culture = null,
        bool preview = false);
}
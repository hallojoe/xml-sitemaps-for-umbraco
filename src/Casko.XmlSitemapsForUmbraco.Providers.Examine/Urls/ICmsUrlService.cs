namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

public interface ICmsUrlService
{
    /// <summary>
    /// Gets all root items in the content tree.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<CmsUrl>> GetUrlsByKeyAsync(Guid key, CancellationToken cancellationToken = default);
    
}
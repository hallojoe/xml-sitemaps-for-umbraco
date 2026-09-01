namespace Casko.XmlSitemapsForUmbraco.Providers.Routing;

/// <summary>
/// IHostUrlProvider is responsible for providing the host URL nodes from Umbraco.
/// </summary>
public interface IHostUrlProvider
{
    /// <summary>
    /// Resolve the host urls from Umbraco and return them as a list of HostUrl records.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IEnumerable<HostUrl>> GetHostUrlsAsync(CancellationToken cancellationToken = default);    
}
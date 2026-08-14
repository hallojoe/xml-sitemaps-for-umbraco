namespace Casko.XmlSitemapsForUmbraco.Providers.Routing;

/// <summary>
/// A record representing a host URL with optional culture, ID, and key.
/// Used to store host URL nodes from Umbraco.
/// </summary>
/// <param name="Uri"></param>
/// <param name="Culture"></param>
/// <param name="Id"></param>
/// <param name="Key"></param>
/// <param name="IsDefaultCulture"></param>
public record HostUrl(Uri Uri, string Culture, int Id, Guid Key, bool IsDefaultCulture);
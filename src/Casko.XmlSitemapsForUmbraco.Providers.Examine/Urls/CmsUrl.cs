namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;

public record CmsUrl(
    string UrlPath, 
    DateTime LastUpdate, 
    string? Hostname, 
    string? Culture, 
    int? Id = null,
    Guid? Key = null);
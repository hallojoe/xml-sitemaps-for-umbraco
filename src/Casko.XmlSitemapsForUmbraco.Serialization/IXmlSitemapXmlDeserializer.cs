using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Serialization;

/// <summary>
/// Deserializes raw XML sitemap documents to sitemap models.
/// </summary>
public interface IXmlSitemapXmlDeserializer
{
    /// <summary>
    /// Deserializes raw XML to a sitemap model.
    /// </summary>
    T Deserialize<T>(string xml)
        where T : IXmlSitemapModel;
}
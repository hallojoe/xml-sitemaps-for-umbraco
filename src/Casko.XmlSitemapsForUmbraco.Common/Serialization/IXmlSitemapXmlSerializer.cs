using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Serialization;

/// <summary>
/// Serializes XML sitemap models to raw XML.
/// </summary>
public interface IXmlSitemapXmlSerializer
{
    /// <summary>
    /// Serializes the supplied sitemap model to XML.
    /// </summary>
    string Serialize<T>(T model)
        where T : IXmlSitemapModel;
}
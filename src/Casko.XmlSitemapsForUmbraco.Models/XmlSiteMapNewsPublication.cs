using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents publication metadata for a news sitemap entry.
/// </summary>
public sealed class XmlSiteMapNewsPublication
{
    /// <summary>
    /// Gets or sets the name of the publication.
    /// </summary>
    [XmlElement(Constants.NameElement, Namespace = Constants.NewsNamespace)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the language of the publication.
    /// </summary>
    [XmlElement(Constants.LanguageElement, Namespace = Constants.NewsNamespace)]
    public required string Language { get; set; }
}

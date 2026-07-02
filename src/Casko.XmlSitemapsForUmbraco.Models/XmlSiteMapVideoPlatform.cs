using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents platform restriction metadata for a video sitemap entry.
/// </summary>
public sealed class XmlSiteMapVideoPlatform
{
    /// <summary>
    /// Gets or sets the relationship between the listed platforms and the restriction.
    /// </summary>
    [XmlAttribute(Constants.RelationshipAttribute)]
    public required string Relationship { get; set; }

    /// <summary>
    /// Gets or sets the space-delimited list of platforms.
    /// </summary>
    [XmlText]
    public required string Value { get; set; }
}

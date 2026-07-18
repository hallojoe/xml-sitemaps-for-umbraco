using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents country restriction metadata for a video sitemap entry.
/// </summary>
public sealed class XmlSitemapVideoRestriction
{
    /// <summary>
    /// Gets or sets the relationship between the listed countries and the restriction.
    /// </summary>
    [XmlAttribute(Constants.RelationshipAttribute)]
    public required string Relationship { get; set; }

    /// <summary>
    /// Gets or sets the space-delimited list of country codes.
    /// </summary>
    [XmlText]
    public required string Value { get; set; }
}

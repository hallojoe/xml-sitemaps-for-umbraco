using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents uploader metadata for a video sitemap entry.
/// </summary>
public sealed class XmlSitemapVideoUploader
{
    /// <summary>
    /// Gets or sets the URL with more information about the uploader.
    /// </summary>
    [XmlAttribute(Constants.InfoAttribute)]
    public string? Info { get; set; }

    /// <summary>
    /// Gets or sets the uploader's display name.
    /// </summary>
    [XmlText]
    public required string Name { get; set; }
}

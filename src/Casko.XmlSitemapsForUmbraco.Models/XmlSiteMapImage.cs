using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents an image entry within an XML sitemap URL.
/// </summary>
public sealed class XmlSiteMapImage
{
    /// <summary>
    /// Gets or sets the URL of the image.
    /// </summary>
    [XmlElement(Constants.LocationElement, Namespace = Constants.ImageNamespace)]
    public required string Location { get; set; }
}

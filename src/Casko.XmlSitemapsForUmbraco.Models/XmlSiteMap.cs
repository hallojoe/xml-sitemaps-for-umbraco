using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents the root element of an XML sitemap, containing a collection of URLs.
/// This class is used to serialize and deserialize the sitemap, which helps search engines understand the structure of a website.
/// </summary>
[XmlRoot(Constants.UrlSetElement, Namespace = Constants.Namespace)]
public sealed class XmlSiteMap : IXmlSiteMapModel
{
    /// <summary>
    /// Gets the XML namespaces used when serializing sitemap extension elements.
    /// </summary>
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces { get; } = CreateNamespaces();

    /// <summary>
    /// Gets or sets the list of URLs included in the sitemap.
    /// Each URL is represented by an instance of the <see cref="XmlSiteMapUrl"/> class, which contains the details of a single page.
    /// </summary>
    [XmlElement(Constants.UrlElement, Type = typeof(XmlSiteMapUrl))]
    public List<XmlSiteMapUrl> Urls { get; set; } = new();

    private static XmlSerializerNamespaces CreateNamespaces()
    {
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(Constants.Empty, Constants.Namespace);
        namespaces.Add(Constants.XhtmlPrefix, Constants.XhtmlNamespace);
        namespaces.Add(Constants.ImagePrefix, Constants.ImageNamespace);
        namespaces.Add(Constants.VideoPrefix, Constants.VideoNamespace);
        namespaces.Add(Constants.NewsPrefix, Constants.NewsNamespace);
        return namespaces;
    }
}

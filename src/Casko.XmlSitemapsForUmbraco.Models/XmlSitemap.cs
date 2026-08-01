using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents the root element of an XML sitemap, containing a collection of URLs.
/// This class is used to serialize and deserialize the sitemap, which helps search engines understand the structure of a website.
/// </summary>
[XmlRoot(Constants.UrlSetElement, Namespace = Constants.Namespace)]
public sealed class XmlSitemap : IXmlSitemapModel
{
    /// <summary>
    /// Gets the XML namespaces used when serializing sitemap extension elements.
    /// </summary>
    [XmlNamespaceDeclarations]
    public XmlSerializerNamespaces Namespaces => CreateNamespaces();

    /// <summary>
    /// Gets or sets the list of URLs included in the sitemap.
    /// Each URL is represented by an instance of the <see cref="XmlSitemapUrl"/> class, which contains the details of a single page.
    /// </summary>
    [XmlElement(Constants.UrlElement, Type = typeof(XmlSitemapUrl))]
    public List<XmlSitemapUrl> Urls { get; set; } = new();

    public XmlSerializerNamespaces GetNamespaces()
    {
        return CreateNamespaces();
    }

    private XmlSerializerNamespaces CreateNamespaces()
    {
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(Constants.Empty, Constants.Namespace);

        if (Urls.Any(url => url.CultureLinks is { Count: > 0 }))
        {
            namespaces.Add(Constants.XhtmlPrefix, Constants.XhtmlNamespace);
        }

        if (Urls.Any(url => url.Images is { Count: > 0 }))
        {
            namespaces.Add(Constants.ImagePrefix, Constants.ImageNamespace);
        }

        if (Urls.Any(url => url.Videos is { Count: > 0 }))
        {
            namespaces.Add(Constants.VideoPrefix, Constants.VideoNamespace);
        }

        if (Urls.Any(url => url.News is not null))
        {
            namespaces.Add(Constants.NewsPrefix, Constants.NewsNamespace);
        }

        return namespaces;
    }
}

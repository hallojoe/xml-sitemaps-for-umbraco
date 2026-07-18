using System.Globalization;
using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents the news metadata attached to a sitemap URL.
/// </summary>
public sealed class XmlSitemapNews
{
    /// <summary>
    /// Gets or sets the publication metadata for the news article.
    /// </summary>
    [XmlElement(Constants.PublicationElement, Namespace = Constants.NewsNamespace)]
    public required XmlSitemapNewsPublication Publication { get; set; }

    /// <summary>
    /// Gets or sets the original publication date of the article.
    /// </summary>
    [XmlIgnore]
    public DateTimeOffset PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets the publication date formatted for XML serialization.
    /// </summary>
    [XmlElement(Constants.PublicationDateElement, Namespace = Constants.NewsNamespace)]
    public string PublicationDateSerialized
    {
        get => PublicationDate.ToString(Constants.W3CDateTimeFormat, CultureInfo.InvariantCulture);
        set => PublicationDate = DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : default;
    }

    /// <summary>
    /// Gets or sets the title of the news article.
    /// </summary>
    [XmlElement(Constants.TitleElement, Namespace = Constants.NewsNamespace)]
    public required string Title { get; set; }
}

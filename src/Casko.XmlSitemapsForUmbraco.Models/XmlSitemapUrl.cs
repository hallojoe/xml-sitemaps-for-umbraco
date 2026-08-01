using System.Xml.Serialization;
using Casko.XmlSitemapsForUmbraco.Models.Attributes;
using Casko.XmlSitemapsForUmbraco.Models.Enums;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents a single URL entry in an XML sitemap.
/// This class is used to serialize and deserialize individual URLs, along with their metadata, such as the last modified date, change frequency, priority, and any alternate language versions.
/// </summary>
public sealed class XmlSitemapUrl
{
    /// <summary>
    /// Gets or sets the URL of the page.
    /// This is the primary location that search engines will index.
    /// </summary>
    [XmlElement(Constants.LocationElement)]
    public string Location { get; set; } = null!;

    /// <summary>
    /// Gets or sets the date and time when the page was last modified.
    /// This value is ignored during XML serialization, but it can be accessed and modified programmatically.
    /// </summary>
    [XmlIgnore]
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Gets or sets the last modified date as a formatted string for XML serialization.
    /// The date is formatted according to a predefined pattern and is used during XML serialization.
    /// </summary>
    [XmlElement(Constants.LastModifiedElement)]
    public string LastModifiedFormatted
    {
        get => LastModified.ToString(Constants.DateFormat);
        set => LastModified = DateTime.TryParse(value, out var date) ? date : default;
    }

    /// <summary>
    /// Gets or sets the change frequency of the URL, which indicates how frequently the content at this URL is likely to change.
    /// This value is ignored during XML serialization but can be accessed and modified programmatically.
    /// </summary>
    [XmlIgnore]
    public ChangeFrequency ChangeFrequency { get; set; }

    /// <summary>
    /// Gets or sets the change frequency as a string for XML serialization.
    /// If the change frequency is set to None, this element will be omitted in the XML.
    /// </summary>
    [XmlElement(Constants.ChangeFrequencyElement, IsNullable = false)]
    public string? ChangeFrequencySerialized
    {
        get => ChangeFrequency == ChangeFrequency.None ? null : ChangeFrequency.ToString().ToLowerInvariant();
        set => ChangeFrequency =
            string.IsNullOrEmpty(value) ? ChangeFrequency.None : Enum.Parse<ChangeFrequency>(value);
    }

    /// <summary>
    /// Gets or sets the priority of the URL relative to other URLs on the site.
    /// The priority is a decimal value between 0.1 and 1.0, rounded to one decimal place.
    /// This property also includes validation to ensure the value falls within the allowed range.
    /// </summary>
    [PriorityValidation]
    [XmlElement(Constants.PriorityElement)]
    public double? Priority
    {
        get;
        set
        {
            if (value.HasValue)
            {
                // Round to one decimal; use AwayFromZero to avoid banker's rounding surprises
                var rounded = Math.Round(value.Value, 1, MidpointRounding.AwayFromZero);

                // Valid range: 0.1 ... 1.0 with step 0.1
                var isValid =
                    rounded is >= 0.1d and <= 1.0d &&
                    Math.Abs(rounded * 10 - Math.Round(rounded * 10)) < double.Epsilon;

                field = isValid ? rounded : null;
            }
            else
            {
                field = null;
            }
        }
    }

    // Tells XmlSerializer whether to emit <priority> at all.
    // Must be named exactly "<PropertyName>Specified".
    [XmlIgnore]
    public bool PrioritySpecified
    {
        get => Priority.HasValue;
        // Keep a setter so deserialization (if you ever do it) doesn't choke.
        set { /* no-op */ }
    }

    /// <summary>
    /// Gets or sets a list of alternate versions, of the URL in different languages or regions.
    /// Each link is represented by an instance of the <see cref="XHtmlLink"/> class.
    /// </summary>
    [XmlElement(Type = typeof(XHtmlLink), ElementName = Constants.XhtmlLinkElement, Namespace = Constants.XhtmlNamespace)]
    public List<XHtmlLink>? CultureLinks { get; set; }

    /// <summary>
    /// Gets or sets the images associated with this URL.
    /// Each image is represented by an instance of the <see cref="XmlSitemapImage"/> class.
    /// </summary>
    [XmlElement(Type = typeof(XmlSitemapImage), ElementName = Constants.ImageElement, Namespace = Constants.ImageNamespace)]
    public List<XmlSitemapImage>? Images { get; set; }

    /// <summary>
    /// Gets or sets the videos associated with this URL.
    /// Each video is represented by an instance of the <see cref="XmlSitemapVideo"/> class.
    /// </summary>
    [XmlElement(Type = typeof(XmlSitemapVideo), ElementName = Constants.VideoElement, Namespace = Constants.VideoNamespace)]
    public List<XmlSitemapVideo>? Videos { get; set; }

    /// <summary>
    /// Gets or sets the news metadata associated with this URL.
    /// </summary>
    [XmlElement(Constants.NewsElement, Namespace = Constants.NewsNamespace)]
    public XmlSitemapNews? News { get; set; }
}

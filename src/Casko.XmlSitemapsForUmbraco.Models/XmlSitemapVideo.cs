using System.Globalization;
using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models;

/// <summary>
/// Represents a video entry within an XML sitemap URL.
/// </summary>
public sealed class XmlSitemapVideo
{
    /// <summary>
    /// Gets or sets the thumbnail URL for the video.
    /// </summary>
    [XmlElement(Constants.ThumbnailLocationElement, Namespace = Constants.VideoNamespace)]
    public required string ThumbnailLocation { get; set; }

    /// <summary>
    /// Gets or sets the title of the video.
    /// </summary>
    [XmlElement(Constants.TitleElement, Namespace = Constants.VideoNamespace)]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the description of the video.
    /// </summary>
    [XmlElement(Constants.DescriptionElement, Namespace = Constants.VideoNamespace)]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the content URL for the video.
    /// Either this property or <see cref="PlayerLocation"/> should be provided.
    /// </summary>
    [XmlElement(Constants.ContentLocationElement, Namespace = Constants.VideoNamespace)]
    public string? ContentLocation { get; set; }

    /// <summary>
    /// Gets or sets the player URL for the video.
    /// Either this property or <see cref="ContentLocation"/> should be provided.
    /// </summary>
    [XmlElement(Constants.PlayerLocationElement, Namespace = Constants.VideoNamespace)]
    public string? PlayerLocation { get; set; }

    /// <summary>
    /// Gets or sets the duration of the video in seconds.
    /// </summary>
    [XmlElement(Constants.DurationElement, Namespace = Constants.VideoNamespace)]
    public int? Duration { get; set; }

    /// <summary>
    /// Gets a value indicating whether the duration element should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool DurationSpecified
    {
        get => Duration.HasValue;
        set { /* no-op */ }
    }

    /// <summary>
    /// Gets or sets the expiration date of the video.
    /// </summary>
    [XmlIgnore]
    public DateTimeOffset? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the expiration date formatted for XML serialization.
    /// </summary>
    [XmlElement(Constants.ExpirationDateElement, Namespace = Constants.VideoNamespace)]
    public string? ExpirationDateSerialized
    {
        get => FormatDate(ExpirationDate);
        set => ExpirationDate = ParseDate(value);
    }

    /// <summary>
    /// Gets or sets the rating of the video.
    /// </summary>
    [XmlElement(Constants.RatingElement, Namespace = Constants.VideoNamespace)]
    public double? Rating { get; set; }

    /// <summary>
    /// Gets a value indicating whether the rating element should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool RatingSpecified
    {
        get => Rating.HasValue;
        set { /* no-op */ }
    }

    /// <summary>
    /// Gets or sets the number of views for the video.
    /// </summary>
    [XmlElement(Constants.ViewCountElement, Namespace = Constants.VideoNamespace)]
    public int? ViewCount { get; set; }

    /// <summary>
    /// Gets a value indicating whether the view count element should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool ViewCountSpecified
    {
        get => ViewCount.HasValue;
        set { /* no-op */ }
    }

    /// <summary>
    /// Gets or sets the date when the video was first published.
    /// </summary>
    [XmlIgnore]
    public DateTimeOffset? PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets the publication date formatted for XML serialization.
    /// </summary>
    [XmlElement(Constants.PublicationDateElement, Namespace = Constants.VideoNamespace)]
    public string? PublicationDateSerialized
    {
        get => FormatDate(PublicationDate);
        set => PublicationDate = ParseDate(value);
    }

    /// <summary>
    /// Gets or sets whether the video is family friendly.
    /// </summary>
    [XmlIgnore]
    public bool? FamilyFriendly { get; set; }

    /// <summary>
    /// Gets or sets the family friendly value formatted for XML serialization.
    /// </summary>
    [XmlElement(Constants.FamilyFriendlyElement, Namespace = Constants.VideoNamespace)]
    public string? FamilyFriendlySerialized
    {
        get => FormatBoolean(FamilyFriendly);
        set => FamilyFriendly = ParseBoolean(value);
    }

    /// <summary>
    /// Gets or sets the country restriction metadata for the video.
    /// </summary>
    [XmlElement(Constants.RestrictionElement, Namespace = Constants.VideoNamespace)]
    public XmlSitemapVideoRestriction? Restriction { get; set; }

    /// <summary>
    /// Gets or sets the platform restriction metadata for the video.
    /// </summary>
    [XmlElement(Constants.PlatformElement, Namespace = Constants.VideoNamespace)]
    public XmlSitemapVideoPlatform? Platform { get; set; }

    /// <summary>
    /// Gets or sets whether a subscription is required to view the video.
    /// </summary>
    [XmlIgnore]
    public bool? RequiresSubscription { get; set; }

    /// <summary>
    /// Gets or sets the subscription requirement formatted for XML serialization.
    /// </summary>
    [XmlElement(Constants.RequiresSubscriptionElement, Namespace = Constants.VideoNamespace)]
    public string? RequiresSubscriptionSerialized
    {
        get => FormatBoolean(RequiresSubscription);
        set => RequiresSubscription = ParseBoolean(value);
    }

    /// <summary>
    /// Gets or sets the uploader metadata for the video.
    /// </summary>
    [XmlElement(Constants.UploaderElement, Namespace = Constants.VideoNamespace)]
    public XmlSitemapVideoUploader? Uploader { get; set; }

    /// <summary>
    /// Gets or sets whether the video is a livestream.
    /// </summary>
    [XmlIgnore]
    public bool? Live { get; set; }

    /// <summary>
    /// Gets or sets the livestream value formatted for XML serialization.
    /// </summary>
    [XmlElement(Constants.LiveElement, Namespace = Constants.VideoNamespace)]
    public string? LiveSerialized
    {
        get => FormatBoolean(Live);
        set => Live = ParseBoolean(value);
    }

    /// <summary>
    /// Gets or sets the tags associated with the video.
    /// </summary>
    [XmlElement(Constants.TagElement, Namespace = Constants.VideoNamespace)]
    public List<string>? Tags { get; set; }

    private static string? FormatBoolean(bool? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        return value.Value ? Constants.YesValue : Constants.NoValue;
    }

    private static bool? ParseBoolean(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            Constants.YesValue => true,
            Constants.NoValue => false,
            _ => null
        };
    }

    private static string? FormatDate(DateTimeOffset? value)
    {
        return value?.ToString(Constants.W3CDateTimeFormat, CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;
    }
}

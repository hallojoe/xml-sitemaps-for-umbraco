using System.Globalization;
using Casko.XmlSitemapsForUmbraco.Common.Serialization;
using Casko.XmlSitemapsForUmbraco.Models;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class XmlSitemapXmlSerializerTests
{
    private readonly XmlSitemapXmlSerializer _sut = new();

    [Test]
    public void Serialize_WhenSitemapModelIsProvided_ReturnsUrlSetXml()
    {
        var result = _sut.Serialize(new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/"
                }
            ]
        });

        Assert.That(result, Does.Contain("<urlset"));
        Assert.That(result, Does.Contain($"xmlns=\"{Constants.Namespace}\""));
        Assert.That(result, Does.Contain("<loc>https://www.example.com/</loc>"));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.XhtmlPrefix}="));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.ImagePrefix}="));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.VideoPrefix}="));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.NewsPrefix}="));
    }

    [Test]
    public void Serialize_WhenSitemapIndexModelIsProvided_ReturnsSitemapIndexXml()
    {
        var result = _sut.Serialize(new XmlSitemapIndex
        {
            Locations =
            [
                new XmlSitemapIndexLocation
                {
                    Location = "https://www.example.com/sitemap.xml"
                }
            ]
        });

        Assert.That(result, Does.Contain("<sitemapindex"));
        Assert.That(result, Does.Contain("<loc>https://www.example.com/sitemap.xml</loc>"));
    }

    [Test]
    public void Serialize_WhenSitemapContainsExtensions_ReturnsExtensionXml()
    {
        var publicationDate = new DateTimeOffset(2026, 7, 2, 14, 30, 45, TimeSpan.FromHours(2));
        var result = _sut.Serialize(new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/articles/example",
                    CultureLinks =
                    [
                        new XHtmlLink
                        {
                            Href = "https://www.example.com/en/articles/example",
                            HrefLang = "en"
                        }
                    ],
                    Images =
                    [
                        new XmlSitemapImage { Location = "https://cdn.example.com/image-1.jpg" },
                        new XmlSitemapImage { Location = "https://cdn.example.com/image-2.jpg" }
                    ],
                    Videos =
                    [
                        new XmlSitemapVideo
                        {
                            ThumbnailLocation = "https://cdn.example.com/video-thumb.jpg",
                            Title = "Example video",
                            Description = "Example description",
                            ContentLocation = "https://cdn.example.com/example-video.mp4",
                            PublicationDate = publicationDate,
                            FamilyFriendly = true,
                            RequiresSubscription = false,
                            Live = false,
                            Tags = ["first-tag", "second-tag"]
                        }
                    ],
                    News = new XmlSitemapNews
                    {
                        Publication = new XmlSitemapNewsPublication
                        {
                            Name = "The Example Times",
                            Language = "en"
                        },
                        PublicationDate = publicationDate,
                        Title = "Companies A, B in Merger Talks"
                    }
                }
            ]
        });

        Assert.That(result, Does.Contain("<xhtml:link"));
        Assert.That(result, Does.Contain($"xmlns:{Constants.XhtmlPrefix}=\"{Constants.XhtmlNamespace}\""));
        Assert.That(result, Does.Contain($"xmlns:{Constants.ImagePrefix}=\"{Constants.ImageNamespace}\""));
        Assert.That(result, Does.Contain($"xmlns:{Constants.VideoPrefix}=\"{Constants.VideoNamespace}\""));
        Assert.That(result, Does.Contain($"xmlns:{Constants.NewsPrefix}=\"{Constants.NewsNamespace}\""));
        Assert.That(result, Does.Contain("rel=\"alternate\""));
        Assert.That(result, Does.Contain("<image:image>"));
        Assert.That(result, Does.Contain("<image:loc>https://cdn.example.com/image-1.jpg</image:loc>"));
        Assert.That(result, Does.Contain("<image:loc>https://cdn.example.com/image-2.jpg</image:loc>"));
        Assert.That(result, Does.Contain("<video:video>"));
        Assert.That(result, Does.Contain("<video:thumbnail_loc>https://cdn.example.com/video-thumb.jpg</video:thumbnail_loc>"));
        Assert.That(result, Does.Contain("<video:title>Example video</video:title>"));
        Assert.That(result, Does.Contain("<video:description>Example description</video:description>"));
        Assert.That(result, Does.Contain("<video:content_loc>https://cdn.example.com/example-video.mp4</video:content_loc>"));
        Assert.That(result, Does.Contain("<video:family_friendly>yes</video:family_friendly>"));
        Assert.That(result, Does.Contain("<video:requires_subscription>no</video:requires_subscription>"));
        Assert.That(result, Does.Contain("<video:live>no</video:live>"));
        Assert.That(result, Does.Contain("<video:tag>first-tag</video:tag>"));
        Assert.That(result, Does.Contain("<video:tag>second-tag</video:tag>"));
        Assert.That(result, Does.Contain("<news:news>"));
        Assert.That(result, Does.Contain("<news:publication>"));
        Assert.That(result, Does.Contain("<news:name>The Example Times</news:name>"));
        Assert.That(result, Does.Contain("<news:language>en</news:language>"));
        Assert.That(result, Does.Contain("<news:title>Companies A, B in Merger Talks</news:title>"));
        Assert.That(result, Does.Contain($"<video:publication_date>{publicationDate.ToString(Constants.W3CDateTimeFormat, CultureInfo.InvariantCulture)}</video:publication_date>"));
        Assert.That(result, Does.Contain($"<news:publication_date>{publicationDate.ToString(Constants.W3CDateTimeFormat, CultureInfo.InvariantCulture)}</news:publication_date>"));
    }

    [Test]
    public void Serialize_WhenVideoContainsOnlyRequiredFields_OmitsUnsetOptionalElements()
    {
        var result = _sut.Serialize(new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/videos/example",
                    Videos =
                    [
                        new XmlSitemapVideo
                        {
                            ThumbnailLocation = "https://cdn.example.com/required-thumb.jpg",
                            Title = "Required only",
                            Description = "Required description",
                            PlayerLocation = "https://www.example.com/player/example"
                        }
                    ]
                }
            ]
        });

        Assert.That(result, Does.Contain("<video:player_loc>https://www.example.com/player/example</video:player_loc>"));
        Assert.That(result, Does.Contain($"xmlns:{Constants.VideoPrefix}=\"{Constants.VideoNamespace}\""));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.XhtmlPrefix}="));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.ImagePrefix}="));
        Assert.That(result, Does.Not.Contain($"xmlns:{Constants.NewsPrefix}="));
        Assert.That(result, Does.Not.Contain("<video:content_loc>"));
        Assert.That(result, Does.Not.Contain("<video:duration>"));
        Assert.That(result, Does.Not.Contain("<video:publication_date>"));
        Assert.That(result, Does.Not.Contain("<video:family_friendly>"));
        Assert.That(result, Does.Not.Contain("<video:requires_subscription>"));
        Assert.That(result, Does.Not.Contain("<video:live>"));
        Assert.That(result, Does.Not.Contain("<video:tag>"));
    }

    [Test]
    public void XmlSiteMapVideo_WhenSerializedPropertiesAreSet_UsesExpectedFormats()
    {
        var publicationDate = new DateTimeOffset(2026, 7, 2, 8, 9, 10, TimeSpan.FromHours(2));
        var expirationDate = new DateTimeOffset(2026, 7, 5, 11, 12, 13, TimeSpan.FromHours(2));

        var video = new XmlSitemapVideo
        {
            ThumbnailLocation = "https://cdn.example.com/thumb.jpg",
            Title = "Formatting example",
            Description = "Formatting description",
            ContentLocation = "https://cdn.example.com/video.mp4",
            PublicationDate = publicationDate,
            ExpirationDate = expirationDate,
            FamilyFriendly = false,
            RequiresSubscription = true,
            Live = true
        };

        Assert.That(video.PublicationDateSerialized, Is.EqualTo(publicationDate.ToString(Constants.W3CDateTimeFormat, CultureInfo.InvariantCulture)));
        Assert.That(video.ExpirationDateSerialized, Is.EqualTo(expirationDate.ToString(Constants.W3CDateTimeFormat, CultureInfo.InvariantCulture)));
        Assert.That(video.FamilyFriendlySerialized, Is.EqualTo(Constants.NoValue));
        Assert.That(video.RequiresSubscriptionSerialized, Is.EqualTo(Constants.YesValue));
        Assert.That(video.LiveSerialized, Is.EqualTo(Constants.YesValue));
    }
}

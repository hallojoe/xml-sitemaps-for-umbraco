using Casko.XmlSitemapsForUmbraco.Storage;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class XmlSitemapDataSourceTests
{
    [Test]
    public async Task ExistsAsync_WhenReadReturnsDocument_ReturnsTrue()
    {
        var key = CreateKey();
        IXmlSitemapDataSource sut = new StubXmlSitemapDataSource(new XmlSitemapStoredDocument(
            key,
            Guid.NewGuid(),
            42,
            "sitemap--default--products.xml",
            "/media/sitemaps/products.xml",
            "<urlset />"));

        var result = await sut.ExistsAsync(key);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistsAsync_WhenReadReturnsNull_ReturnsFalse()
    {
        var key = CreateKey();
        IXmlSitemapDataSource sut = new StubXmlSitemapDataSource(null);

        var result = await sut.ExistsAsync(key);

        Assert.That(result, Is.False);
    }

    private static XmlSitemapStorageKey CreateKey()
    {
        return new XmlSitemapStorageKey(XmlSitemapDocumentKind.Sitemap, "products", null);
    }

    private sealed class StubXmlSitemapDataSource(XmlSitemapStoredDocument? document) : IXmlSitemapDataSource
    {
        public Task<XmlSitemapStoredDocument?> ReadAsync(
            XmlSitemapStorageKey key,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(document);
        }

        public Task<XmlSitemapStoredDocument> WriteAsync(
            XmlSitemapStorageKey key,
            string xml,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}

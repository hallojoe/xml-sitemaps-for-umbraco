using System.Net;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Integration;

[TestFixture]
public sealed class XmlSitemapDeliveryApiIntegrationTests : UmbracoTestServerBase
{
    [Test]
    public async Task DirectRoute_ReturnsControllerResponse_InsteadOfWebsite404()
    {
        using var response = await Client.GetAsync(PrepareUrl("/api/sitemap/key?key=missing-sitemap"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(body, Does.Not.Contain("No umbraco document matches the URL"));
        });
    }

    [Test]
    public async Task RewriteRoute_ReturnsControllerResponse_InsteadOfWebsite404()
    {
        using var response = await Client.GetAsync(PrepareUrl("/xmlsitemap.xml"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.InternalServerError));
            Assert.That(body, Does.Not.Contain("No umbraco document matches the URL"));
        });
    }

    [Test]
    public async Task SwaggerDocument_ContainsDeliveryApiPaths()
    {
        using var response = await Client.GetAsync(PrepareUrl("/umbraco/swagger/sitemap-api/swagger.json"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType, Is.Not.Null);
            Assert.That(response.Content.Headers.ContentType!.MediaType, Is.EqualTo("application/json"));
            Assert.That(body, Does.Contain("\"/api/sitemap/key\""));
            Assert.That(body, Does.Contain("\"/api/sitemap/path\""));
        });
    }
}

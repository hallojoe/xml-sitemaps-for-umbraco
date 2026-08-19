using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class SitemapRewriteMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_WhenSitemapMatches_RewritesToSitemapKeyRoute()
    {
        var context = CreateContext("/xmlsitemap-host-dk-en.xml", "host.dk");
        var rewriteService = Substitute.For<ISitemapRewriteDefinitionService>();
        rewriteService
            .TryMatch(context.Request.Path, context.Request.Host, out Arg.Any<SitemapRewriteDefinition?>())
            .Returns(callInfo =>
            {
                callInfo[2] = new SitemapRewriteDefinition(
                    "/xmlsitemap-host-dk-en.xml",
                    $"/{XmlSitemapApiConstants.ApiRoute}/xmlsitemap?key=xmlsitemap-host-dk-en",
                    "xmlsitemap-host-dk-en",
                    "xmlsitemap-host-dk-en",
                    SitemapRewriteKind.Sitemap,
                    "host.dk");

                return true;
            });
        var sut = new SitemapRewriteMiddleware(_ => Task.CompletedTask, rewriteService);

        await sut.InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Request.Path.Value, Is.EqualTo($"/{XmlSitemapApiConstants.ApiRoute}/xmlsitemap"));
            Assert.That(context.Request.QueryString.Value, Is.EqualTo("?key=xmlsitemap-host-dk-en"));
        });
    }

    [Test]
    public async Task InvokeAsync_WhenIndexMatches_RewritesToIndexKeyRoute()
    {
        var context = CreateContext("/xmlsitemap.xml", "host.dk");
        var rewriteService = Substitute.For<ISitemapRewriteDefinitionService>();
        rewriteService
            .TryMatch(context.Request.Path, context.Request.Host, out Arg.Any<SitemapRewriteDefinition?>())
            .Returns(callInfo =>
            {
                callInfo[2] = new SitemapRewriteDefinition(
                    "/xmlsitemap.xml",
                    $"/{XmlSitemapApiConstants.ApiRoute}/xmlsitemapindex?key=xmlsitemap",
                    "xmlsitemap",
                    "xmlsitemap",
                    SitemapRewriteKind.SitemapIndex,
                    "host.dk");

                return true;
            });
        var sut = new SitemapRewriteMiddleware(_ => Task.CompletedTask, rewriteService);

        await sut.InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Request.Path.Value, Is.EqualTo($"/{XmlSitemapApiConstants.ApiRoute}/xmlsitemapindex"));
            Assert.That(context.Request.QueryString.Value, Is.EqualTo("?key=xmlsitemap"));
        });
    }

    [Test]
    public async Task InvokeAsync_WhenNoDefinitionMatches_LeavesRequestUnchanged()
    {
        var context = CreateContext("/xmlsitemap.xml", "other.dk");
        var rewriteService = Substitute.For<ISitemapRewriteDefinitionService>();
        rewriteService
            .TryMatch(context.Request.Path, context.Request.Host, out Arg.Any<SitemapRewriteDefinition?>())
            .Returns(false);
        var sut = new SitemapRewriteMiddleware(_ => Task.CompletedTask, rewriteService);

        await sut.InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Request.Path.Value, Is.EqualTo("/xmlsitemap.xml"));
            Assert.That(context.Request.QueryString.Value, Is.Empty);
        });
    }

    [Test]
    public void ShouldRegister_WhenRewritesEnabled_ReturnsTrue()
    {
        Assert.That(SitemapRewritePipeline.ShouldRegister(new() { RewritesEnabled = true }), Is.True);
    }

    [Test]
    public void ShouldRegister_WhenRewritesDisabled_ReturnsFalse()
    {
        Assert.That(SitemapRewritePipeline.ShouldRegister(new() { RewritesEnabled = false }), Is.False);
    }

    private static HttpContext CreateContext(string path, string host)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Host = new HostString(host);

        return context;
    }
}

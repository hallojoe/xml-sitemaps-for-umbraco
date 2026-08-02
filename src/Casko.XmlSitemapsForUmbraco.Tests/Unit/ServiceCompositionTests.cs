using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.Examine;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;
using Casko.XmlSitemapsForUmbraco.Serialization;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class ServiceCompositionTests
{
    [Test]
    public void PackageOrder_UsesStoredProviderOverExamineSourceProvider()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddXmlSitemapConfiguration(configuration);
        services.AddXmlSitemapPublishedContentProvider();
        services.AddXmlSitemapExamineProvider();
        services.AddXmlSitemapsUmbracoMediaStorage();

        services.AddScoped(_ => Substitute.For<IPublishedContentService>());
        services.AddScoped(_ => Substitute.For<ICmsUrlService>());
        services.AddScoped(_ => Substitute.For<IXmlSitemapDataSource>());

        using var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });
        using var scope = serviceProvider.CreateScope();

        var publicProvider = scope.ServiceProvider.GetRequiredService<IXmlSitemapProvider>();
        var sourceProvider = scope.ServiceProvider.GetRequiredService<IXmlSitemapSourceProvider>();

        Assert.Multiple(() =>
        {
            Assert.That(publicProvider, Is.TypeOf<StoredXmlSitemapProvider>());
            Assert.That(sourceProvider, Is.TypeOf<ExamineXmlSitemapProvider>());
        });
    }

    [Test]
    public void UmbracoMediaStorage_EnsuresBaseProviderAndSerializationDependencies()
    {
        var services = new ServiceCollection();

        services.AddXmlSitemapsUmbracoMediaStorage();

        Assert.Multiple(() =>
        {
            Assert.That(services.Any(service => service.ServiceType == typeof(TimeProvider)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IHostUrlProvider)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapUrlBuilder)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapXmlSerializer)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapXmlDeserializer)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapDataSource)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapStorageRefreshService)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapProvider) &&
                                                service.ImplementationType == typeof(StoredXmlSitemapProvider)), Is.True);
        });
    }
}

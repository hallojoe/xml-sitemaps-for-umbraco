using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Common.Serialization;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.Examine;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Routing;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["XmlSitemaps:Storage:VersionCleanupAfterSeconds"] = "600"
        });

        services.AddXmlSitemapsConfiguration(configuration);
        services.AddXmlSitemapExamineProvider();
        services.AddXmlSitemapsUmbracoMediaStorage(configuration);
        services.AddLogging();

        services.AddScoped(_ => Substitute.For<IHostUrlProvider>());
        services.AddScoped(_ => Substitute.For<IExamineSitemapRootResolver>());
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
    public void UmbracoMediaStorage_WhenStorageIsAbsent_DoesNotRegisterStorageServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddXmlSitemapsUmbracoMediaStorage(configuration);

        Assert.Multiple(() =>
        {
            Assert.That(services.Any(service => service.ServiceType == typeof(TimeProvider)), Is.False);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapDataSource)), Is.False);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapStorageRefreshService)), Is.False);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapProvider) &&
                                                service.ImplementationType == typeof(StoredXmlSitemapProvider)), Is.False);
        });
    }

    [Test]
    public void ExamineProvider_WhenUsingExternalIndex_RegistersExternalIndexUrlService()
    {
        var services = new ServiceCollection();

        services.AddXmlSitemapExamineProvider(Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName);

        Assert.That(
            services.Single(service => service.ServiceType == typeof(ICmsUrlService)).ImplementationType,
            Is.EqualTo(typeof(ExternalIndexUrlService)));
    }

    [Test]
    public void ExamineProvider_RegistersSharedSearchResultFilter()
    {
        var services = new ServiceCollection();

        services.AddXmlSitemapExamineProvider();

        Assert.That(
            services.Single(service => service.ServiceType == typeof(IExamineSitemapSearchResultFilter)).ImplementationType,
            Is.EqualTo(typeof(ExamineSitemapSearchResultFilter)));
    }

    [Test]
    public void ExamineProvider_WhenUsingDeliveryApiContentIndex_RegistersDeliveryApiUrlService()
    {
        var services = new ServiceCollection();

        services.AddXmlSitemapExamineProvider(Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName);

        Assert.That(
            services.Single(service => service.ServiceType == typeof(ICmsUrlService)).ImplementationType,
            Is.EqualTo(typeof(DeliveryApiContentIndexUrlService)));
    }

    [Test]
    public void ExamineProvider_WhenUsingUnsupportedIndex_ThrowsClearConfigurationError()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddXmlSitemapExamineProvider("CustomIndex"));

        Assert.That(exception?.Message, Does.Contain("CustomIndex"));
    }

    [Test]
    public void UmbracoMediaStorage_WhenBackgroundJobIsAbsent_DoesNotRegisterRecurringJob()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["XmlSitemaps:Storage:VersionCleanupAfterSeconds"] = "600"
        });

        services.AddXmlSitemapsUmbracoMediaStorage(configuration);

        Assert.Multiple(() =>
        {
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapDataSource)), Is.True);
            Assert.That(services.Any(service => service.ServiceType == typeof(IXmlSitemapStorageRefreshService)), Is.True);
            Assert.That(services.Any(service => service.ImplementationType == typeof(UmbracoMediaXmlSitemapRefreshBackgroundJob)), Is.False);
        });
    }

    [Test]
    public void UmbracoMediaStorage_WhenBackgroundJobIsPresent_RegistersRecurringJob()
    {
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["XmlSitemaps:Storage:BackgroundJob:IntervalSeconds"] = "300"
        });

        services.AddXmlSitemapsUmbracoMediaStorage(configuration);

        Assert.That(services.Any(service => service.ImplementationType == typeof(UmbracoMediaXmlSitemapRefreshBackgroundJob)), Is.True);
    }

    private static IConfiguration CreateConfiguration(IReadOnlyDictionary<string, string?> values)
    {
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

}

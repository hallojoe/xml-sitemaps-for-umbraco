using Casko.XmlSitemapsForUmbraco.Storage;
using Casko.XmlSitemapsForUmbraco.Storage.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public class UmbracoMediaXmlSitemapRefreshBackgroundJobTests
{
    [Test]
    public void Delay_UsesDefaultValue()
    {
        var sut = CreateJob(new XmlSitemapStorageOptions
        {
            BackgroundJob = new XmlSitemapStorageBackgroundJobOptions()
        }, Substitute.For<IXmlSitemapStorageRefreshService>());

        Assert.That(sut.Delay, Is.EqualTo(TimeSpan.FromSeconds(10)));
    }

    [Test]
    public void Period_UsesConfiguredIntervalSeconds()
    {
        var sut = CreateJob(new XmlSitemapStorageOptions
        {
            BackgroundJob = new XmlSitemapStorageBackgroundJobOptions { IntervalSeconds = 120 }
        }, Substitute.For<IXmlSitemapStorageRefreshService>());

        Assert.That(sut.Period, Is.EqualTo(TimeSpan.FromSeconds(120)));
    }

    [Test]
    public void ServerRoles_UseConfiguredServerRoles()
    {
        var sut = CreateJob(new XmlSitemapStorageOptions
        {
            BackgroundJob = new XmlSitemapStorageBackgroundJobOptions()
        }, Substitute.For<IXmlSitemapStorageRefreshService>());

        Assert.That(sut.ServerRoles, Is.EqualTo(new[]
        {
            ServerRole.SchedulingPublisher,
            ServerRole.Single,
            ServerRole.Unknown
        }));
    }

    [Test]
    public async Task RunJobAsync_RefreshesAllConfiguredDocuments()
    {
        var refreshService = Substitute.For<IXmlSitemapStorageRefreshService>();
        var sut = CreateJob(new XmlSitemapStorageOptions
        {
            BackgroundJob = new XmlSitemapStorageBackgroundJobOptions()
        }, refreshService);

        await sut.RunJobAsync();

        await refreshService.Received(1).RefreshAllAsync();
    }

    private static UmbracoMediaXmlSitemapRefreshBackgroundJob CreateJob(
        XmlSitemapStorageOptions options,
        IXmlSitemapStorageRefreshService refreshService)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => refreshService);
        var serviceProvider = services.BuildServiceProvider();

        return new UmbracoMediaXmlSitemapRefreshBackgroundJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(options),
            Substitute.For<ILogger<UmbracoMediaXmlSitemapRefreshBackgroundJob>>());
    }
}

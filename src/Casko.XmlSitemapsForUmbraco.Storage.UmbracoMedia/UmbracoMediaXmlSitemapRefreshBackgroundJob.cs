using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;

public sealed class UmbracoMediaXmlSitemapRefreshBackgroundJob(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<XmlSitemapsOptions> xmlSitemapOptions,
    ILogger<UmbracoMediaXmlSitemapRefreshBackgroundJob> logger) : IRecurringBackgroundJob
{
    private const int DefaultIntervalSeconds = 3600;
    private const int MinimumDelaySeconds = 10;

    public TimeSpan Period => TimeSpan.FromSeconds(GetIntervalSeconds());

    public TimeSpan Delay => TimeSpan.FromSeconds(GetDelaySeconds());

    public ServerRole[] ServerRoles =>
    [
        ServerRole.SchedulingPublisher,
        ServerRole.Single,
        ServerRole.Unknown
    ];

    public event EventHandler? PeriodChanged
    {
        add { }
        remove { }
    }

    public async Task RunJobAsync()
    {
        logger.LogInformation(
            "Running XML sitemap media refresh background job.");

        if (!xmlSitemapOptions.Value.Storage.BackgroundJob.Enabled)
        {
            logger.LogDebug(
                "XML sitemap media refresh background job is disabled. Skipping refresh.");

            return;
        }

        logger.LogDebug(
            "Creating service scope for XML sitemap media refresh.");

        using var scope = serviceScopeFactory.CreateScope();

        var refreshService =
            scope.ServiceProvider.GetRequiredService<IXmlSitemapStorageRefreshService>();

        logger.LogDebug(
            "Starting refresh of all XML sitemap storage.");

        await refreshService.RefreshAllAsync();

        logger.LogDebug(
            "Completed refresh of all XML sitemap storage.");
    }

    private int GetIntervalSeconds()
    {
        var intervalSeconds =
            xmlSitemapOptions.Value.Storage.BackgroundJob.IntervalSeconds;

        if (intervalSeconds > 0)
        {
            return intervalSeconds;
        }

        logger.LogDebug(
            "Configured XML sitemap refresh interval {IntervalSeconds} is invalid. Using default interval {DefaultIntervalSeconds} seconds.",
            intervalSeconds,
            DefaultIntervalSeconds);

        return DefaultIntervalSeconds;
    }

    private int GetDelaySeconds()
    {
        var delaySeconds =
            xmlSitemapOptions.Value.Storage.BackgroundJob.RefreshJobDelayInSeconds;

        if (delaySeconds >= MinimumDelaySeconds)
        {
            return delaySeconds;
        }

        logger.LogDebug(
            "Configured XML sitemap refresh delay {DelaySeconds} is below the minimum. Using {MinimumDelaySeconds} seconds.",
            delaySeconds,
            MinimumDelaySeconds);

        return MinimumDelaySeconds;
    }
}
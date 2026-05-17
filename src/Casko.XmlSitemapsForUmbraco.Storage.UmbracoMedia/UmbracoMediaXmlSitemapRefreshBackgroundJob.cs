using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia;

public sealed class UmbracoMediaXmlSitemapRefreshBackgroundJob( 
    IServiceScopeFactory serviceScopeFactory,
    IOptions<XmlSitemapsOptions> xmlSitemapOptions) : IRecurringBackgroundJob
{
    public TimeSpan Period => TimeSpan.FromSeconds(GetIntervalSeconds());

    public TimeSpan Delay => TimeSpan.FromSeconds(10);

    public ServerRole[] ServerRoles => IRecurringBackgroundJob.DefaultServerRoles;

    public event EventHandler? PeriodChanged
    {
        add { }
        remove { }
    }

    public async Task RunJobAsync()
    {
        if (!xmlSitemapOptions.Value.Storage.BackgroundJob.Enabled)
        {
            return;
        }

        using var scope = serviceScopeFactory.CreateScope();
        var refreshService = scope.ServiceProvider.GetRequiredService<IXmlSitemapStorageRefreshService>();
        await refreshService.RefreshAllAsync();
    }

    private int GetIntervalSeconds()
    {
        var intervalSeconds = xmlSitemapOptions.Value.Storage.BackgroundJob.IntervalSeconds;
        return intervalSeconds > 0 ? intervalSeconds : 3600;
    }
}

using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsUmbracoMediaStorage(this IServiceCollection services)
    {

        services.TryAddTimeProvider();
        services.AddScoped<IXmlSitemapStorageNameProvider, XmlSitemapStorageNameProvider>();
        services.AddScoped<IUmbracoMediaFileAccessor, UmbracoMediaFileAccessor>();
        services.AddScoped<IXmlSitemapDataSource, UmbracoMediaXmlSitemapDataSource>();
        services.AddScoped<IXmlSitemapStorageRefreshService, XmlSitemapStorageRefreshService>();
        
        // Take over default IXmlSitemapService
        services.AddScoped<IXmlSitemapService, StoredXmlSitemapService>();
        services.AddRecurringBackgroundJob<UmbracoMediaXmlSitemapRefreshBackgroundJob>();
        
        return services;
    }

    private static IServiceCollection TryAddTimeProvider(this IServiceCollection services)
    {
        if (services.Any(service => service.ServiceType == typeof(TimeProvider)))
        {
            return services;
        }

        services.AddSingleton(TimeProvider.System);
        return services;
    }
}

using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Casko.XmlSitemapsForUmbraco.Serialization.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsUmbracoMediaStorage(this IServiceCollection services)
    {

        services.AddXmlSitemapProviders();
        services.AddXmlSitemapsSerialization();
        services.TryAddTimeProvider();
        
        services.AddScoped<IXmlSitemapStorageNameProvider, XmlSitemapStorageNameProvider>();
        services.AddScoped<IUmbracoMediaFileAccessor, UmbracoMediaFileAccessor>();
        services.AddScoped<IXmlSitemapDataSource, UmbracoMediaXmlSitemapDataSource>();
        services.AddScoped<IXmlSitemapStorageRefreshService, XmlSitemapStorageRefreshService>();
        
        // Take over default IXmlSitemapProvider
        services.AddScoped<IXmlSitemapProvider, StoredXmlSitemapProvider>();
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

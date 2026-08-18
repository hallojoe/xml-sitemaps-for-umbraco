using Casko.XmlSitemapsForUmbraco.Common.Serialization.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers;
using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Storage.Configuration;

/// <summary>
/// Registers services for stored XML sitemap delivery.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers storage services when the storage configuration section exists.
    /// </summary>
    public static IServiceCollection AddXmlSitemapsStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storageSection = configuration.GetSection(XmlSitemapStorageOptions.Key);
        if (!storageSection.Exists())
        {
            return services;
        }

        services.Configure<XmlSitemapStorageOptions>(storageSection);
        services.AddXmlSitemapsProviders();
        services.AddXmlSitemapsSerialization();
        services.TryAddTimeProvider();
        services.AddScoped<IXmlSitemapStorageNameProvider, XmlSitemapStorageNameProvider>();
        services.AddScoped<IXmlSitemapStorageRefreshService, XmlSitemapStorageRefreshService>();
        services.AddScoped<IXmlSitemapProvider, StoredXmlSitemapProvider>();

        return services;
    }

    private static IServiceCollection TryAddTimeProvider(this IServiceCollection services)
    {
        if (!services.Any(service => service.ServiceType == typeof(TimeProvider)))
        {
            services.AddSingleton(TimeProvider.System);
        }

        return services;
    }
}

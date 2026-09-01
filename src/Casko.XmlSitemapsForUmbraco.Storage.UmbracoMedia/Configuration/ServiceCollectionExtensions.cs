using Casko.XmlSitemapsForUmbraco.Storage.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsUmbracoMediaStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storageSection = configuration.GetSection(XmlSitemapStorageOptions.Key);
        if (!storageSection.Exists())
        {
            return services;
        }

        services.AddXmlSitemapsStorage(configuration);
        services.AddScoped<IUmbracoMediaFileAccessor, UmbracoMediaFileAccessor>();
        services.AddScoped<IXmlSitemapDataSource, UmbracoMediaXmlSitemapDataSource>();

        if (storageSection.GetSection(nameof(XmlSitemapStorageOptions.BackgroundJob)).Exists())
        {
            services.AddRecurringBackgroundJob<UmbracoMediaXmlSitemapRefreshBackgroundJob>();
        }

        return services;
    }
}

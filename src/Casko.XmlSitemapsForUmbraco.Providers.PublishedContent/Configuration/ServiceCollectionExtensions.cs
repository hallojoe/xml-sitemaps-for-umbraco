using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.ContentReading;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapPublishedContentProvider(this IServiceCollection services)
    {
        services.AddXmlSitemapProviders();

        services.AddScoped<IPublishedContentService, PublishedContentService>();
        services.AddScoped<IPublishedContentUrlBuilder, PublishedContentUrlBuilder>();
        services.AddScoped<IPublishedContentCollector, PublishedContentCollector>();
        services.AddScoped<IPublishedContentUrlCultureLinkRenderer, PublishedContentUrlCultureLinkRenderer>();
        services.AddScoped<IPublishedContentUrlRenderer, PublishedContentUrlRenderer>();
        services.AddScoped<IPublishedContentRenderer, PublishedContentRenderer>();
        services.AddScoped<IPublishedContentIndexRenderer, PublishedContentIndexRenderer>();
        
        services.AddScoped<PublishedContentXmlSitemapProvider>();
        services.AddScoped<IXmlSitemapSourceProvider, PublishedContentXmlSitemapProvider>();
        services.AddScoped<IXmlSitemapProvider, PublishedContentXmlSitemapProvider>();

        return services;
    }
}

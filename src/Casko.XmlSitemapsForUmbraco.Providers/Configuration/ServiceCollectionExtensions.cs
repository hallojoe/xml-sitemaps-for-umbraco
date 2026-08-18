using Casko.XmlSitemapsForUmbraco.Providers.Routing;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsProviders(this IServiceCollection services)
    {
        services.AddScoped<IHostUrlProvider, HostUrlProvider>();
        services.AddScoped<IXmlSitemapUrlBuilder, XmlSitemapUrlBuilder>();
        services.AddScoped<IXmlSitemapUrlSetRenderer, XmlSitemapUrlSetRenderer>();
        services.AddScoped<IXmlSitemapIndexRenderer, XmlSitemapIndexRenderer>();

        return services;
    }
    
    public static IServiceCollection AddXmlSitemapsCustomProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IXmlSitemapCustomProvider
    {
        services.AddScoped<IXmlSitemapCustomProvider, TProvider>();
        return services;
    }
}

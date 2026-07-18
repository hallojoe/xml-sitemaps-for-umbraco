using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Indexes;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.SitemapRendering.UrlSets;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapProviders(this IServiceCollection services)
    {
        services.AddScoped<IXmlSitemapUrlBuilder, XmlSitemapUrlBuilder>();
        services.AddScoped<IXmlSitemapUrlSetRenderer, XmlSitemapUrlSetRenderer>();
        services.AddScoped<IXmlSitemapIndexRenderer, XmlSitemapIndexRenderer>();

        return services;
    }
    
    public static IServiceCollection AddXmlSitemapCustomProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IXmlSitemapCustomProvider
    {
        services.AddScoped<IXmlSitemapCustomProvider, TProvider>();
        return services;
    }
}

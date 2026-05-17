using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Common.Services.Cms;
using Casko.XmlSitemapsForUmbraco.Common.Services.Rendering;
using Casko.XmlSitemapsForUmbraco.Models.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Common.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemap(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<XmlSitemapsOptions>(configuration.GetSection(XmlSitemapsOptions.Key));
        
        services.AddScoped<ICmsContentService, DefaultCmsContentService>();
        
        services.AddScoped<ISitemapUrlBuilder, SitemapUrlBuilder>();
        
        services.AddScoped<IXmlSitemapContentCollector, XmlSitemapContentCollector>();
        
        services.AddScoped<IXmlSitemapUrlCultureLinkRenderer, XmlSitemapUrlCultureLinkRenderer>();
        services.AddScoped<IXmlSitemapUrlRenderer, XmlSitemapUrlRenderer>();
        services.AddScoped<IXmlSitemapRenderer, XmlSitemapRenderer>();

        services.AddScoped<IXmlSitemapIndexRenderer, XmlSitemapIndexRenderer>();
        services.AddScoped<IXmlSitemapXmlSerializer, XmlSitemapXmlSerializer>();
        services.AddScoped<IXmlSitemapXmlDeserializer, XmlSitemapXmlDeserializer>();

        services.AddScoped<DefaultXmlSiteMapService>();
        services.AddScoped<IXmlSitemapService, DefaultXmlSiteMapService>();
        
        return services;
    }

    public static IServiceCollection AddXmlSitemapCustomProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IXmlSitemapCustomProvider
    {
        services.AddScoped<IXmlSitemapCustomProvider, TProvider>();
        return services;
    }
}

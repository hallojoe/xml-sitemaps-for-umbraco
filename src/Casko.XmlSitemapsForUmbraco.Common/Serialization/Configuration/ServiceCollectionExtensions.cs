using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Common.Serialization.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsSerialization(this IServiceCollection services)
    {
        services.AddScoped<IXmlSitemapXmlSerializer, XmlSitemapXmlSerializer>();
        services.AddScoped<IXmlSitemapXmlDeserializer, XmlSitemapXmlDeserializer>();
        
        return services;
    }
    
}

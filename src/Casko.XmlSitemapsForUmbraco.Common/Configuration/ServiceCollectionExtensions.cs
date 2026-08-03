using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Common.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<XmlSitemapsOptions>(configuration.GetSection(XmlSitemapsOptions.Key));
        
        return services;
    }
    
}

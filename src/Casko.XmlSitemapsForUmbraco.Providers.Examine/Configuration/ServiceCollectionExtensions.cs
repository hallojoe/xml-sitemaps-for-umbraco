using Casko.XmlSitemapsForUmbraco.Common.Providers.Examine.Urls;
using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapExamineProvider(this IServiceCollection services)
    {
        services.AddXmlSitemapProviders();

        services.AddXmlSitemapPublishedContentProvider();
        
        services.AddScoped<ICmsUrlService, ContentUrlService>();

        services.AddScoped<ExamineXmlSitemapProvider>();
        services.AddScoped<IXmlSitemapSourceProvider, ExamineXmlSitemapProvider>();
        services.AddScoped<IXmlSitemapProvider, ExamineXmlSitemapProvider>();

        services.AddScoped<IExamineUrlRenderer, ExamineUrlRenderer>();
        services.AddScoped<IExamineXmlSitemapRenderer, ExamineXmlSitemapRenderer>();
        
        return services;
    }
}

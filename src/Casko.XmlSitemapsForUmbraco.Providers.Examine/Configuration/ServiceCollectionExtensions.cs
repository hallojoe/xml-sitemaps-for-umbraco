using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Routing;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapExamineProvider(this IServiceCollection services, string indexName = Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName)
    {
        services.AddXmlSitemapProviders();
        
        services.AddScoped<IExamineSitemapRootResolver, ExamineSitemapRootResolver>();
        services.AddScoped<ICmsUrlService, ExternalIndexUrlService>();

        // services.AddScoped<ExamineXmlSitemapProvider>();
        if (indexName.Equals(Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName, StringComparison.InvariantCultureIgnoreCase))
        {
            services.AddScoped<IXmlSitemapSourceProvider, ExamineXmlSitemapProvider>();
            services.AddScoped<IXmlSitemapProvider, ExamineXmlSitemapProvider>();
        }
        else
        {
            services.AddScoped<IXmlSitemapSourceProvider, ExamineXmlSitemapProvider>();
            services.AddScoped<IXmlSitemapProvider, ExamineXmlSitemapProvider>();
        }


        services.AddScoped<IExamineUrlRenderer, ExamineUrlRenderer>();
        services.AddScoped<IExamineXmlSitemapRenderer, ExamineXmlSitemapRenderer>();
        
        return services;
    }
}

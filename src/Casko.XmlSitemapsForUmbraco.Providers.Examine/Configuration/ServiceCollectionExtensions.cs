using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Rendering;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Routing;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Urls;
using Microsoft.Extensions.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapExamineProvider(
        this IServiceCollection services,
        string indexName = Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName)
    {
        services.AddXmlSitemapsProviders();
        services.AddScoped<IExamineSitemapRootResolver, ExamineSitemapRootResolver>();

        if (string.Equals(
                indexName,
                Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName,
                StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICmsUrlService, ExternalIndexUrlService>();
        }
        else if (string.Equals(
                     indexName,
                     Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName,
                     StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<ICmsUrlService, DeliveryApiContentIndexUrlService>();
        }
        else
        {
            throw new InvalidOperationException(
                $"XML sitemap Examine index '{indexName}' is not supported. " +
                $"Use '{Umbraco.Cms.Core.Constants.UmbracoIndexes.ExternalIndexName}' or " +
                $"'{Umbraco.Cms.Core.Constants.UmbracoIndexes.DeliveryApiContentIndexName}'.");
        }

        services.AddScoped<IXmlSitemapSourceProvider, ExamineXmlSitemapProvider>();
        services.AddScoped<IXmlSitemapProvider, ExamineXmlSitemapProvider>();

        services.AddScoped<IExamineUrlRenderer, ExamineUrlRenderer>();
        services.AddScoped<IExamineXmlSitemapRenderer, ExamineXmlSitemapRenderer>();
        
        return services;
    }
}

using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapsDeliveryApiRewrites(this IServiceCollection services,
        IConfiguration configuration, bool addRewritePipeline = true)
    {
        if (addRewritePipeline)
        {
            services.AddXmlSitemapsDeliveryApiRewritePipeline(configuration);
        }
        
        return services;
    }

    private static void AddXmlSitemapsDeliveryApiRewritePipeline(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = new XmlSitemapsOptions();
        configuration.GetSection(XmlSitemapsOptions.Key).Bind(settings);
        
        if (!SitemapRewritePipeline.ShouldRegister(settings))
        {
            return;
        }
        
        services.AddSingleton<ISitemapRewriteDefinitionService, SitemapRewriteDefinitionService>();

        services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                XmlSitemapApiConstants.XmlSitemapRewritesKey,
                prePipeline: app => app.UseMiddleware<SitemapRewriteMiddleware>()));
        });
    }
}

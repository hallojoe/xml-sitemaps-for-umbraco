using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Delivery.Controllers;
using Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;
using Casko.XmlSitemapsForUmbraco.Delivery.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXmlSitemapDeliveryApi(this IServiceCollection services,
        IConfiguration configuration, bool addRewritePipeline = true)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(XmlSitemapDeliveryApiController).Assembly);

        services.AddXmlSitemap(configuration);

        services.ConfigureOptions<SiteMapApiConfigureSwaggerGenOptions>();
        services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                $"{XmlSitemapApiConstants.ApiName}-controllers",
                endpoints: app => app.UseEndpoints(endpoints => endpoints.MapControllers())));
        });

        if (addRewritePipeline)
        {
            services.AddXmlSitemapApiRewritePipeline(configuration);
        }
        
        return services;
    }

    private static void AddXmlSitemapApiRewritePipeline(this IServiceCollection services, IConfiguration configuration)
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

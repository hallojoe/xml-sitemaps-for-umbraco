using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Configuration;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddXmlSitemapDeliveryApi(this IUmbracoBuilder builder)
    {
        builder.Services.AddXmlSitemapDeliveryApi(builder.Config);
        return builder;
    }
}
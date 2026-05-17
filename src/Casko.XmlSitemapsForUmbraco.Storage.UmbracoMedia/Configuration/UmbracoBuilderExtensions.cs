using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;

public static class UmbracoBuilderExtensions
{
    public static void AddXmlSitemapsUmbracoMediaStorage(this IUmbracoBuilder builder)
    {
        builder.Services.AddXmlSitemapsUmbracoMediaStorage();
        
    }
}
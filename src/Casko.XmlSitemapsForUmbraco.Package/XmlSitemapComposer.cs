using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Delivery.Configuration;
using Casko.XmlSitemapsForUmbraco.Delivery.Rewriting.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Package;

public sealed class XmlSitemapComposer: IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHybridCache();

        builder.Services.AddXmlSitemapsConfiguration(builder.Config);

        var xmlSitemapsOptions = new XmlSitemapsOptions();
        builder.Config.GetSection(XmlSitemapsOptions.Key).Bind(xmlSitemapsOptions);

        // XML Sitemaps Examine provider and preferred live source.
        builder.Services.AddXmlSitemapExamineProvider(xmlSitemapsOptions.IndexName);
        
        // This wraps the live source and becomes the public IXmlSitemapProvider.
        builder.Services.AddXmlSitemapsUmbracoMediaStorage(builder.Config);
        
        // Delivery API
        builder.Services.AddXmlSitemapDeliveryApi(builder.Config);
        
        // Delivery API rewrites
        builder.Services.AddXmlSitemapsDeliveryApiRewrites(builder.Config, xmlSitemapsOptions.RewritesEnabled);
    }
}

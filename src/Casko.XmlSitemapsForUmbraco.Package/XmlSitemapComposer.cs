using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Delivery.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.Configuration;
using Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Package;

public sealed class XmlSitemapComposer: IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHybridCache();

        builder.Services.AddXmlSitemapConfiguration(builder.Config);

        // XML Sitemaps IPublishedContent provider and source fallback.
        builder.Services.AddXmlSitemapPublishedContentProvider();

        // XML Sitemaps Examine provider and preferred live source.
        builder.Services.AddXmlSitemapExamineProvider();

        // // This wraps the live source and becomes the public IXmlSitemapProvider.
        // builder.Services.AddXmlSitemapsUmbracoMediaStorage();
        
        // Delivery API
        builder.Services.AddXmlSitemapDeliveryApi(builder.Config);
    }
}

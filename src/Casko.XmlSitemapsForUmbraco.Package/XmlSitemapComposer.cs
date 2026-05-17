using Casko.XmlSitemapsForUmbraco.Delivery.Configuration;
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

        builder.AddXmlSitemapDeliveryApi();

        builder.AddXmlSitemapsUmbracoMediaStorage();
    }
}

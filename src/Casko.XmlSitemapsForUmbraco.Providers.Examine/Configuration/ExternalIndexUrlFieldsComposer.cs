using Casko.XmlSitemapsForUmbraco.Common.Providers.Examine.Indexing;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Indexing;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;

public sealed class ExternalIndexUrlFieldsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Components().Append<ExternalIndexUrlFieldsComponent>();
    }
}
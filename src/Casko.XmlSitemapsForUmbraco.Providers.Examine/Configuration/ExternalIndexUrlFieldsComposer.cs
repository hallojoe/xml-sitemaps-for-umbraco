using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Providers.Examine.Indexing;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Configuration;

public sealed class ExternalIndexUrlFieldsComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var indexName = builder.Config.GetValue<string>($"{XmlSitemapsOptions.Key}:IndexName")
                        ?? Constants.UmbracoIndexes.ExternalIndexName;
        if (!string.Equals(indexName, Constants.UmbracoIndexes.ExternalIndexName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        builder.Components().Append<ExternalIndexUrlFieldsComponent>();
    }
}

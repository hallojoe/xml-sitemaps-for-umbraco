using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.SitemapRendering;

public sealed class PublishedContentCollector(IUmbracoContextFactory umbracoContextFactory) : IPublishedContentCollector
{
    public IEnumerable<IPublishedContent> Collect(PublishedContentRenderContext context)
    {
        using var umbracoContextReference = umbracoContextFactory.EnsureUmbracoContext();

            
        foreach (var rootContent in context.RootContents)
        {
            var refreshedRootContent = umbracoContextReference.UmbracoContext.Content.GetByIdAsync(rootContent.Key).GetAwaiter().GetResult();
            if (refreshedRootContent is null)
            {
                continue;
            }

            foreach (var descendant in refreshedRootContent.Descendants().Prepend(refreshedRootContent))
            {
                yield return descendant;
            }
        }
        // return context.RootContents
        // .SelectMany(rootContent => rootContent.Descendants().Prepend(rootContent));
    }
    

    // private IEnumerable<IPublishedContent> Traverse(IPublishedContent content)
    // {
    //     yield return content;
    //
    //     foreach (var child in content.Children())
    //     {
    //         foreach (var descendant in Traverse(child))
    //         {
    //             yield return descendant;
    //         }
    //     }
    // }    
}
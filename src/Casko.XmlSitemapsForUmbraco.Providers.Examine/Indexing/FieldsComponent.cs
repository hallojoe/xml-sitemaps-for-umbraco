using Casko.XmlSitemapsForUmbraco.Common.Providers.Examine.Indexing;
using Examine;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;

namespace Casko.XmlSitemapsForUmbraco.Providers.Examine.Indexing;

public sealed class ExternalIndexUrlFieldsComponent(
    IExamineManager examineManager,
    IContentService contentService,
    ILogger<ExternalIndexUrlFieldsComponent> logger)
    : IAsyncComponent
{
    
    public Task InitializeAsync(bool isRestarting, CancellationToken cancellationToken)
    { 
        if (!examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out var index))
        {
            logger.LogWarning("Could not find Examine index {IndexName}", Constants.UmbracoIndexes.ExternalIndexName);

            return Task.CompletedTask;
        }

        index.TransformingIndexValues += OnTransformingIndexValues;    
        
        return Task.CompletedTask;
    }

    public Task TerminateAsync(bool isRestarting, CancellationToken cancellationToken)
    {
        if (examineManager.TryGetIndex(Constants.UmbracoIndexes.ExternalIndexName, out var index))
        {
            index.TransformingIndexValues -= OnTransformingIndexValues;
        }
        
        return Task.CompletedTask;
    }

    private Guid[] ResolvePathKeys(string path)
    {
        var pathKeys = new List<Guid>();
        var pathParts = path.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pathPart in pathParts)
        {
            if (!int.TryParse(pathPart, out var contentId))
            {
                continue;
            }
            
            var content = contentService.GetById(contentId);

            if (content is null)
            {
                continue;
            }

            pathKeys.Add(content.Key);
        }
        
        return pathKeys.ToArray();   
    }

    private void OnTransformingIndexValues(object? sender, IndexingItemEventArgs e)
    {
        if (e.ValueSet.Category != IndexTypes.Content)
        {
            return;
        }

        if (!int.TryParse(e.ValueSet.Id, out var contentId))
        {
            return;
        }

        var content = contentService.GetById(contentId);
        if (content is null)
        {
            return;
        }

        Dictionary<string, IEnumerable<object>> values = e.ValueSet.Values.ToDictionary(
            x => x.Key,
            x => x.Value.AsEnumerable());

        AddInvariantFields(content, values);

        e.SetValues(values);
    }

    private void AddInvariantFields(
        IContent content,
        Dictionary<string, IEnumerable<object>> values)
    {
        var pathKeys = ResolvePathKeys(content.Path);
        if (pathKeys.Length > 0)
        {
            values[ExternalIndexFieldNameConstants.PathKeys] = pathKeys.Select(x => x.ToString("D"));
        }
    }
}
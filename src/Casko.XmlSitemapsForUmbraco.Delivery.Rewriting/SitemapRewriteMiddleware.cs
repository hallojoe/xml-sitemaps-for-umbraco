using Microsoft.AspNetCore.Http;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Rewriting;

public sealed class SitemapRewriteMiddleware(
    RequestDelegate next,
    ISitemapRewriteDefinitionService rewriteDefinitionService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (rewriteDefinitionService.TryMatch(context.Request.Path, context.Request.Host, out var definition) &&
            definition is not null)
        {
            RewriteRequest(context, definition.TargetPath);
        }

        await next(context);
    }

    private static void RewriteRequest(HttpContext context, string targetPath)
    {
        var querySeparatorIndex = targetPath.IndexOf('?', StringComparison.Ordinal);
        if (querySeparatorIndex < 0)
        {
            context.Request.Path = targetPath;
            context.Request.QueryString = QueryString.Empty;
            return;
        }

        context.Request.Path = targetPath[..querySeparatorIndex];
        context.Request.QueryString = QueryString.FromUriComponent(targetPath[querySeparatorIndex..]);
    }
}

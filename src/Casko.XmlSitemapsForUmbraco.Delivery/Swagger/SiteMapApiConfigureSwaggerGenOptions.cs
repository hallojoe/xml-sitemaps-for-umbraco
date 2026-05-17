using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Swagger;

public sealed class SiteMapApiConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc($"{XmlSitemapApiConstants.ApiName}", new OpenApiInfo
        {
            Title = XmlSitemapApiConstants.ApiTitle,
            Version = XmlSitemapApiConstants.ApiVersion
        });
    }
}

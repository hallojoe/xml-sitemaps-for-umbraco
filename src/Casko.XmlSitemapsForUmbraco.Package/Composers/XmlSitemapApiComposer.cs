using Asp.Versioning;
using Casko.XmlSitemapsForUmbraco.Package.Controllers;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Package.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Cms.Web.Common.Authorization;

namespace Casko.XmlSitemapsForUmbraco.Package.Composers;

public class XmlSitemapApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(XmlSitemapsApiController).Assembly);

        builder.Services.AddSingleton<IOperationIdHandler, CaskoSitemapsForUmbracoOperationHandler>();

        builder.Services.Configure<UmbracoPipelineOptions>(options =>
        {
            options.AddFilter(new UmbracoPipelineFilter(
                $"{XmlSitemapConstants.ApiName}-configuration-api",
                postPipeline: app => app.Use(async (context, next) =>
                {
                    if (HttpMethods.IsGet(context.Request.Method)
                        && context.Request.Path.Equals(
                            "/umbraco/backoffice/xmlsitemapsforumbraco/api/v1/configuration",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (context.User.Identity?.IsAuthenticated is not true)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }

                        XmlSitemapsOptions xmlSitemapOptions = context.RequestServices
                            .GetRequiredService<IOptions<XmlSitemapsOptions>>()
                            .Value;

                        await context.Response.WriteAsJsonAsync(
                            XmlSitemapConfigurationResponse.FromOptions(xmlSitemapOptions));
                        return;
                    }

                    await next();
                }),
                endpoints: app => app.UseEndpoints(endpoints =>
                {
                    endpoints
                        .MapGet(
                            "/umbraco/backoffice/charlietangoumbracoxmlsitemap/api/v1/configuration",
                            (IOptions<XmlSitemapsOptions> xmlSitemapOptions) =>
                                Results.Ok(XmlSitemapConfigurationResponse.FromOptions(xmlSitemapOptions.Value)))
                        .RequireAuthorization(AuthorizationPolicies.SectionAccessContent)
                        .WithGroupName(XmlSitemapConstants.ApiName)
                        .WithName("GetConfiguration")
                        .Produces<XmlSitemapConfigurationResponse>();
                })));
        });

        builder.Services.Configure<SwaggerGenOptions>(opt =>
        {
            // Related documentation:
            // URL: https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api

            // Configure the Swagger generation options
            // Add in a new Swagger API document solely for our own package that can be browsed via Swagger UI
            // Along with having a generated swagger JSON file that we can use to auto generate a TypeScript client
            opt.SwaggerDoc(XmlSitemapConstants.ApiName, new OpenApiInfo
            {
                Title = XmlSitemapConstants.ApiTitle,
                Version = XmlSitemapConstants.ApiVersion,
                Contact = new OpenApiContact
                {
                    Name = XmlSitemapConstants.ApiAuthors,
                    Email = XmlSitemapConstants.ApiContactEmail,
                    Url = new Uri(XmlSitemapConstants.ApiOrganizationUrl)
                }
            });

            // Enable Umbraco authentication for the Swagger document
            // PR: https://github.com/umbraco/Umbraco-CMS/pull/15699
            opt.OperationFilter<CharlieTangoUmbracoXmlSitemapOperationSecurityFilter>();
        });
    }

    public class
        CharlieTangoUmbracoXmlSitemapOperationSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => XmlSitemapConstants.ApiName;
    }

    // This is used to generate pretty operation IDs in our swagger JSON file.
    // So the generated TypeScript client has pretty method names and not too verbose
    // URL: https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
    public class CaskoSitemapsForUmbracoOperationHandler : OperationIdHandler
    {
        public CaskoSitemapsForUmbracoOperationHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) :
            base(apiVersioningOptions)
        {
        }

        protected override bool CanHandle(ApiDescription apiDescription,
            ControllerActionDescriptor controllerActionDescriptor)
        {
            return controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith(XmlSitemapConstants.ControllerNamespace,
                comparisonType: StringComparison.InvariantCultureIgnoreCase) is true;
        }

        public override string Handle(ApiDescription apiDescription) =>
            $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
    }
}

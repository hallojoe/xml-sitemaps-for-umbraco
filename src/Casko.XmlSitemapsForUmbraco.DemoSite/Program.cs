using System.Net;
using Casko.XmlSitemapsForUmbraco.DemoSite;
using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Microsoft.AspNetCore.HttpOverrides;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load XML sitemap settings before Umbraco composers run so rewrite middleware
// registration can see RewritesEnabled and the host-specific sitemap mappings.
builder.Configuration
    .AddJsonFile("appsettings.XmlSitemapsForUmbraco.json", optional: false, reloadOnChange: true);

var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

if (isDevelopment)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedHost |
            ForwardedHeaders.XForwardedProto;

        options.KnownProxies.Add(IPAddress.Loopback);
        options.KnownProxies.Add(IPAddress.IPv6Loopback);
    });
}

builder.Services.AddXmlSitemapsCustomProvider<CustomSitemapProvider>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

if (isDevelopment)
{
    app.UseForwardedHeaders();
}

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
{
    u.UseBackOffice();
    u.UseWebsite();
})
.WithEndpoints(u =>
{
    u.UseBackOfficeEndpoints();
    u.UseWebsiteEndpoints();
});

await app.RunAsync();

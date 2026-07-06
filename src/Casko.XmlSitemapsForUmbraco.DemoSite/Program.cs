WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load XML sitemap settings before Umbraco composers run so rewrite middleware
// registration can see RewritesEnabled and the host-specific sitemap mappings.
builder.Configuration
    .AddJsonFile("appsettings.XmlSitemapsForUmbraco.json", optional: false, reloadOnChange: true);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

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

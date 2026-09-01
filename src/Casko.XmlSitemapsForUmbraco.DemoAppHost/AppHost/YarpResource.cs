namespace Casko.XmlSitemapsForUmbraco.DemoAppHost.AppHost;

internal static class YarpResourceExtensions
{
    public static void AddYarpResource(
        this IDistributedApplicationBuilder builder,
        UmbracoResources sites,
        IResourceBuilder<DashboardGroupResource> group)
    {
        builder
            .AddProject<Projects.Casko_XmlSitemapsForUmbraco_DemoSiteReverseProxy>(
                "yarp",
                launchProfileName: "LocalReverseProxy")
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables["ReverseProxy__Clusters__single-site__Destinations__default__Address"] = sites.SingleSite.GetEndpoint("https");
                context.EnvironmentVariables["ReverseProxy__Clusters__mixed-site__Destinations__default__Address"] = sites.MixedSite.GetEndpoint("https");
                context.EnvironmentVariables["ReverseProxy__Clusters__language-variant-site__Destinations__default__Address"] = sites.LanguageVariantSite.GetEndpoint("https");
                context.EnvironmentVariables["ReverseProxy__Clusters__many-1-site__Destinations__default__Address"] = sites.ManySite.GetEndpoint("https");
                context.EnvironmentVariables["ReverseProxy__Clusters__many-2-site__Destinations__default__Address"] = sites.ManySite.GetEndpoint("https");
            })
            .WithReference(sites.SingleSite)
            .WithReference(sites.MixedSite)
            .WithReference(sites.LanguageVariantSite)
            .WithReference(sites.ManySite)
            .WithParentRelationship(group);
    }
}

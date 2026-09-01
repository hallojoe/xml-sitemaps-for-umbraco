using Casko.XmlSitemapsForUmbraco.DemoAppHost.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var groups = builder.AddDashboardGroups();
var databases = builder.AddDatabaseResources(groups.Database);
var sites = builder.AddUmbracoResources(databases, groups.Umbraco);

builder.AddYarpResource(sites, groups.Network);

builder.Build().Run();

namespace Casko.XmlSitemapsForUmbraco.DemoAppHost.AppHost;

internal static class DatabaseResourceExtensions
{
    public static DatabaseResources AddDatabaseResources(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<DashboardGroupResource> group)
    {
        var sql = builder
            .AddSqlServer("sql", port: 11433)
            .WithImage("azure-sql-edge")
            .WithImageRegistry("mcr.microsoft.com")
            .WithDataVolume("xml-sitemaps-for-umbraco-sql-data")
            .WithParentRelationship(group);

        return new DatabaseResources(
            sql,
            sql.AddDatabase("single-site-db", "xml-sitemaps-single-site"),
            sql.AddDatabase("mixed-site-db", "xml-sitemaps-mixed-site"),
            sql.AddDatabase("language-variant-site-db", "xml-sitemaps-language-variant-site"),
            sql.AddDatabase("many-site-db", "xml-sitemaps-many-site"));
    }
}

internal sealed record DatabaseResources(
    IResourceBuilder<SqlServerServerResource> Sql,
    IResourceBuilder<SqlServerDatabaseResource> SingleSite,
    IResourceBuilder<SqlServerDatabaseResource> MixedSite,
    IResourceBuilder<SqlServerDatabaseResource> LanguageVariantSite,
    IResourceBuilder<SqlServerDatabaseResource> ManySite);

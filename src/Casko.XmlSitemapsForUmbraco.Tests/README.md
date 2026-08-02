---
project: Casko.XmlSitemapsForUmbraco.Tests
type: test
language: C#
framework: net10.0
solution_role: NUnit unit and integration tests for XML sitemaps package behavior
depends_on:
  - Casko.XmlSitemapsForUmbraco.Delivery
  - Casko.XmlSitemapsForUmbraco.Providers
  - Casko.XmlSitemapsForUmbraco.Providers.Examine
  - Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
  - Casko.XmlSitemapsForUmbraco.Storage
  - Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
  - Casko.XmlSitemapsForUmbraco.TestSite
used_by: []
---

# Casko.XmlSitemapsForUmbraco.Tests

## Purpose

This project contains the automated NUnit test coverage for the XML sitemaps package. It exercises unit-level services, renderers, serializers, middleware, package API responses, storage behavior, and integration routes hosted through Umbraco test infrastructure.

It is the main verification project for changes across the package's internal projects and the local `TestSite`.

## Responsibilities

- Test sitemap rendering, content/culture/property filtering, custom provider behavior, and root-key behavior.
- Test XML serialization and HTTP XML result behavior.
- Test storage refresh, stored sitemap provider behavior, media data source behavior, storage naming, and media refresh background jobs.
- Test rewrite definition and middleware behavior.
- Test service composition and package configuration response mapping.
- Run integration checks for delivery API routing and Swagger delivery API paths through `UmbracoTestServerBase`.
- Create and clean up test fixture state through `TestAssemblySetup`.

## Non-responsibilities

- This project does not ship package code or assets.
- This project should not own reusable test-site content; Umbraco site state belongs in `../Casko.XmlSitemapsForUmbraco.TestSite`.
- This project should not replace focused tests in future project-specific test assemblies if a project later gets its own dedicated test project.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Delivery
Casko.XmlSitemapsForUmbraco.Providers
Casko.XmlSitemapsForUmbraco.Providers.Examine
Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
Casko.XmlSitemapsForUmbraco.Storage
Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
Casko.XmlSitemapsForUmbraco.TestSite
       |
       v
Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project                                                                                                                   | Reason |
|---------------------------------------------------------------------------------------------------------------------------|---|
| `../Casko.XmlSitemapsForUmbraco.Delivery/Casko.XmlSitemapsForUmbraco.Delivery.csproj`                                     | Tests delivery API routing, access behavior, and service composition. |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj`                                   | Tests sitemap rendering and provider-level behavior. |
| `../Casko.XmlSitemapsForUmbraco.Providers.Examine/Casko.XmlSitemapsForUmbraco.Providers.Examine.csproj`                   | Tests Examine-backed sitemap provider behavior. |
| `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent/Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.csproj` | Tests published-content rendering and selection behavior. |
| `../Casko.XmlSitemapsForUmbraco.Storage/Casko.XmlSitemapsForUmbraco.Storage.csproj`                                       | Tests stored sitemap provider, refresh service, and storage-name behavior. |
| `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia/Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.csproj`             | Tests Umbraco media storage data source and background job behavior. |
| `../Casko.XmlSitemapsForUmbraco.TestSite/Casko.XmlSitemapsForUmbraco.DemoSingleSite.csproj`                               | Supplies the local Umbraco host and content/configuration surface for integration-style coverage. |

### Used by

This project has no direct project consumers.

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Tests.csproj` | Defines test dependencies, project references, coverlet settings, copied appsettings, and temp directory creation targets. |
| `TestAssemblySetup.cs` | Creates `TEMP` folders and runs Umbraco integration global setup/teardown once for the assembly. |
| `appsettings.Tests.json` | SQLite and Umbraco test settings used by integration test infrastructure. |
| `appsettings.Tests.Local.json` | Local test settings copied to output when present. |
| `Integration/UmbracoTestServerBase.cs` | Configures API versioning, test authentication, and content-section authorization for integration tests. |
| `Integration/XmlSitemapDeliveryApiIntegrationTests.cs` | Verifies delivery API direct routes, rewrite routes, and Swagger delivery API paths. |
| `Unit` | Contains focused NUnit tests for serializers, rendering, filtering, storage, middleware, data sources, package API responses, and composition. |

## Testing strategy

The project mixes fast unit tests with Umbraco integration test infrastructure. Unit tests use NUnit and NSubstitute where appropriate. Integration tests derive from `UmbracoTestServerBase`, which supplies test authentication and an authorization policy matching Umbraco content-section access.

`CoverletExclude` excludes `Casko.XmlSitemapsForUmbraco.TestSite` from coverage in the test project file.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

The project creates `TEMP/media`, `TEMP/cache`, `TEMP/logs`, and `TEMP/databases` under the test project during build or assembly setup.

## Agent guidance

When modifying this project:

1. Add or update tests close to the behavior being changed: serializer tests for XML shape, rendering tests for URL output, storage tests for persisted XML behavior, and integration tests for routing.
2. Keep integration tests isolated through `UmbracoTestServerBase`; do not depend on interactive backoffice login.
3. Keep test settings and copied appsettings aligned with Umbraco integration test requirements.
4. Avoid asserting transitive project relationships in this README; list only direct `ProjectReference` entries.
5. Update this README when test fixtures, referenced projects, copied settings, or major test categories change.

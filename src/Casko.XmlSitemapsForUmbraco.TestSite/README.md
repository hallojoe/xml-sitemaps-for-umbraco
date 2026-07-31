---
project: Casko.XmlSitemapsForUmbraco.TestSite
type: web-application
language: C#
framework: net10.0
solution_role: Local Umbraco host for integration tests and project-reference package validation
depends_on:
  - Casko.XmlSitemapsForUmbraco.Models
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Providers
used_by:
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.TestSite

## Purpose

This project is a local Umbraco web application used to host the XML sitemaps package from source. It gives tests and maintainers a real Umbraco runtime with package composers, delivery API, rewrite routes, uSync content, and sample sitemap configuration.

Unlike the demo site, this project references local package projects directly so changes can be validated before packaging or publishing.

## Responsibilities

- Boot an Umbraco 17 site with backoffice, website, composers, and delivery API enabled in `Program.cs`.
- Reference the local `Casko.XmlSitemapsForUmbraco.Package` project so Umbraco discovers the package composer during local runs.
- Register `DummyCustomSitemapProvider` as a custom sitemap provider through `AddXmlSitemapCustomProvider<DummyCustomSitemapProvider>()`.
- Provide test sitemap settings under the `XmlSitemaps` section in `appsettings.json`.
- Provide uSync content, languages, domains, media, data types, and content types under `uSync/v17`.
- Provide stable local URLs through `Properties/launchSettings.json`, including `https://localhost:44341`.

## Non-responsibilities

- This project is not the NuGet package entry point; package composition and backoffice assets belong in `../Casko.XmlSitemapsForUmbraco.Package`.
- This project should not contain reusable sitemap domain logic. Shared behavior belongs in the package, provider, storage, serialization, delivery, or model projects.
- This project is not a public sample of consuming the published package. Use `../Casko.XmlSitemapsForUmbraco.DemoSite` for package-reference behavior.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Models
Casko.XmlSitemapsForUmbraco.Package
Casko.XmlSitemapsForUmbraco.Providers
       |
       v
Casko.XmlSitemapsForUmbraco.TestSite
       ^
       |
Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project | Reason |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Models/Casko.XmlSitemapsForUmbraco.Models.csproj` | Supplies sitemap model types used by `DummyCustomSitemapProvider`. |
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Loads the local package composer, API, and backoffice assets into the Umbraco host. |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj` | Supplies custom provider abstractions and registration extensions used by the test site. |

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj` | Uses the site as the Umbraco host for integration-style test coverage. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.TestSite.csproj` | Defines the Umbraco web host, local project references, uSync, ICU, and Razor build settings. |
| `Program.cs` | Boots Umbraco, registers delivery API and composers, and wires backoffice and website endpoints. |
| `DummyCustomSitemapProvider.cs` | Provides deterministic custom sitemap output for local and test configuration. |
| `appsettings.json` | Main test-site Umbraco and `XmlSitemaps` configuration. |
| `appsettings.Development.json` | Development database, unattended install, uSync import/export, and logging settings. |
| `Properties/launchSettings.json` | Local launch profiles and ports. |
| `uSync/v17` | Serialized Umbraco content, languages, domains, media, data types, and content types used by the site. |

## Configuration

The main sitemap configuration lives under `XmlSitemaps` in `appsettings.json`. It enables sitemap generation and rewrites, configures cultures `en`, `da`, and `pl`, defines `xmlsitemapindex`, configures the `dummy-custom-sitemap` custom sitemap, and defines several content sitemap variants.

The custom provider alias is `dummy-custom-sitemap-provider`, which must stay aligned between `DummyCustomSitemapProvider.Alias` and `XmlSitemaps:CustomSitemaps:dummy-custom-sitemap:ProviderAlias`.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.TestSite/Casko.XmlSitemapsForUmbraco.TestSite.csproj
dotnet run --project src/Casko.XmlSitemapsForUmbraco.TestSite/Casko.XmlSitemapsForUmbraco.TestSite.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

The test project creates `TEMP` folders used by Umbraco integration test infrastructure. Local site runs may also create database, media, cache, log, and uSync-generated artifacts.

## Agent guidance

When modifying this project:

1. Keep the local project-reference setup aligned with tests that need source-level package behavior.
2. Keep `DummyCustomSitemapProvider.Alias` synchronized with `appsettings.json`.
3. Be careful when changing uSync content, domains, languages, or launch ports because integration tests and configured host names may depend on them.
4. Keep reusable package behavior out of the test site; add it to the relevant library project and reference it here only for hosting.
5. Update this README when project references, local ports, custom provider aliases, or major sitemap configuration keys change.

---
project: Casko.XmlSitemapsForUmbraco.Package
type: library
language: C#
framework: net10.0
solution_role: NuGet package assembly, Umbraco composer, backoffice API, and bundled backoffice client
depends_on:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Delivery
  - Casko.XmlSitemapsForUmbraco.Models
  - Casko.XmlSitemapsForUmbraco.Providers
  - Casko.XmlSitemapsForUmbraco.Providers.Examine
  - Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
  - Casko.XmlSitemapsForUmbraco.Storage
  - Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
used_by:
  - Casko.XmlSitemapsForUmbraco.Demo*Site
---

# Casko.XmlSitemapsForUmbraco.Package

## Purpose

This project is the package-facing assembly for XML Sitemaps for Umbraco. It composes the internal implementation projects into one installable Umbraco package, exposes a backoffice management API, and ships the bundled backoffice client assets under `wwwroot/App_Plugins/XmlSitemapsForUmbraco`.

It also carries NuGet metadata and includes `../../docs/README_nuget.md` as the package README.

## Responsibilities

- Compose package services through `XmlSitemapComposer`, including configuration, published-content provider, Examine provider, Umbraco media storage, and delivery API registration.
- Configure a package-specific Swagger document and operation IDs through `Composers/XmlSitemapApiComposer.cs`.
- Expose a content-section-protected backoffice API route under `xmlsitemapsforumbraco/api/v{version:apiVersion}`.
- Return a read-only sitemap configuration summary from `XmlSitemapsApiController.GetConfiguration()`.
- Map `XmlSitemapsOptions` into backoffice response records in `Models/XmlSitemapConfigurationResponse.cs`.
- Build and ship Umbraco backoffice extension manifests and Lit workspace UI from `Client`.
- Include referenced project build output in the NuGet package through `IncludeProjectReferenceBuildOutput`.

## Non-responsibilities

- This project should not own low-level sitemap rendering, storage, serialization, or delivery behavior. Those concerns belong in the referenced implementation projects.
- This project should not contain test-site or demo-site content.
- The generated client files under `Client/src/api` should not be hand-edited when they are regenerated from OpenAPI.
- The built files under `wwwroot/App_Plugins/XmlSitemapsForUmbraco` are package assets; source TypeScript changes belong under `Client/src`.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Common
Casko.XmlSitemapsForUmbraco.Delivery
Casko.XmlSitemapsForUmbraco.Models
Casko.XmlSitemapsForUmbraco.Providers
Casko.XmlSitemapsForUmbraco.Providers.Examine
Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
Casko.XmlSitemapsForUmbraco.Storage
Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
       |
       v
Casko.XmlSitemapsForUmbraco.Package
       ^
       |
Casko.XmlSitemapsForUmbraco.TestSite
```

### Dependencies

| Project | Reason                                                                                             |
|---|----------------------------------------------------------------------------------------------------|
| `../Casko.XmlSitemapsForUmbraco.Common/Casko.XmlSitemapsForUmbraco.Common.csproj` | Provides configuration options and shared services used by package API responses and composition. Included in the packaged runtime output for XML HTTP delivery support. Included for XML serialization and deserialization support. |
| `../Casko.XmlSitemapsForUmbraco.Delivery/Casko.XmlSitemapsForUmbraco.Delivery.csproj` | Registers public delivery API behavior through the package composer.                               |
| `../Casko.XmlSitemapsForUmbraco.Models/Casko.XmlSitemapsForUmbraco.Models.csproj` | Included in the packaged runtime output for sitemap model contracts.                               |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj` | Provides provider abstractions and core sitemap rendering services.                                |
| `../Casko.XmlSitemapsForUmbraco.Providers.Examine/Casko.XmlSitemapsForUmbraco.Providers.Examine.csproj` | Registers the Examine-backed sitemap provider.                                                     |
| `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent/Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.csproj` | Registers the published-content sitemap provider and fallback source.                              |
| `../Casko.XmlSitemapsForUmbraco.Storage/Casko.XmlSitemapsForUmbraco.Storage.csproj` | Included for stored sitemap provider and refresh behavior.                                         |
| `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia/Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.csproj` | Registers Umbraco media-backed storage as the public sitemap provider wrapper.                     |

### Used by

| Project                                                                                | Usage                                                                                                                               |
|----------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `../Casko.XmlSitemapsForUmbraco.TestSite/Casko.XmlSitemapsForUmbraco.Demo*Site.csproj` | Sites (in different configurations) hosts the local package project through a direct project reference for source-level validation. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Package.csproj` | Defines NuGet metadata, package README inclusion, static web asset path, package references, project references, and build-output packaging. |
| `XmlSitemapComposer.cs` | Main Umbraco composer that registers package services. |
| `Composers/XmlSitemapApiComposer.cs` | Registers package Swagger/OpenAPI document, security filter, and operation ID handler. |
| `Controllers/XmlSitemapsApiControllerBase.cs` | Defines the backoffice API route, API mapping, and authorization boundary. |
| `Controllers/XmlSitemapsApiController.cs` | Exposes `GET configuration` for the backoffice configuration view. |
| `Models/XmlSitemapConfigurationResponse.cs` | Maps configured options to read-only API response records for the backoffice UI. |
| `XmlSitemapConstants.cs` | Centralizes package API names, Swagger metadata, and controller namespace constants. |
| `Properties/AssemblyInfo.cs` | Grants internals access to the test assembly and dynamic proxy generation. |
| `Client/package.json` | Defines TypeScript build, watch, and OpenAPI client generation scripts. |
| `Client/src/bundle.manifests.ts` | Collates Umbraco backoffice extension manifests. |
| `Client/src/workspace` | Defines the XML Sitemaps workspace, workspace view, menu item, and Lit configuration UI. |
| `Client/src/api` | Generated TypeScript API client used by the workspace UI. |
| `Client/public/umbraco-package.json` | Defines the package bundle loaded by Umbraco. |
| `wwwroot/App_Plugins/XmlSitemapsForUmbraco` | Built JavaScript assets shipped with the package. |

## Backoffice API and client

The backoffice API is mapped to the API name `xmlsitemapsforumbraco`. `XmlSitemapsApiController.GetConfiguration()` returns `XmlSitemapConfigurationResponse` and requires Umbraco content-section access through `AuthorizationPolicies.SectionAccessContent`.

The TypeScript client is generated by `Client/scripts/generate-openapi.js` from a running Umbraco Swagger endpoint. The `generate-client` npm script currently targets:

```bash
https://localhost:44341/umbraco/swagger/xmlsitemapsforumbraco/swagger.json
```

The Vite build outputs extension assets into `wwwroot/App_Plugins/XmlSitemapsForUmbraco`.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj
dotnet pack src/Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

From `src/Casko.XmlSitemapsForUmbraco.Package/Client`:

```bash
npm run build
npm run watch
npm run generate-client
```

`npm run generate-client` requires a running Umbraco site that exposes the package Swagger JSON at the configured URL.

## Agent guidance

When modifying this project:

1. Keep composition in `XmlSitemapComposer` thin; add implementation behavior to the underlying library projects.
2. Update `XmlSitemapConstants`, Swagger configuration, generated TypeScript client, and tests together when changing package API routes or response contracts.
3. Do not hand-edit generated files in `Client/src/api` unless the task is specifically about generated output.
4. Rebuild `Client` when changing extension manifests or Lit workspace source so `wwwroot/App_Plugins/XmlSitemapsForUmbraco` stays in sync.
5. Keep package metadata and `../../docs/README_nuget.md` aligned when changing the NuGet-facing package story.
6. Update this README when direct project references, package composition, API routes, or backoffice client build flow change.

---
project: Casko.XmlSitemapsForUmbraco.Providers.Examine
type: library
language: C#
framework: net10.0
solution_role: Examine-backed sitemap source provider and URL index integration
depends_on:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Providers
  - Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
used_by:
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Providers.Examine

## Purpose

This project provides an Examine-backed sitemap source provider. It uses Umbraco's `ExternalIndex` to resolve indexed content URLs for a root content key, then renders those URLs into sitemap models using the shared provider contracts and sitemap renderers.

It is intended as the preferred live source provider registered by the package composer, while still relying on the published-content provider project for path/root resolution helpers and shared public-name behavior.

## Responsibilities

- Implement `IXmlSitemapSourceProvider` through `ExamineXmlSitemapProvider`.
- Resolve configured content sitemaps, custom sitemaps, root-key requests, path requests, and sitemap indexes.
- Read indexed URLs through `ICmsUrlService` / `ContentUrlService`.
- Render `CmsUrl` records into `XmlSitemapUrl` entries with primary URLs and alternate culture links.
- Add `pathKeys` values to Umbraco's `ExternalIndex` content items through `ExternalIndexUrlFieldsComponent`.
- Register the Examine provider, renderers, URL service, and supporting published-content services through `AddXmlSitemapExamineProvider()`.
- Append the index field component through `ExternalIndexUrlFieldsComposer`.

## Non-responsibilities

- This project does not define the sitemap model types; those come through `../Casko.XmlSitemapsForUmbraco.Providers` and `../Casko.XmlSitemapsForUmbraco.Models`.
- This project does not own the canonical published-content path lookup behavior; it reuses `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent`.
- This project does not serialize XML, store generated sitemap XML, or expose HTTP endpoints.
- This project should not contain package composition beyond its own provider registration and Umbraco component registration.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Common
Casko.XmlSitemapsForUmbraco.Providers
Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
       |
       v
Casko.XmlSitemapsForUmbraco.Providers.Examine
       ^
       |
       +-- Casko.XmlSitemapsForUmbraco.Package
       +-- Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project | Reason |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Common/Casko.XmlSitemapsForUmbraco.Common.csproj` | Supplies `XmlSitemapsOptions`, shared exceptions, and Examine URL/indexing support types used by this provider. |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj` | Supplies provider contracts, custom provider contracts, sitemap render contexts, and shared URL set/index rendering. |
| `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent/Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.csproj` | Supplies `IPublishedContentService`, culture selection, public sitemap-name resolution, and published-content provider registration reused by the Examine provider. |

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Registers this provider as the preferred live sitemap source in package composition. |
| `../Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj` | Tests Examine provider behavior, URL rendering, and service composition. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Providers.Examine.csproj` | Defines the `net10.0` library, Umbraco/Options package references, and direct project references. |
| `ExamineXmlSitemapProvider.cs` | Main `IXmlSitemapSourceProvider` implementation for root-key, path, configured sitemap, custom sitemap, and index access. |
| `Configuration/ServiceCollectionExtensions.cs` | Registers shared provider services, published-content services, `ContentUrlService`, `ExamineXmlSitemapProvider`, and Examine renderers. |
| `Configuration/ExternalIndexUrlFieldsComposer.cs` | Appends `ExternalIndexUrlFieldsComponent` to Umbraco composition. |
| `Indexing/FieldsComponent.cs` | Adds `pathKeys` values to ExternalIndex content entries during `TransformingIndexValues`. |
| `Indexing/Constants.cs` | Defines the `pathKeys` Examine field name. |
| `Urls/UrlService.cs` | Defines `UrlResolverSettings`, `CmsUrl`, `ICmsUrlService`, and the `ContentUrlService` implementation that pages through `ExternalIndex` results. |
| `Rendering/ExamineXmlSitemapRenderer.cs` | Converts an Examine render context into an `XmlSitemap` through URL rendering and URL set rendering. |
| `Rendering/ExamineUrlRenderer.cs` | Groups `CmsUrl` records, selects primary URLs, builds alternate culture links, and normalizes host/path output. |
| `Rendering/ExamineXmlSitemapRenderContext.cs` | Carries indexed URLs, default culture, alternate cultures, host name, and alternate-link behavior into rendering. |

## Public API

`AddXmlSitemapExamineProvider()` is the main registration entry point. It registers the shared provider renderers, published-content provider services, `ICmsUrlService`, `ExamineXmlSitemapProvider`, and Examine renderers.

`ExamineXmlSitemapProvider` implements `IXmlSitemapSourceProvider`, so consumers can use the standard provider methods:

- `GetByRootKey` / `GetByRootKeyAsync`
- `GetByPath` / `GetByPathAsync`
- `GetConfigured` / `GetConfiguredAsync`
- `GetIndex` / `GetIndexAsync`

`ExternalIndexUrlFieldsComposer` is the Umbraco composition entry point that registers the component responsible for adding the `pathKeys` index field used by `ContentUrlService`.

## Configuration

This project reads `XmlSitemapsOptions` for configured sitemaps, custom sitemaps, sitemap indexes, culture filtering, alternate-link behavior, and public sitemap names.

`UrlResolverSettings` uses configuration key `Casko:Search:Url` and currently exposes `PageSize`, defaulting to `1000`.

`ContentUrlService` builds URL records from Umbraco's configured languages, ordered with the default language first, then applies `XmlSitemapsOptions.IncludedCultures` and `ExcludedCultures`.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Providers.Examine/Casko.XmlSitemapsForUmbraco.Providers.Examine.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

There is no dedicated test project for this library. Existing tests cover it through `../Casko.XmlSitemapsForUmbraco.Tests`, especially `ExamineXmlSitemapProviderTests` and `ServiceCompositionTests`.

## Agent guidance

When modifying this project:

1. Keep `pathKeys` indexing behavior aligned between `ExternalIndexUrlFieldsComponent` and `ContentUrlService`.
2. Inspect `ExamineXmlSitemapProviderTests` before changing configured sitemap lookup, custom sitemap fallback, culture filtering, URL grouping, or alternate-link rendering.
3. Keep published-content path/root resolution concerns in `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent`; use this project for Examine-backed URL lookup and rendering.
4. Keep DI registration in `Configuration/ServiceCollectionExtensions.cs` aligned with provider and renderer constructor dependencies.
5. Update this README when direct project references, registration entry points, index field names, URL resolver settings, or rendering rules change.

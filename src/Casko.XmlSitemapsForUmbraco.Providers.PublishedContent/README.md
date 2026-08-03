---
project: Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
type: library
language: C#
framework: net10.0
solution_role: Umbraco published-content sitemap provider and renderers
depends_on:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Providers
used_by:
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Providers.PublishedContent

## Purpose

This project turns Umbraco published content into sitemap models. It resolves root content by key, path, host name, and culture; applies sitemap configuration filters; renders URL sets and sitemap indexes; and supports configured custom sitemap providers.

It is the default live content provider layer used before storage, caching, delivery, or package composition wraps the sitemap output.

## Responsibilities

- Implement `IXmlSitemapProvider` and `IXmlSitemapSourceProvider` through `PublishedContentXmlSitemapProvider`.
- Resolve Umbraco content and languages through `IPublishedContentService` and `PublishedContentService`.
- Apply sitemap configuration from `XmlSitemapsOptions`, including included/excluded cultures, included/excluded content type aliases, `RootNodeSearchLevel`, and URL-exclusion property settings.
- Render published content and descendants into `XmlSitemap` URL sets.
- Render configured sitemap indexes with public sitemap names and legacy `.xml` locations.
- Resolve configured custom sitemaps through registered `IXmlSitemapCustomProvider` implementations.
- Register published-content provider services through `AddXmlSitemapPublishedContentProvider()`.

## Non-responsibilities

- This project does not define sitemap model contracts; those belong in `../Casko.XmlSitemapsForUmbraco.Models`.
- This project does not own the shared provider interfaces or generic sitemap renderers; those belong in `../Casko.XmlSitemapsForUmbraco.Providers`.
- This project does not serialize XML or return HTTP responses.
- This project does not store generated sitemap XML in Umbraco media; storage behavior belongs in `../Casko.XmlSitemapsForUmbraco.Storage` and `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia`.
- This project does not query Examine indexes. Examine-backed provider behavior belongs in `../Casko.XmlSitemapsForUmbraco.Providers.Examine`.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Common
Casko.XmlSitemapsForUmbraco.Providers
       |
       v
Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
       ^
       |
       +-- Casko.XmlSitemapsForUmbraco.Package
       +-- Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project | Reason |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Common/Casko.XmlSitemapsForUmbraco.Common.csproj` | Supplies `XmlSitemapsOptions`, `SitemapOptions`, and shared exceptions/configuration used while resolving configured sitemaps. |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj` | Supplies provider contracts, custom provider contracts, render contexts, and shared URL set/index renderers. |

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Registers this provider as part of package composition. |
| `../Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj` | Tests content selection, published-content service behavior, rendering, custom configured sitemaps, and composition. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.csproj` | Defines the `net10.0` library and direct references to `Common` and `Providers`. |
| `PublishedContentXmlSitemapProvider.cs` | Main provider implementation for root-key, path, configured sitemap, configured custom sitemap, and index access. |
| `Configuration/ServiceCollectionExtensions.cs` | Registers shared provider services plus published-content readers, renderers, and provider implementations. |
| `Configuration/SitemapPublicName.cs` | Resolves public sitemap names from configuration keys and optional public-name settings. |
| `ContentReading/IPublishedContentService.cs` | Abstraction over Umbraco content, root content, path lookup, and language lookup. |
| `ContentReading/PublishedContentService.cs` | Umbraco-backed implementation using context, document URL, document navigation, language, and URL services. |
| `../Casko.XmlSitemapsForUmbraco.Providers/SitemapRendering/Contexts/SitemapCultureSelection.cs` | Shared helper that resolves included cultures and whether alternate links should render. |
| `ContentReading/SitemapContentTypeSelection.cs` | Resolves root and per-sitemap content type include/exclude filters. |
| `ContentReading/SitemapPropertyExclusionSelection.cs` | Excludes content based on configured property alias/value matching. |
| `SitemapRendering/PublishedContentRenderer.cs` | Collects content, applies inclusion predicate, renders URL entries, and builds the URL set. |
| `SitemapRendering/PublishedContentUrlRenderer.cs` | Renders individual `XmlSitemapUrl` entries from published content. |
| `SitemapRendering/PublishedContentUrlCultureLinkRenderer.cs` | Renders alternate culture `XHtmlLink` entries. |
| `SitemapRendering/PublishedContentUrlBuilder.cs` | Builds content URLs with optional host names and delegates generic sitemap URL building. |
| `SitemapRendering/PublishedContentIndexRenderer.cs` | Wraps the shared sitemap index renderer for published-content provider composition. |
| `SitemapRendering/PublishedContentCollector.cs` | Collects root content and descendants for sitemap rendering. |

## Public API

`AddXmlSitemapPublishedContentProvider()` is the main registration entry point. It registers:

- Shared sitemap provider renderers via `AddXmlSitemapProviders()`.
- Published-content reading and rendering services.
- `PublishedContentXmlSitemapProvider` as itself, `IXmlSitemapSourceProvider`, and `IXmlSitemapProvider`.

`PublishedContentXmlSitemapProvider` supports the `IXmlSitemapProvider` methods for root-key lookup, path lookup, configured sitemap lookup, and index lookup. Configured sitemap lookup first checks `XmlSitemapsOptions.Sitemaps`; if no content sitemap exists for the key, it checks `XmlSitemapsOptions.CustomSitemaps` and resolves a matching `IXmlSitemapCustomProvider.Alias`.

## Configuration

This project reads `XmlSitemapsOptions`, including:

- `Sitemaps`
- `CustomSitemaps`
- `Indexes`
- `IncludedCultures`
- `ExcludedCultures`
- `IncludedContentTypeAliases`
- `ExcludedContentTypeAliases`
- `RenderAlternateLinksForSingleCultureSitemaps`
- `RootNodeSearchLevel`
- `ExcludingUrlPropertyAlias`
- `ExcludingUrlPropertyValue`

`PublishedContentService` supports `RootNodeSearchLevel` values `0` and `1`. Other values throw an `InvalidOperationException` in the default implementation.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Providers.PublishedContent/Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

There is no dedicated test project for this library. Existing tests cover it through `../Casko.XmlSitemapsForUmbraco.Tests`, especially `PublishedContentServiceTests`, `SitemapRenderingServiceTests`, `SitemapCultureSelectionTests`, `SitemapContentTypeSelectionTests`, `SitemapPropertyExclusionSelectionTests`, `DefaultXmlSiteMapServiceRootKeyTests`, and `DefaultXmlSiteMapServiceCustomProviderTests`.

## Agent guidance

When modifying this project:

1. Treat `PublishedContentXmlSitemapProvider`, `IPublishedContentService`, and renderer interfaces as cross-project contracts.
2. Inspect tests before changing configuration precedence, culture filtering, content type filtering, property exclusion, host-name resolution, or custom sitemap behavior.
3. Keep shared provider abstractions in `../Casko.XmlSitemapsForUmbraco.Providers`; keep Umbraco published-content implementation here.
4. Keep DI registration in `Configuration/ServiceCollectionExtensions.cs` aligned with any new reader or renderer service.
5. Update this README when direct project references, public registration methods, configuration behavior, or major rendering rules change.

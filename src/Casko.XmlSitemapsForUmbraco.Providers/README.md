---
project: Casko.XmlSitemapsForUmbraco.Providers
type: library
language: C#
framework: net10.0
solution_role: Provider abstractions and shared sitemap rendering helpers
depends_on:
  - Casko.XmlSitemapsForUmbraco.Models
used_by:
  - Casko.XmlSitemapsForUmbraco.Delivery
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Providers.Examine
  - Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
  - Casko.XmlSitemapsForUmbraco.Storage
  - Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
  - Casko.XmlSitemapsForUmbraco.TestSite
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Providers

## Purpose

This project defines the provider contracts and shared rendering helpers used to produce sitemap models. It sits between the model contracts in `Casko.XmlSitemapsForUmbraco.Models` and concrete provider implementations such as published-content, Examine, storage, delivery, and package composition projects.

The project owns the common abstractions for public sitemap access, live source providers, custom sitemap providers, sitemap index rendering, URL set rendering, and sitemap URL building.

## Responsibilities

- Define the public sitemap provider contract through `IXmlSitemapProvider`.
- Define `IXmlSitemapSourceProvider` for live sitemap providers before storage or caching decorators are applied.
- Define custom sitemap extension points through `IXmlSitemapCustomProvider` and `XmlSitemapCustomProviderContext`.
- Provide shared sitemap rendering services: `IXmlSitemapIndexRenderer`, `IXmlSitemapUrlSetRenderer`, and `IXmlSitemapUrlBuilder`.
- Provide render context records for sitemap indexes, URL sets, and URL rendering.
- Centralize delivery API route constants used by URL builders and consumers in `XmlSitemapApiConstants`.
- Register shared provider services and custom providers through `Configuration/ServiceCollectionExtensions.cs`.

## Non-responsibilities

- This project does not query Umbraco content or Examine indexes. Concrete source behavior belongs in `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent` and `../Casko.XmlSitemapsForUmbraco.Providers.Examine`.
- This project does not serialize XML; serialization belongs in `../Casko.XmlSitemapsForUmbraco.Serialization`.
- This project does not cache, store, or refresh sitemap output; storage behavior belongs in `../Casko.XmlSitemapsForUmbraco.Storage` and `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia`.
- This project does not expose HTTP endpoints; delivery and backoffice API behavior belongs in `../Casko.XmlSitemapsForUmbraco.Delivery` and `../Casko.XmlSitemapsForUmbraco.Package`.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Models
       |
       v
Casko.XmlSitemapsForUmbraco.Providers
       ^
       |
       +-- Casko.XmlSitemapsForUmbraco.Delivery
       +-- Casko.XmlSitemapsForUmbraco.Package
       +-- Casko.XmlSitemapsForUmbraco.Providers.Examine
       +-- Casko.XmlSitemapsForUmbraco.Providers.PublishedContent
       +-- Casko.XmlSitemapsForUmbraco.Storage
       +-- Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
       +-- Casko.XmlSitemapsForUmbraco.TestSite
       +-- Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project | Reason |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Models/Casko.XmlSitemapsForUmbraco.Models.csproj` | Supplies `IXmlSitemapModel`, `XmlSitemap`, `XmlSitemapIndex`, `XmlSitemapUrl`, and related sitemap model types returned by provider and renderer contracts. |

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Delivery/Casko.XmlSitemapsForUmbraco.Delivery.csproj` | Uses provider contracts to serve sitemap models through delivery API routes. |
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Includes provider contracts and shared renderers in package composition and NuGet output. |
| `../Casko.XmlSitemapsForUmbraco.Providers.Examine/Casko.XmlSitemapsForUmbraco.Providers.Examine.csproj` | Builds an Examine-backed provider using shared contracts and renderers. |
| `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent/Casko.XmlSitemapsForUmbraco.Providers.PublishedContent.csproj` | Builds a published-content provider using shared contracts and renderers. |
| `../Casko.XmlSitemapsForUmbraco.Storage/Casko.XmlSitemapsForUmbraco.Storage.csproj` | Wraps provider output with stored sitemap retrieval and refresh behavior. |
| `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia/Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.csproj` | Registers storage behavior and shared provider services for Umbraco media-backed sitemap storage. |
| `../Casko.XmlSitemapsForUmbraco.TestSite/Casko.XmlSitemapsForUmbraco.TestSite.csproj` | Registers a dummy custom sitemap provider against `IXmlSitemapCustomProvider`. |
| `../Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj` | Tests provider contracts, shared renderers, service composition, and concrete consumers. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Providers.csproj` | Defines the `net10.0` library, direct model dependency, and dependency injection abstractions package. |
| `IXmlSitemapProvider.cs` | Main public provider contract for root-key, path, configured sitemap, and sitemap index access. |
| `IXmlSitemapSourceProvider.cs` | Marker interface for live providers before storage or caching wrappers are applied. |
| `IXmlSitemapCustomProvider.cs` | Custom sitemap provider contract implemented by user or test-site providers. |
| `XmlSitemapCustomProviderContext.cs` | Context passed to custom providers, including key, host name, and custom settings. |
| `XmlSitemapApiConstants.cs` | Delivery API route, name, title, version, and rewrite-key constants. |
| `Configuration/ServiceCollectionExtensions.cs` | Registers shared renderers and custom sitemap providers with `IServiceCollection`. |
| `SitemapRendering/Contexts` | Render context records for sitemap URL sets and sitemap indexes. |
| `SitemapRendering/Indexes/XmlSitemapIndexRenderer.cs` | Builds `XmlSitemapIndex` models from configured sitemap aliases and location mode. |
| `SitemapRendering/UrlSets/XmlSitemapUrlSetRenderer.cs` | Builds `XmlSitemap` models from valid, distinct URL entries. |
| `SitemapRendering/Urls/XmlSitemapUrlBuilder.cs` | Builds delivery API URLs, legacy `.xml` URLs, and host-prefixed relative URLs. |

## Public API

`IXmlSitemapProvider` is the broad sitemap access contract. It exposes sync and async methods for:

- `GetByRootKey` / `GetByRootKeyAsync`
- `GetByPath` / `GetByPathAsync`
- `GetConfigured` / `GetConfiguredAsync`
- `GetIndex` / `GetIndexAsync`

`IXmlSitemapCustomProvider` is the extension point for custom sitemap sources. Implementations provide an `Alias` that configuration can refer to, and return an `XmlSitemap` from `GetSitemapAsync(XmlSitemapCustomProviderContext, CancellationToken)`.

`AddXmlSitemapProviders()` registers shared scoped renderer services. `AddXmlSitemapCustomProvider<TProvider>()` registers custom provider implementations as scoped `IXmlSitemapCustomProvider` instances.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

There is no dedicated test project for this library. Existing tests cover provider behavior through `../Casko.XmlSitemapsForUmbraco.Tests`, especially service composition, sitemap rendering, custom provider, Examine provider, and published-content provider tests.

## Agent guidance

When modifying this project:

1. Treat provider interfaces and render context records as cross-project contracts.
2. Inspect direct consumers before changing method signatures, route constants, custom provider context fields, or renderer output rules.
3. Keep Umbraco-specific content querying out of this project; add concrete behavior in provider implementation projects.
4. Keep dependency injection helpers aligned with the concrete renderer services they register.
5. Update tests and this README when public provider contracts, route constants, direct project references, or renderer behavior change.

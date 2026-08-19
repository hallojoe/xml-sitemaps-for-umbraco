---
project: Casko.XmlSitemapsForUmbraco.Providers
type: library
language: C#
framework: net10.0
solution_role: Provider contracts and shared sitemap rendering services
depends_on:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Models
used_by:
  - Casko.XmlSitemapsForUmbraco.Delivery
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Providers.Examine
  - Casko.XmlSitemapsForUmbraco.Storage
  - Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Providers

## Purpose

Defines the public sitemap-provider contracts and shared rendering services. It sits between the Common/Models contracts and concrete providers, such as the Examine implementation, without querying content or storing XML itself.

## Responsibilities

- Define `IXmlSitemapProvider` for public root-key, path, configured-sitemap, and index access.
- Define `IXmlSitemapSourceProvider` for live providers before storage wraps public access.
- Define the custom-sitemap extension point through `IXmlSitemapCustomProvider` and `XmlSitemapCustomProviderContext`.
- Provide URL, URL-set, and sitemap-index rendering contracts and implementations.
- Provide render contexts, culture selection, and host-URL contracts shared by provider implementations.
- Register shared services and custom providers through the `IServiceCollection` extensions.

## Registration

```csharp
services.AddXmlSitemapsProviders();
services.AddXmlSitemapsCustomProvider<MyCustomSitemapProvider>();
```

`AddXmlSitemapsProviders` registers these scoped services:

| Service | Implementation |
| --- | --- |
| `IHostUrlProvider` | `HostUrlProvider` |
| `IXmlSitemapUrlBuilder` | `XmlSitemapUrlBuilder` |
| `IXmlSitemapUrlSetRenderer` | `XmlSitemapUrlSetRenderer` |
| `IXmlSitemapIndexRenderer` | `XmlSitemapIndexRenderer` |

`AddXmlSitemapsCustomProvider<TProvider>` adds the supplied provider as a scoped `IXmlSitemapCustomProvider`.

## Boundaries

- Content querying and Examine index access belong to `Providers.Examine`.
- XML serialization, API constants, and configuration types belong to Common.
- Stored-document retrieval and refresh orchestration belong to Storage.
- HTTP endpoints belong to Delivery and Package.

## Important files

| File or directory | Responsibility |
| --- | --- |
| `IXmlSitemapProvider.cs` | Public sitemap access contract. |
| `IXmlSitemapSourceProvider.cs` | Live-source provider contract. |
| `IXmlSitemapCustomProvider.cs` | Custom sitemap extension point. |
| `Configuration/ServiceCollectionExtensions.cs` | Shared renderer and custom-provider registration. |
| `Rendering/Contexts` | Sitemap, URL, index, and culture-selection contexts. |
| `Rendering/Indexes/XmlSitemapIndexRenderer.cs` | Builds index models and their sitemap locations. |
| `Rendering/UrlSets/XmlSitemapUrlSetRenderer.cs` | Builds sitemap models from valid, distinct URL entries. |
| `Rendering/Urls/XmlSitemapUrlBuilder.cs` | Builds delivery API, legacy `.xml`, and host-prefixed URLs. |
| `Routing/HostUrlProvider.cs` | Resolves configured Umbraco host URLs for provider implementations. |

## Public contracts

`IXmlSitemapProvider` exposes synchronous and asynchronous methods for `GetByRootKey`, `GetByPath`, `GetConfigured`, and `GetIndex`.

`IXmlSitemapCustomProvider` implementations expose an `Alias` and create an `XmlSitemap` from `XmlSitemapCustomProviderContext`, which contains the sitemap key, configured host name, and custom settings.

The shared URL builder uses the delivery API constants from Common to construct current API URLs and preserves support for legacy `.xml` locations.

## Validation

```powershell
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj --filter "FullyQualifiedName~XmlSitemapUrl|FullyQualifiedName~XmlSitemapIndex|FullyQualifiedName~ServiceComposition"
```

## Development notes

- Treat provider interfaces and rendering contexts as cross-project contracts.
- Keep this project independent of concrete Umbraco content querying.
- Update direct consumers and tests whenever public signatures or rendering rules change.

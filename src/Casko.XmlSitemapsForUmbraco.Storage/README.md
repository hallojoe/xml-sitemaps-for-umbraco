---
project: Casko.XmlSitemapsForUmbraco.Storage
type: library
language: C#
framework: net10.0
solution_role: Stored sitemap abstractions, refresh service, and cached provider wrapper
depends_on:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Models
  - Casko.XmlSitemapsForUmbraco.Providers
used_by:
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Storage

## Purpose

This project defines the storage layer contracts and services for generated sitemap XML. It sits between live sitemap providers and concrete backing stores, letting configured sitemap XML be read from storage, refreshed when missing or stale, and written back as raw XML documents.

The project does not choose a physical storage medium. `Storage.UmbracoMedia` supplies the Umbraco media-backed implementation of the `IXmlSitemapDataSource` abstraction.

## Responsibilities

- Define stored document identity through `XmlSitemapStorageKey` and `XmlSitemapDocumentKind`.
- Represent stored raw XML and optional media metadata through `XmlSitemapStoredDocument`.
- Define `IXmlSitemapDataSource` for reading and writing stored XML documents.
- Define stable storage file-name generation through `IXmlSitemapStorageNameProvider` and `XmlSitemapStorageNameProvider`.
- Define refresh operations through `IXmlSitemapStorageRefreshService`.
- Implement `XmlSitemapStorageRefreshService`, which rebuilds configured sitemaps, custom sitemaps, and sitemap indexes from `IXmlSitemapSourceProvider`, serializes them, and writes them to `IXmlSitemapDataSource`.
- Implement `StoredXmlSitemapProvider`, which exposes `IXmlSitemapProvider` while serving configured sitemap and index requests from storage when possible.

## Non-responsibilities

- This project does not implement the backing store; Umbraco media storage belongs in `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia`.
- This project does not generate live sitemap models; live source behavior belongs in provider projects such as `../Casko.XmlSitemapsForUmbraco.Providers.Examine` and `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent`.
- This project does not define sitemap model serialization rules; serialization belongs in `../Casko.XmlSitemapsForUmbraco.Serialization`.
- This project does not expose HTTP endpoints or schedule background jobs.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Common
Casko.XmlSitemapsForUmbraco.Models
Casko.XmlSitemapsForUmbraco.Providers
       |
       v
Casko.XmlSitemapsForUmbraco.Storage
       ^
       |
       +-- Casko.XmlSitemapsForUmbraco.Package
       +-- Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
       +-- Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project | Reason                                                                                                                 |
|---|------------------------------------------------------------------------------------------------------------------------|
| `../Casko.XmlSitemapsForUmbraco.Common/Casko.XmlSitemapsForUmbraco.Common.csproj` | Supplies `XmlSitemapsOptions` and storage configuration values such as `RefreshStaleAfterSeconds`. Supplies XML serializer/deserializer services for storing and reading raw XML documents.                    |
| `../Casko.XmlSitemapsForUmbraco.Models/Casko.XmlSitemapsForUmbraco.Models.csproj` | Supplies `IXmlSitemapModel`, `XmlSitemap`, and `XmlSitemapIndex` types refreshed and returned by storage services.     |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj` | Supplies `IXmlSitemapProvider` and `IXmlSitemapSourceProvider` contracts used by stored provider and refresh services. |

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Includes storage services in the packaged output. |
| `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia/Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.csproj` | Provides the concrete Umbraco media data source, registers storage services, and schedules refresh work. |
| `../Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj` | Tests stored provider behavior, refresh behavior, data-source defaults, and file-name normalization. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Storage.csproj` | Defines the `net10.0` storage library and direct project references. |
| `IXmlSitemapDataSource.cs` | Backing-store abstraction for reading, writing, and checking stored XML sitemap documents. |
| `IXmlSitemapStorageRefreshService.cs` | Refresh service contract for all configured documents, individual configured sitemaps, custom sitemaps, and indexes. |
| `XmlSitemapStorageKey.cs` | Storage identity record containing document kind, alias, and host name. |
| `XmlSitemapDocumentKind.cs` | Distinguishes sitemap documents from sitemap index documents. |
| `XmlSitemapStoredDocument.cs` | Raw XML document record returned by storage data sources. |
| `XmlSitemapStorageNameProvider.cs` | Builds normalized file names such as `sitemap--default--products.xml` and `sitemap-index--example-com--main.xml`. |
| `Services/StoredXmlSitemapProvider.cs` | Public provider wrapper that serves configured sitemaps and indexes from storage when available and fresh. |
| `Services/XmlSitemapStorageRefreshService.cs` | Refresh implementation that calls the live source provider, serializes generated models, and writes stored XML. |

## Public API

`IXmlSitemapDataSource` is the storage adapter contract. Implementations must provide `ReadAsync()` and `WriteAsync()`; the default interface method `ExistsAsync()` checks existence by calling `ReadAsync()`.

`StoredXmlSitemapProvider` delegates root-key and path requests directly to `IXmlSitemapSourceProvider`. For configured sitemap, custom sitemap, and index requests, it reads storage first. If the stored document exists and is not stale, it deserializes and returns it. If the document is missing or stale, it calls `IXmlSitemapStorageRefreshService`.

`XmlSitemapStorageRefreshService.RefreshAllAsync()` refreshes configured sitemaps first, then configured custom sitemaps, then configured sitemap indexes.

## Configuration

This project reads `XmlSitemapsOptions.Storage.RefreshStaleAfterSeconds`.

If `RefreshStaleAfterSeconds` is `0` or less, stored documents are never treated as stale by `StoredXmlSitemapProvider`. If the value is positive, documents with no `RefreshedUtc` are stale, and documents older than the configured number of seconds are refreshed.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Storage/Casko.XmlSitemapsForUmbraco.Storage.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

There is no dedicated test project for this library. Existing tests cover it through `../Casko.XmlSitemapsForUmbraco.Tests`, especially `StoredXmlSitemapProviderTests`, `XmlSitemapStorageRefreshServiceTests`, `XmlSitemapStorageNameProviderTests`, and `XmlSitemapDataSourceTests`.

## Agent guidance

When modifying this project:

1. Treat `IXmlSitemapDataSource`, `IXmlSitemapStorageRefreshService`, `XmlSitemapStorageKey`, and `XmlSitemapStoredDocument` as cross-project contracts.
2. Inspect `../Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia` before changing data-source semantics, stored document fields, or storage file-name behavior.
3. Keep physical storage details out of this project; add backing-store behavior in concrete storage projects.
4. Keep refresh behavior aligned with `XmlSitemapsOptions` and serializer/deserializer expectations.
5. Update tests and this README when staleness rules, refresh order, storage keys, file-name normalization, or direct project references change.

---
project: Casko.XmlSitemapsForUmbraco.Storage
type: library
language: C#
framework: net10.0
solution_role: Sitemap storage contracts, stored provider, and refresh service
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

Defines the storage-layer contracts and services for generated sitemap XML. It separates sitemap generation from the physical backing store: the storage project names documents, serializes refreshed models, and serves previously stored XML; `Storage.UmbracoMedia` supplies the concrete Media-backed data source.

Stored delivery is read-only. A missing stored sitemap or index is represented by an empty model; serving a request does not trigger a refresh.

## Responsibilities

- Define storage identity through `XmlSitemapStorageKey` and `XmlSitemapDocumentKind`.
- Define `IXmlSitemapDataSource` and `XmlSitemapStoredDocument` for raw XML persistence.
- Provide `XmlSitemapStorageNameProvider` for stable file names based on document kind, host, and alias.
- Provide `XmlSitemapStorageRefreshService`, which generates configured, custom, and index models from `IXmlSitemapSourceProvider`, serializes them, and writes them to the data source.
- Provide `StoredXmlSitemapProvider`, the public provider wrapper that serves configured sitemap and index documents from storage while delegating root-key and path requests to the live source provider.
- Register generic storage services through `AddXmlSitemapsStorage` when `XmlSitemaps:Storage` is configured.

## Registration

```csharp
services.AddXmlSitemapsStorage(configuration);
```

The extension does nothing unless `XmlSitemaps:Storage` exists. Otherwise it configures `XmlSitemapStorageOptions`, registers shared provider and serialization services, adds `TimeProvider.System` only when no `TimeProvider` is registered, and registers these scoped services:

| Service | Implementation |
| --- | --- |
| `IXmlSitemapStorageNameProvider` | `XmlSitemapStorageNameProvider` |
| `IXmlSitemapStorageRefreshService` | `XmlSitemapStorageRefreshService` |
| `IXmlSitemapProvider` | `StoredXmlSitemapProvider` |

An application must also register an `IXmlSitemapSourceProvider` and a concrete `IXmlSitemapDataSource`. The package does this through the Examine and Umbraco Media projects.

## Refresh behavior

`RefreshAllAsync` refreshes the implicit default sitemap when running in single-sitemap mode without explicit sitemap, custom-sitemap, or index entries. Otherwise it refreshes configured sitemaps, then custom sitemaps, then indexes.

If a configured sitemap root cannot be resolved, that sitemap is skipped and any existing stored document is retained. Invalid keys and other failures are surfaced to the caller.

## Storage names and configuration

File names follow this form:

```text
sitemap--{normalized-host}--{normalized-alias}.xml
sitemap-index--{normalized-host}--{normalized-alias}.xml
```

An absent or unparseable host becomes `default`; host paths and ports do not form part of the file name. Segments are lower-cased and normalized to hyphen-separated alphanumeric values.

`XmlSitemaps:Storage` is also the configuration section used by concrete storage implementations. `XmlSitemapStorageOptions` currently provides `VersionCleanupAfterSeconds` (default `600`); its value is consumed by the Media-backed implementation.

## Important files

| File | Responsibility |
| --- | --- |
| `Configuration/ServiceCollectionExtensions.cs` | Generic storage service registration. |
| `IXmlSitemapDataSource.cs` | Raw XML backing-store abstraction. |
| `IXmlSitemapStorageRefreshService.cs` | Refresh-service contract. |
| `XmlSitemapStorageKey.cs` | Validated storage key. |
| `XmlSitemapStorageNameProvider.cs` | Stable storage-file naming. |
| `Services/StoredXmlSitemapProvider.cs` | Storage-backed public provider. |
| `Services/XmlSitemapStorageRefreshService.cs` | Refresh, serialization, and write orchestration. |

## Validation

```powershell
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj --filter "FullyQualifiedName~StoredXmlSitemapProvider|FullyQualifiedName~XmlSitemapStorageRefreshService|FullyQualifiedName~XmlSitemapStorageNameProvider"
```

## Development notes

- Treat `IXmlSitemapDataSource`, storage keys, and stored-document fields as cross-project contracts.
- Keep physical storage behavior in a concrete storage project.
- Preserve the read-only behavior of `StoredXmlSitemapProvider`; refresh is explicit or scheduled by the host application.

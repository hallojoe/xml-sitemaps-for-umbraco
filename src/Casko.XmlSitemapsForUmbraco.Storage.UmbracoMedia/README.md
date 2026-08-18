---
project: Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
type: library
language: C#
framework: net10.0
solution_role: Umbraco media-backed sitemap storage and refresh job integration
depends_on:
  - Casko.XmlSitemapsForUmbraco.Providers
  - Casko.XmlSitemapsForUmbraco.Common.Serialization
  - Casko.XmlSitemapsForUmbraco.Storage
used_by:
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia

## Purpose

This project provides the concrete Umbraco media-backed storage implementation for generated XML sitemap documents. It stores sitemap XML files under an Umbraco media root folder, updates existing media files when possible, and registers a recurring background job that refreshes stored sitemap output.

It also wires the storage layer into the public provider pipeline by registering `StoredXmlSitemapProvider` as the default `IXmlSitemapProvider`.

## Responsibilities

- Implement `IXmlSitemapDataSource` with `UmbracoMediaXmlSitemapDataSource`.
- Create or reuse the root media folder named `Xml Sitemaps`.
- Create, update, read, and return stored sitemap media file metadata.
- Isolate Umbraco media file operations behind `IUmbracoMediaFileAccessor`.
- Register storage, serialization, shared provider services, and `TimeProvider` through `AddXmlSitemapsUmbracoMediaStorage()`.
- Register `IXmlSitemapProvider` as `StoredXmlSitemapProvider` so configured sitemaps can be served from storage before falling back to refresh.
- Register `UmbracoMediaXmlSitemapRefreshBackgroundJob` as a recurring Umbraco background job.

## Non-responsibilities

- This project does not define storage contracts or refresh behavior; those belong in `../Casko.XmlSitemapsForUmbraco.Storage`.
- This project does not generate live sitemap models; live providers belong in `../Casko.XmlSitemapsForUmbraco.Providers.Examine` and `../Casko.XmlSitemapsForUmbraco.Providers.PublishedContent`.
- This project does not define XML serialization rules; serialization belongs in `../Casko.XmlSitemapsForUmbraco.Common.Serialization`.
- This project does not expose delivery or backoffice HTTP endpoints.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Providers
Casko.XmlSitemapsForUmbraco.Common
Casko.XmlSitemapsForUmbraco.Storage
       |
       v
Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
       ^
       |
       +-- Casko.XmlSitemapsForUmbraco.Package
       +-- Casko.XmlSitemapsForUmbraco.Tests
```

### Dependencies

| Project                                                                                  | Reason |
|------------------------------------------------------------------------------------------|---|
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj`  | Supplies shared provider registrations and `IXmlSitemapProvider` contracts used by storage composition. |
| `../Casko.XmlSitemapsForUmbraco.Common.Serialization/Casko.XmlSitemapsForUmbraco.Common.csproj` | Supplies XML serializer/deserializer registration used by stored sitemap provider and refresh services. |
| `../Casko.XmlSitemapsForUmbraco.Storage/Casko.XmlSitemapsForUmbraco.Storage.csproj`      | Supplies storage contracts, name provider, refresh service, and stored provider wrapper. |

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Registers Umbraco media storage as the packaged storage implementation. |
| `../Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj` | Tests media data source behavior, background job behavior, and service composition. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.csproj` | Defines the `net10.0` library, Umbraco package references, and direct project references. |
| `Configuration/ServiceCollectionExtensions.cs` | Main registration entry point for media storage, serialization, refresh services, stored provider, and background job. |
| `UmbracoMediaXmlSitemapDataSource.cs` | Concrete `IXmlSitemapDataSource` implementation backed by Umbraco media files. |
| `IUmbracoMediaFileAccessor.cs` | Abstraction over Umbraco media file reads and writes. |
| `UmbracoMediaFileAccessor.cs` | Umbraco implementation for reading and updating the media file property and file content stream. |
| `UmbracoMediaXmlSitemapRefreshBackgroundJob.cs` | Recurring background job that calls `IXmlSitemapStorageRefreshService.RefreshAllAsync()`. |

## Public API

`AddXmlSitemapsUmbracoMediaStorage()` is the project’s main public entry point. It registers:

- Shared sitemap provider services via `AddXmlSitemapProviders()`.
- XML serialization services via `AddXmlSitemapsSerialization()`.
- `IXmlSitemapStorageNameProvider` as `XmlSitemapStorageNameProvider`.
- `IUmbracoMediaFileAccessor` as `UmbracoMediaFileAccessor`.
- `IXmlSitemapDataSource` as `UmbracoMediaXmlSitemapDataSource`.
- `IXmlSitemapStorageRefreshService` as `XmlSitemapStorageRefreshService`.
- `IXmlSitemapProvider` as `StoredXmlSitemapProvider`.
- `UmbracoMediaXmlSitemapRefreshBackgroundJob` as a recurring background job.

## Storage Behavior

`UmbracoMediaXmlSitemapDataSource` stores files below the root media folder `Xml Sitemaps`.

On write, it:

1. Validates the `XmlSitemapStorageKey`.
2. Creates the root folder when it does not exist.
3. Looks for an existing child media item with the generated storage file name.
4. Updates existing file content when the media item has a file path.
5. Otherwise creates a new Umbraco file media item and sets its initial file.

On read, it finds the generated file name under the root folder, opens the media file stream, reads XML as UTF-8, and returns an `XmlSitemapStoredDocument` with media key, media id, file name, media path, XML, and refreshed timestamp.

## Configuration

The background job reads:

- `XmlSitemaps:Storage:BackgroundJob:Enabled`, default `true`.
- `XmlSitemaps:Storage:BackgroundJob:IntervalSeconds`, default `3600`.
- `XmlSitemaps:Storage:BackgroundJob:RefreshJobDelayInSeconds`, default `10`.

`UmbracoMediaXmlSitemapRefreshBackgroundJob` enforces a minimum delay of 10 seconds. If `IntervalSeconds` is `0` or less, it falls back to 3600 seconds.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia/Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

There is no dedicated test project for this library. Existing tests cover it through `../Casko.XmlSitemapsForUmbraco.Tests`, especially `UmbracoMediaXmlSitemapDataSourceTests`, `UmbracoMediaXmlSitemapRefreshBackgroundJobTests`, and `ServiceCompositionTests`.

## Agent guidance

When modifying this project:

1. Keep `AddXmlSitemapsUmbracoMediaStorage()` aligned with constructor dependencies in the storage, serialization, and provider services it registers.
2. Inspect `../Casko.XmlSitemapsForUmbraco.Storage` before changing data-source semantics or stored document fields.
3. Preserve the root media folder name and file-name matching behavior unless migration or compatibility work is included.
4. Update tests when changing background job timing, enablement rules, media read/write behavior, or provider registration order.
5. Update this README when direct project references, storage behavior, background job configuration, or DI ownership changes.

---
project: Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia
type: library
language: C#
framework: net10.0
solution_role: Umbraco Media-backed XML sitemap storage
depends_on:
  - Casko.XmlSitemapsForUmbraco.Providers
  - Casko.XmlSitemapsForUmbraco.Storage
used_by:
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Storage.UmbracoMedia

## Purpose

Provides an `IXmlSitemapDataSource` that stores generated sitemap XML as Umbraco Media items. It also registers an optional recurring background job that refreshes all stored sitemaps.

The generic storage contracts, refresh service, stored sitemap provider, serialization, and HTTP result helpers live in the Storage and Common projects. This project supplies the Umbraco Media implementation.

## Responsibilities

- Read and write sitemap XML through Umbraco Media.
- Create and use a dedicated **Xml Sitemaps** media folder.
- Create immutable, versioned media files for writes while retaining recent versions for safe reads.
- Cache the latest media version for one minute and fall back safely if cached media has been removed.
- Remove old versioned files after the configured cleanup period while retaining the two newest versions.
- Register and run the optional `UmbracoMediaXmlSitemapRefreshBackgroundJob`.

## Non-responsibilities

- Generating sitemap URLs or traversing published content; that belongs to the Providers projects.
- Defining generic sitemap storage contracts, names, refresh orchestration, or stored-provider behavior; that belongs to the Storage project.
- XML serialization and HTTP response helpers; those belong to Common.

## Registration

Register the Media-backed storage implementation during application startup:

```csharp
builder.Services.AddXmlSitemapsUmbracoMediaStorage(builder.Configuration);
```

The extension does nothing unless the `XmlSitemaps:Storage` configuration section exists. When it does, it first calls `AddXmlSitemapsStorage`, which registers the generic storage services, including the stored sitemap provider, refresh service, storage-name provider, XML serialization, and a `TimeProvider` when one has not already been registered.

It then registers these Umbraco-specific services:

| Service | Implementation |
| --- | --- |
| `IUmbracoMediaFileAccessor` | `UmbracoMediaFileAccessor` |
| `IXmlSitemapDataSource` | `UmbracoMediaXmlSitemapDataSource` |

The refresh background job is registered only when the `XmlSitemaps:Storage:BackgroundJob` section exists.

## Storage behavior

Each sitemap is stored as a media item beneath the **Xml Sitemaps** folder. A write creates a new versioned file name containing a UTC timestamp and unique suffix, rather than replacing the active file in place. This lets reads continue using a valid earlier version while a new version is being created.

Reads resolve the newest versioned file for the sitemap key. Legacy, unversioned file names are still supported for backwards compatibility. The newest resolved version is cached for one minute; if that media item no longer exists, the cache is cleared and the lookup is retried.

Version cleanup runs after a successful write. It retains the two newest versioned files and removes older versions only after `VersionCleanupAfterSeconds` has elapsed. Set that value to `0` or a negative value to disable cleanup.

## Configuration

```json
{
  "XmlSitemaps": {
    "Storage": {
      "VersionCleanupAfterSeconds": 600,
      "BackgroundJob": {
        "IntervalSeconds": 3600,
        "RefreshJobDelayInSeconds": 10
      }
    }
  }
}
```

| Setting | Default | Description |
| --- | ---: | --- |
| `XmlSitemaps:Storage:VersionCleanupAfterSeconds` | `600` | Age an old version must reach before it may be deleted. `<= 0` disables version cleanup. |
| `XmlSitemaps:Storage:BackgroundJob:IntervalSeconds` | `3600` | Refresh interval in seconds. A non-positive configured value falls back to `3600`. |
| `XmlSitemaps:Storage:BackgroundJob:RefreshJobDelayInSeconds` | `10` | Initial delay in seconds before refreshing. Values below `10` are raised to `10`. |

Omit the `BackgroundJob` section entirely to avoid registering the scheduled refresh job. If it is present, the job logs its UTC start and completion times plus a human-readable elapsed duration at Information level.

## Important files

| File | Responsibility |
| --- | --- |
| `Configuration/ServiceCollectionExtensions.cs` | Registers Media storage and the optional job. |
| `Storage/UmbracoMediaXmlSitemapDataSource.cs` | Versioned Media read/write, caching, and cleanup. |
| `Storage/UmbracoMediaFileAccessor.cs` | Reads and writes the Umbraco Media file property. |
| `UmbracoMediaXmlSitemapRefreshBackgroundJob.cs` | Scheduled refresh-job configuration and execution. |

## Validation

Run the focused test suite when changing this project:

```powershell
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj --filter "FullyQualifiedName~UmbracoMedia"
```

## Development notes

- Preserve versioned-write behavior; do not replace the active media file in place.
- Keep cleanup conservative: retain two latest versions and never remove versions younger than the configured cleanup age.
- Keep the background job opt-in through the presence of its configuration section.
- Use `IUmbracoMediaFileAccessor` rather than accessing Media file properties directly from the data source.

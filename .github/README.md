# XML Sitemaps for Umbraco

[![Downloads](https://img.shields.io/nuget/dt/Casko.XmlSitemapsForUmbraco?color=cc9900)](https://www.nuget.org/packages/Casko.XmlSitemapsForUmbraco/)
[![NuGet](https://img.shields.io/nuget/vpre/Casko.XmlSitemapsForUmbraco?color=0273B3)](https://www.nuget.org/packages/Casko.XmlSitemapsForUmbraco)
[![GitHub license](https://img.shields.io/github/license/hallojoe/xml-sitemaps-for-umbraco?color=8AB803)](https://github.com/hallojoe/xml-sitemaps-for-umbraco/blob/main/LICENSE)

XML Sitemaps for Umbraco adds configurable XML sitemap and sitemap index delivery to Umbraco 17+ projects on .NET 10. It can render sitemaps from Umbraco content, expose friendly rewrite URLs such as `/xmlsitemap.xml`, store generated XML in Umbraco media, refresh stored files in the background, and let projects plug in custom sitemap providers for data that does not come from the content tree.

The public NuGet package is `Casko.XmlSitemapsForUmbraco`. Internally the implementation is split into focused assemblies for common configuration and serialization, models, shared providers, Examine-backed URL discovery, storage, Umbraco media storage, delivery, rewriting, and the package/backoffice UI.

Most sitemap behavior is registered behind interfaces, so projects can replace package services through dependency injection when the default implementation is not enough. This includes rendering, URL building, content collection, XML serialization, storage, and custom sitemap providers.

## Installation

Install the NuGet package:

```powershell
dotnet add package Casko.XmlSitemapsForUmbraco
```

The package composer registers HybridCache, sitemap configuration, the Examine source provider, Umbraco media storage, the delivery API, rewrite middleware, and the read-only backoffice configuration workspace.

## Quick Start

Add an `XmlSitemaps` section to `appsettings.json`. This minimal configuration uses the default single-sitemap mode and mirrors the language-variant demo site's settings:

```json
{
  "XmlSitemaps": {
    "Enabled": true,
    "RewritesEnabled": true,
    "ExcludingUrlPropertyAlias": "umbracoNaviHide",
    "ExcludingUrlPropertyValue": "1",
    "UseDeliveryApiAccessPolicy": false,
    "IndexName": "ExternalIndex",
    "IncludedCultures": [ "da", "en" ],
    "ExcludedCultures": [],
    "RenderAlternateLinksForSingleCultureSitemaps": true
  }
}
```

With rewrites enabled, the single sitemap is available at:

- `/xmlsitemap.xml`.

The delivery API can also be called directly:

- `/api/xmlsitemaps?name=xmlsitemap` for the default single sitemap.

To use configuration mode, add `"Mode": "Configuration"` and configure `Sitemaps`, `CustomSitemaps`, and/or `Indexes` as shown below. Their direct delivery API routes are:

- `/api/xmlsitemaps/xmlsitemap?key={sitemapKey}` for configured and custom sitemaps.
- `/api/xmlsitemaps/xmlsitemapindex?key={indexKey}` for sitemap indexes.

## Configured Sitemaps

Configured sitemaps live under `XmlSitemaps:Sitemaps`. Each entry key is the internal unique ID used by the API, storage, and sitemap index configuration. Set `PublicName` when the public XML filename should differ from that internal key.

```json
{
  "XmlSitemaps": {
    "Sitemaps": {
      "products-en": {
        "PublicName": "products",
        "Path": "/products",
        "HostName": "https://www.example.com",
        "Culture": "en",
        "IncludedCultures": [ "en" ],
        "ExcludedCultures": [],
        "IncludedDocumentTypeAliases": [ "productPage" ],
        "ExcludedDocumentTypeAliases": []
      }
    }
  }
}
```

Important settings:

- `PublicName`: public XML file name without `.xml`. Defaults to the entry key.
- `Path`: content path to use as the sitemap root. Defaults to `/`.
- `HostName`: host used to resolve the root and render absolute URLs.
- `Culture`: primary culture used when rendering URLs.
- `IncludedCultures` and `ExcludedCultures`: per-sitemap culture filtering.
- `IncludedDocumentTypeAliases` and `ExcludedDocumentTypeAliases`: per-sitemap document type filtering.

Root-level culture and document type settings apply to all configured sitemaps unless a sitemap entry narrows them further.

Set `ExcludingUrlPropertyAlias` and `ExcludingUrlPropertyValue` at the root level to exclude content when a property contains a specific value. For example, setting `ExcludingUrlPropertyAlias` to `metaRobots` and `ExcludingUrlPropertyValue` to `noindex` excludes any content item whose `metaRobots` value contains `noindex`, ignoring casing. The filter is only active when both settings are configured.

Set `RootNodeSearchLevel` at the root level to control where routed site roots are resolved:

- `0`: treat Umbraco navigation roots as the routed site roots. This supports the common single-site and multi-site trees.
- `1`: treat Umbraco navigation roots as unrouted containers and resolve their direct children as the routed site roots.
- Values above `1` are not supported by the default content service and require a custom content service.

Set `UseDeliveryApiAccessPolicy` at the root level to opt in or out of the Delivery API access policy used by the package's delivery endpoints. It defaults to `true`.

## Sitemap Indexes

Configured indexes live under `XmlSitemaps:Indexes`. Each index lists sitemap keys to include:

```json
{
  "XmlSitemaps": {
    "Indexes": {
      "xmlsitemap": {
        "PublicName": "xmlsitemap",
        "HostName": "https://www.example.com",
        "Sitemaps": [ "products-en", "articles-en" ]
      }
    }
  }
}
```

Indexes may reference regular configured sitemaps and custom configured sitemaps by their internal keys. When an index is rendered as XML, each child sitemap location uses that sitemap's `PublicName` when configured.

Important settings:

- `PublicName`: public XML file name without `.xml`. Defaults to the entry key.
- `HostName`: host used when rendering absolute sitemap index URLs.
- `Sitemaps`: internal sitemap keys included in the index, covering both configured and custom sitemaps.

## Custom Sitemaps

Custom sitemaps are for XML sitemap entries that should be generated by project code instead of the Umbraco content tree. Configure them under `XmlSitemaps:CustomSitemaps`:

```json
{
  "XmlSitemaps": {
    "CustomSitemaps": {
      "external-products": {
        "PublicName": "products-feed",
        "ProviderAlias": "external-products-provider",
        "HostName": "https://www.example.com",
        "Settings": {
          "FeedId": "products"
        }
      }
    }
  }
}
```

Create and register a provider:

```csharp
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;

public sealed class ExternalProductsSitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "external-products-provider";

    public Task<XmlSitemap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        var sitemap = new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/products/example-product",
                    LastModified = DateTime.UtcNow
                }
            ]
        };

        return Task.FromResult(sitemap);
    }
}
```

Register the provider in an Umbraco composer or another startup path:

```csharp
using Casko.XmlSitemapsForUmbraco.Providers.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

public sealed class SitemapProviderComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddXmlSitemapCustomProvider<ExternalProductsSitemapProvider>();
    }
}
```

The provider context contains the configured sitemap key, host name, and flat string settings. If a custom sitemap references a provider alias that is not registered, the package throws a clear configuration error.

`Settings` is the provider-specific string dictionary for a custom sitemap. Each entry is passed through to the provider context unchanged so project code can read whatever named values it needs.

### Extension Models For Custom Sitemaps

The package exposes `image`, `video`, and `news` XML sitemap model types in `Casko.XmlSitemapsForUmbraco.Models`:

- `XmlSitemapImage`
- `XmlSitemapVideo`, `XmlSitemapVideoRestriction`, `XmlSitemapVideoPlatform`, and `XmlSitemapVideoUploader`
- `XmlSitemapNews` and `XmlSitemapNewsPublication`

These types are available for custom sitemap implementations, but the package's default Umbraco content-tree renderer does not populate `XmlSitemapUrl.Images`, `XmlSitemapUrl.Videos`, or `XmlSitemapUrl.News`.

If a project needs `image`, `video`, or `news` sitemap output, implement `IXmlSitemapCustomProvider` or replace the package rendering services with a custom implementation.

Minimal image sitemap example:

```csharp
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;

public sealed class ProductImagesSitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "product-images-provider";

    public Task<XmlSitemap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/products/example-product",
                    LastModified = new DateTime(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc),
                    Images =
                    [
                        new XmlSitemapImage
                        {
                            Location = "https://cdn.example.com/products/example-product/main-image.jpg"
                        }
                    ]
                }
            ]
        });
    }
}
```

Minimal video sitemap example:

```csharp
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;

public sealed class VideoLibrarySitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "video-library-provider";

    public Task<XmlSitemap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/videos/example-video",
                    LastModified = new DateTime(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc),
                    Videos =
                    [
                        new XmlSitemapVideo
                        {
                            ThumbnailLocation = "https://cdn.example.com/videos/example-video/thumbnail.jpg",
                            Title = "Example Product Walkthrough",
                            Description = "Short walkthrough of the example product.",
                            ContentLocation = "https://cdn.example.com/videos/example-video/video.mp4"
                        }
                    ]
                }
            ]
        });
    }
}
```

Minimal news sitemap example:

```csharp
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;

public sealed class NewsArticlesSitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "news-articles-provider";

    public Task<XmlSitemap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new XmlSitemap
        {
            Urls =
            [
                new XmlSitemapUrl
                {
                    Location = "https://www.example.com/news/example-story",
                    LastModified = new DateTime(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc),
                    News = new XmlSitemapNews
                    {
                        Publication = new XmlSitemapNewsPublication
                        {
                            Name = "Example News",
                            Language = "en"
                        },
                        PublicationDate = new DateTimeOffset(2026, 7, 2, 12, 0, 0, TimeSpan.Zero),
                        Title = "Example Story Headline"
                    }
                }
            ]
        });
    }
}
```

These examples are intentionally minimal. A single custom provider can combine regular URL metadata with `image`, `video`, and `news` sitemap metadata when a project needs multiple sitemap extensions in the same sitemap.

Important settings:

- `PublicName`: public XML file name without `.xml`. Defaults to the entry key.
- `ProviderAlias`: alias of the registered `IXmlSitemapCustomProvider` implementation to execute.
- `HostName`: host used when rendering absolute URLs for the custom sitemap.
- `Settings`: provider-specific string values passed through to the custom provider context.

## Public Names And Rewrites

`PublicName` is available on `Sitemaps`, `CustomSitemaps`, and `Indexes`. It controls the public XML filename used by rewrite URLs and sitemap index locations. Do not include `.xml`; the package appends it.

When `RewritesEnabled` is `true`, configured entries are exposed as XML files at the site root using `PublicName` when configured, otherwise the internal key.

If `PublicName` is omitted, the package uses the configuration key, preserving existing behavior. The configuration key must still be unique because it is the internal ID used by the API, storage, custom provider context, and index `Sitemaps` lists.

This is useful when multiple hostnames should publish the same public sitemap filename:

```json
{
  "XmlSitemaps": {
    "RewritesEnabled": true,
    "Sitemaps": {
      "host1-main": {
        "PublicName": "xmlsitemap",
        "Path": "/",
        "HostName": "https://host1.dk",
        "Culture": "en",
        "IncludedCultures": [ "en" ]
      },
      "host2-main": {
        "PublicName": "xmlsitemap",
        "Path": "/",
        "HostName": "https://host2.dk",
        "Culture": "da",
        "IncludedCultures": [ "da" ]
      }
    }
  }
}
```

This publishes:

- `https://host1.dk/xmlsitemap.xml` to internal key `host1-main`.
- `https://host2.dk/xmlsitemap.xml` to internal key `host2-main`.

Duplicate public names are allowed when different `HostName` values make them unambiguous. If two entries produce the same public path for the same host scope, the first definition wins. Sitemap indexes are registered before regular sitemaps, and regular sitemaps before custom sitemaps.

## Stored Media Files And Refresh

The package stores generated XML sitemap files in Umbraco Media as immutable versions. Delivery reads the latest stored file and returns an empty sitemap or index when none has been generated yet. Refresh is performed by the optional background job, not on the delivery request path.

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

Storage behavior:

- Stored sitemap files are created in an `Xml Sitemaps` media folder.
- Each write creates a versioned media file; the two newest versions are retained.
- The background job starts after `Storage:BackgroundJob:RefreshJobDelayInSeconds`.
- The job refreshes regular sitemaps, custom sitemaps, then sitemap indexes.
- The background job is registered only when the `Storage:BackgroundJob` section exists.

Important settings:

- `VersionCleanupAfterSeconds`: age an older version must reach before it can be deleted. Defaults to `600`; `0` or less disables cleanup.
- `BackgroundJob.IntervalSeconds`: number of seconds between background refresh runs. Defaults to `3600`.
- `BackgroundJob.RefreshJobDelayInSeconds`: number of seconds to delay the first background refresh run. Defaults to `10`.

## Backoffice Configuration View

The package includes a read-only backoffice configuration workspace. It summarizes:

- Whether XML sitemaps and friendly rewrites are enabled.
- Alternate link rendering, root search level, and storage cleanup settings.
- Global filters for cultures, content types, and exclusion properties.
- Configured content sitemaps, custom sitemaps, sitemap indexes, and public rewrite links.

The view helps editors and developers inspect the active configuration, but sitemap settings are still managed through application configuration.

## Configuration Reference

Common root settings:

- `Enabled`: enables XML sitemap features. Defaults to `true`.
- `RewritesEnabled`: exposes configured entries as friendly XML rewrite paths.
- `IncludedContentTypeAliases`: global document type allow list.
- `ExcludedContentTypeAliases`: global document type deny list.
- `IncludedCultures`: global culture allow list.
- `ExcludedCultures`: global culture deny list.
- `ExcludingUrlPropertyAlias`: content property alias used for URL exclusion.
- `ExcludingUrlPropertyValue`: property value that excludes a content URL when found in `ExcludingUrlPropertyAlias`.
- `RenderAlternateLinksForSingleCultureSitemaps`: controls alternate link rendering for single-culture sitemaps.
- `RootNodeSearchLevel`: controls how routed site roots are resolved from the Umbraco tree.
- `UseDeliveryApiAccessPolicy`: enables the package's Delivery API access policy by default.
- `Sitemaps`: generated sitemap configurations.
- `CustomSitemaps`: custom provider-backed sitemap configurations.
- `Indexes`: sitemap index configurations.
- `Storage`: stored media and refresh settings.

## Developer Notes

The source solution is intentionally split into small projects while the NuGet package remains a single package for consumers. The package project includes the referenced internal assemblies in the package output and ships the built Umbraco backoffice assets from `wwwroot/App_Plugins/XmlSitemapsForUmbraco`.

The client source lives under `Casko.XmlSitemapsForUmbraco.Package/Client`. Its generated API client is produced with `@hey-api/openapi-ts` from the local package Swagger document. Rebuild the client after changing backoffice source or generated API contracts.

Useful commands from `src`:

```powershell
dotnet build
```

Useful commands from `src/Casko.XmlSitemapsForUmbraco.Package/Client`:

```powershell
npm run build
npm run watch
npm run generate-client
```

`npm run generate-client` expects a running local Umbraco instance exposing the XML sitemaps Swagger document.

## Notes

This package targets modern Umbraco projects and uses strongly typed configuration, dependency injection, OpenAPI-backed backoffice APIs, and XML sitemap model types. It is intended to be configured per site, with custom provider support for anything that needs project-specific data or sitemap rules.

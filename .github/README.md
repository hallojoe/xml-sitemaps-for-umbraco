# XML Sitemaps for Umbraco

[![Downloads](https://img.shields.io/nuget/dt/Casko.XmlSitemapsForUmbraco?color=cc9900)](https://www.nuget.org/packages/Casko.XmlSitemapsForUmbraco/)
[![NuGet](https://img.shields.io/nuget/vpre/Casko.XmlSitemapsForUmbraco?color=0273B3)](https://www.nuget.org/packages/Casko.XmlSitemapsForUmbraco)
[![GitHub license](https://img.shields.io/github/license/hallojoe/xml-sitemaps-for-umbraco?color=8AB803)](https://github.com/hallojoe/xml-sitemaps-for-umbraco/blob/main/LICENSE)

XML Sitemaps for Umbraco adds configurable XML sitemap and sitemap index delivery to Umbraco. It can render sitemaps from Umbraco content, expose friendly rewrite URLs such as `/xmlsitemap.xml`, store generated XML in Umbraco media, refresh stored files in the background, and let projects plug in custom sitemap providers for data that does not come from the content tree.

The package is built for teams that want sitemap behavior controlled from configuration while still keeping room for custom implementation when a site needs it.

Most sitemap behavior is registered behind interfaces, so projects can replace package services through dependency injection when the default implementation is not enough. This includes rendering, URL building, content collection, XML serialization, storage, and custom sitemap providers.

## Installation

Install the NuGet package:

```powershell
dotnet add package Casko.XmlSitemapsForUmbraco
```

The package composer registers the delivery API, rewrite pipeline, XML rendering services, Umbraco media storage, and the background refresh job.

## Quick Start

Add an `XmlSiteMaps` section to `appsettings.json`:

```json
{
  "XmlSiteMaps": {
    "Enabled": true,
    "RewritesEnabled": true,
    "RootNodeSearchLevel": 0,
    "UseDeliveryApiAccessPolicy": true,
    "IncludedCultures": [ "en", "da" ],
    "ExcludedCultures": [],
    "ExcludingUrlPropertyAlias": "metaRobots",
    "ExcludingUrlPropertyValue": "noindex",
    "RenderAlternateLinksForSingleCultureSitemaps": true,
    "Indexes": {
      "xmlsitemap": {
        "PublicName": "xmlsitemap",
        "HostName": "https://www.example.com",
        "Sitemaps": [ "xmlsitemap-en", "xmlsitemap-da" ]
      }
    },
    "Sitemaps": {
      "xmlsitemap-en": {
        "PublicName": "xmlsitemap-en",
        "Path": "/",
        "HostName": "https://www.example.com",
        "Culture": "en",
        "IncludedCultures": [ "en" ],
        "ExcludedCultures": [ "da" ]
      },
      "xmlsitemap-da": {
        "PublicName": "xmlsitemap-da",
        "Path": "/",
        "HostName": "https://www.example.com",
        "Culture": "da",
        "IncludedCultures": [ "da" ],
        "ExcludedCultures": [ "en" ]
      }
    }
  }
}
```

With rewrites enabled, the configured entries are available as:

- `/xmlsitemap.xml` for the sitemap index.
- `/xmlsitemap-en.xml` and `/xmlsitemap-da.xml` for configured sitemaps.

The delivery API is also available directly:

- `/api/sitemap/key?key=xmlsitemap-en`
- `/api/sitemap/index/key?key=xmlsitemap`

## Configured Sitemaps

Configured sitemaps live under `XmlSiteMaps:Sitemaps`. Each entry key is the internal unique ID used by the API, storage, and sitemap index configuration. Set `PublicName` when the public XML filename should differ from that internal key.

```json
{
  "XmlSiteMaps": {
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
- Values above `1` are not supported by the default content service and require a custom `ICmsContentService`.

Set `UseDeliveryApiAccessPolicy` at the root level to opt in or out of the Delivery API access policy used by the package's API endpoints. It defaults to `true`.

## Sitemap Indexes

Configured indexes live under `XmlSiteMaps:Indexes`. Each index lists sitemap keys to include:

```json
{
  "XmlSiteMaps": {
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

Custom sitemaps are for XML sitemap entries that should be generated by project code instead of the Umbraco content tree. Configure them under `XmlSiteMaps:CustomSitemaps`:

```json
{
  "XmlSiteMaps": {
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
using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Models;

public sealed class ExternalProductsSitemapProvider : IXmlSitemapCustomProvider
{
    public string Alias => "external-products-provider";

    public Task<XmlSiteMap> GetSitemapAsync(
        XmlSitemapCustomProviderContext context,
        CancellationToken cancellationToken = default)
    {
        var sitemap = new XmlSiteMap
        {
            Urls =
            [
                new XmlSiteMapUrl
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
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
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

Important settings:

- `PublicName`: public XML file name without `.xml`. Defaults to the entry key.
- `ProviderAlias`: alias of the registered `IXmlSitemapCustomProvider` implementation to execute.
- `HostName`: host used when rendering absolute URLs for the custom sitemap.
- `Settings`: provider-specific string values passed through to the custom provider context.

## Public Names

`PublicName` is available on `Sitemaps`, `CustomSitemaps`, and `Indexes`. It controls the public XML filename used by rewrite URLs and sitemap index locations. Do not include `.xml`; the package appends it.

If `PublicName` is omitted, the package uses the configuration key, preserving existing behavior. The configuration key must still be unique because it is the internal ID used by the API, storage, custom provider context, and index `Sitemaps` lists.

This is useful when multiple hostnames should publish the same public sitemap filename:

```json
{
  "XmlSiteMaps": {
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

Duplicate public names are allowed when different `HostName` values make them unambiguous. If two entries produce the same public path for the same host scope, the first definition wins.

## Stored Media Files And Refresh

The package stores generated XML sitemap files in Umbraco media. Stored files are reused when fresh and rebuilt when missing or stale.

```json
{
  "XmlSiteMaps": {
    "Storage": {
      "RefreshStaleAfterSeconds": 3600,
      "BackgroundJob": {
        "Enabled": true,
        "IntervalSeconds": 3600
      }
    }
  }
}
```

Storage behavior:

- Stored sitemap files are created in an `Xml Sitemaps` media folder.
- The background job starts after a 10 second delay.
- The job refreshes regular sitemaps, custom sitemaps, then sitemap indexes.
- Request-time delivery rebuilds a stored sitemap if the media file is older than `RefreshStaleAfterSeconds`.
- Set `RefreshStaleAfterSeconds` to `0` or less to disable request-time stale checks.

Important settings:

- `RefreshStaleAfterSeconds`: number of seconds before a stored sitemap is treated as stale. Defaults to `3600`.
- `BackgroundJob.Enabled`: enables the recurring refresh job. Defaults to `true`.
- `BackgroundJob.IntervalSeconds`: number of seconds between background refresh runs. Defaults to `3600`.

## Rewrite Delivery

When `RewritesEnabled` is `true`, configured entries are exposed as XML files at the site root using `PublicName` when configured, otherwise the internal key.

```json
{
  "XmlSiteMaps": {
    "RewritesEnabled": true
  }
}
```

Examples:

- Index key `xmlsitemap` becomes `/xmlsitemap.xml`.
- Sitemap key `products` becomes `/products.xml`.
- Custom sitemap key `external-products` becomes `/external-products.xml`.
- Sitemap key `products-en` with `PublicName` set to `products` becomes `/products.xml`.

If two configured entries produce the same path for the same host scope, the first definition wins. Sitemap indexes are registered before regular sitemaps, and regular sitemaps before custom sitemaps.

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

## Notes

This package targets modern Umbraco projects and uses strongly typed configuration, dependency injection, and the existing XML sitemap model types. It is intended to be configured per site, with custom provider support for anything that needs project-specific data or sitemap rules.

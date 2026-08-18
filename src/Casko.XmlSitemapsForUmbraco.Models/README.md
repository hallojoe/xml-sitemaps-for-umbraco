---
project: Casko.XmlSitemapsForUmbraco.Models
type: library
language: C#
framework: net10.0
solution_role: XML sitemap model contracts and XML serialization metadata
depends_on: []
used_by:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Http
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Providers
  - Casko.XmlSitemapsForUmbraco.Storage
  - Casko.XmlSitemapsForUmbraco.TestSite
  - Casko.XmlSitemapsForUmbraco.Common.Serialization
---

# Casko.XmlSitemapsForUmbraco.Models

## Purpose

This project defines the sitemap data structures shared across the package. The types model standard XML sitemap documents, sitemap indexes, URL entries, alternate culture links, and Google sitemap extensions for image, video, and news metadata.

The project also owns the XML serialization annotations and constants that make those models serialize to the expected sitemap element names and namespaces.

## Responsibilities

- Define root sitemap contracts through `IXmlSitemapModel`, `XmlSitemap`, and `XmlSitemapIndex`.
- Represent sitemap URL entries with `XmlSitemapUrl`, including `lastmod`, `changefreq`, `priority`, alternate language links, images, videos, and news metadata.
- Represent sitemap index entries with `XmlSitemapIndexLocation`.
- Represent extension metadata with `XHtmlLink`, `XmlSitemapImage`, `XmlSitemapVideo`, `XmlSitemapNews`, and related video/news helper models.
- Centralize sitemap XML element names, namespace URIs, formatting strings, and shared constant values in `Constants`.
- Provide small model-level helpers such as the `ChangeFrequency` enum and internal `PriorityValidationAttribute`.

## Non-responsibilities

- This project does not serialize, deserialize, stream, cache, or store XML; that belongs in projects such as `Casko.XmlSitemapsForUmbraco.Common.Serialization`, `Casko.XmlSitemapsForUmbraco.Http`, and `Casko.XmlSitemapsForUmbraco.Storage`.
- This project does not query Umbraco content, Examine indexes, or delivery API data.
- This project should not depend on Umbraco packages or service registration code. It is intentionally a small .NET model library with no direct project references.

## Project relationships

```text
Casko.XmlSitemapsForUmbraco.Models
       ^
       |
       +-- Casko.XmlSitemapsForUmbraco.Common
       +-- Casko.XmlSitemapsForUmbraco.Http
       +-- Casko.XmlSitemapsForUmbraco.Package
       +-- Casko.XmlSitemapsForUmbraco.Providers
       +-- Casko.XmlSitemapsForUmbraco.Storage
       +-- Casko.XmlSitemapsForUmbraco.TestSite
       +-- Casko.XmlSitemapsForUmbraco.Common.Serialization
```

### Dependencies

This project has no direct project references.

### Used by

| Project | Usage |
|---|---|
| `../Casko.XmlSitemapsForUmbraco.Common/Casko.XmlSitemapsForUmbraco.Common.csproj` | Shares model constants and contracts across common services. |
| `../Casko.XmlSitemapsForUmbraco.Http/Casko.XmlSitemapsForUmbraco.Http.csproj` | Returns sitemap model instances from HTTP result types. |
| `../Casko.XmlSitemapsForUmbraco.Package/Casko.XmlSitemapsForUmbraco.Package.csproj` | Includes the model assembly in the packaged Umbraco extension. |
| `../Casko.XmlSitemapsForUmbraco.Providers/Casko.XmlSitemapsForUmbraco.Providers.csproj` | Produces sitemap and sitemap index models from provider/rendering services. |
| `../Casko.XmlSitemapsForUmbraco.Storage/Casko.XmlSitemapsForUmbraco.Storage.csproj` | Stores and refreshes rendered sitemap model content. |
| `../Casko.XmlSitemapsForUmbraco.TestSite/Casko.XmlSitemapsForUmbraco.TestSite.csproj` | Uses the models in test-site custom sitemap provider examples. |
| `../Casko.XmlSitemapsForUmbraco.Common.Serialization/Casko.XmlSitemapsForUmbraco.Common.Serialization.csproj` | Serializes and deserializes `IXmlSitemapModel` implementations. |

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.Models.csproj` | Defines the `net10.0` library with nullable reference types and implicit usings enabled. |
| `IXmlSitemapModel.cs` | Marker interface for sitemap model roots consumed by serializers and HTTP results. |
| `XmlSitemap.cs` | Root `<urlset>` model and namespace declaration logic for optional sitemap extensions. |
| `XmlSitemapUrl.cs` | Main URL entry model, including XML serialization formatting for dates, change frequency, priority, culture links, image, video, and news data. |
| `XmlSitemapIndex.cs` | Root `<sitemapindex>` model. |
| `XmlSitemapIndexLocation.cs` | Individual sitemap reference inside a sitemap index. |
| `XHtmlLink.cs` | Alternate culture link model serialized with the XHTML namespace. |
| `XmlSitemapImage.cs` | Image sitemap extension model. |
| `XmlSitemapVideo.cs` | Video sitemap extension model, including optional dates, booleans, restrictions, platforms, uploader, and tags. |
| `XmlSitemapNews.cs` | News sitemap extension model with publication metadata and formatted publication date. |
| `Constants.cs` | Shared sitemap namespaces, XML element names, route-related constants, and formatting values. |
| `Enums/ChangeFrequency.cs` | Supported sitemap change frequency values plus `None` for omission. |
| `Attributes/PriorityValidationAttribute.cs` | Internal metadata for priority range expectations. |

## Public API

The main public model roots are `XmlSitemap` and `XmlSitemapIndex`; both implement `IXmlSitemapModel`. Most consumers work with these roots and nested model types when producing, serializing, storing, or returning sitemap XML.

`XmlSitemapUrl` is the primary URL contract. Its `LastModifiedFormatted`, `ChangeFrequencySerialized`, and `PrioritySpecified` members exist for `XmlSerializer` behavior and should be changed carefully because serializers and tests depend on their names and formatting.

`XmlSitemap.GetNamespaces()` and the `Namespaces` property add extension namespaces only when the sitemap contains culture links, images, videos, or news entries.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.Models/Casko.XmlSitemapsForUmbraco.Models.csproj
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj
```

There is no dedicated test project for this library. Existing unit tests cover the models through serialization, HTTP result, rendering, storage, and provider tests in `../Casko.XmlSitemapsForUmbraco.Tests`.

## Agent guidance

When modifying this project:

1. Treat public model properties and XML attributes as cross-project contracts.
2. Inspect `../Casko.XmlSitemapsForUmbraco.Common.Serialization` and relevant tests before changing XML element names, namespaces, serializer-facing property names, or date/boolean formatting.
3. Keep this project free of Umbraco-specific dependencies and service registration.
4. Add new sitemap extension models here only when they are shared model contracts, not provider-specific rendering concerns.
5. Update this README when public model contracts or direct project relationships change.

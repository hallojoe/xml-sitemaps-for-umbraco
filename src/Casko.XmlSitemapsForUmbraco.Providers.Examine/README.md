---
project: Casko.XmlSitemapsForUmbraco.Providers.Examine
type: library
language: C#
framework: net10.0
solution_role: Examine-backed live sitemap source provider
depends_on:
  - Casko.XmlSitemapsForUmbraco.Common
  - Casko.XmlSitemapsForUmbraco.Providers
used_by:
  - Casko.XmlSitemapsForUmbraco.Package
  - Casko.XmlSitemapsForUmbraco.Tests
---

# Casko.XmlSitemapsForUmbraco.Providers.Examine

## Purpose

Provides the live `IXmlSitemapSourceProvider` implementation. It finds content beneath a resolved root in an Umbraco Examine index, resolves culture-specific routes and domains, and renders the results into sitemap models.

The package selects this provider through `XmlSitemaps:IndexName`; supported values are Umbraco's `ExternalIndex` and `DeliveryApiContentIndex`.

## Responsibilities

- Implement root-key, path, configured sitemap, custom sitemap, and sitemap-index access through `ExamineXmlSitemapProvider`.
- Resolve configured roots from host URLs and document routes without using the published-content cache.
- Read indexed content URLs through `ICmsUrlService`.
- Render `CmsUrl` records into primary locations and culture alternate links.
- Support both `ExternalIndexUrlService` and `DeliveryApiContentIndexUrlService`.
- Add the `pathKeys` field required by ExternalIndex queries through `ExternalIndexUrlFieldsComponent`.

## Registration

```csharp
services.AddXmlSitemapExamineProvider(indexName);
```

The extension registers shared provider services, `IExamineSitemapRootResolver`, the selected `ICmsUrlService`, `IExamineUrlRenderer`, `IExamineXmlSitemapRenderer`, and `ExamineXmlSitemapProvider` as both `IXmlSitemapSourceProvider` and `IXmlSitemapProvider`.

Only the two supported Umbraco indexes may be passed to the registration method; an unsupported name throws an `InvalidOperationException`. Package composition passes `XmlSitemaps:IndexName`, whose default is `ExternalIndex`.

`ExternalIndexUrlFieldsComposer` appends the ExternalIndex field component only when that index is selected. The component adds `pathKeys` from each content item's ancestor path during indexing.

## URL and culture behavior

Both URL services create `CmsUrl` values from Umbraco document routes, assigned domains, routing settings, and the configured culture filters. A culture's assigned domain may include a path prefix, such as `/en` or `/pl`.

For each content item, the renderer groups culture variants by content id, selects the configured/default culture as the primary sitemap location, and emits alternate links for the selected cultures when alternates are enabled. Primary locations use the configured sitemap host when supplied. Alternate links prefer each culture URL's `CmsUrl.Hostname`, falling back to the resolved sitemap host only when that culture host is absent.

## Configuration

| Setting | Description |
| --- | --- |
| `XmlSitemaps:IndexName` | Selects `ExternalIndex` (default) or `DeliveryApiContentIndex`. |
| `XmlSitemaps:IncludedCultures` / `ExcludedCultures` | Filters cultures included in URLs. |
| `XmlSitemaps:ExcludingUrlPropertyAlias` / `ExcludingUrlPropertyValue` | Excludes matching indexed content from sitemap URLs. |

The URL services use `UrlResolverSettings.PageSize` with a default of `1000` when paging index results.

## Important files

| File | Responsibility |
| --- | --- |
| `ExamineXmlSitemapProvider.cs` | Live provider and configured/custom/index sitemap orchestration. |
| `Configuration/ServiceCollectionExtensions.cs` | Index-specific DI registration. |
| `Configuration/ExternalIndexUrlFieldsComposer.cs` | Conditional ExternalIndex component registration. |
| `Indexing/FieldsComponent.cs` | Adds the `pathKeys` indexed field. |
| `Routing/ExamineSitemapRootResolver.cs` | Resolves configured host roots and subsection paths. |
| `Urls/ExternalIndexUrlService.cs` | Reads and converts ExternalIndex results. |
| `Urls/DeliveryApiContentIndexUrlService.cs` | Reads and converts Delivery API index results. |
| `Rendering/ExamineUrlRenderer.cs` | Groups culture variants and renders locations/alternates. |

## Validation

```powershell
dotnet test src/Casko.XmlSitemapsForUmbraco.Tests/Casko.XmlSitemapsForUmbraco.Tests.csproj --filter "FullyQualifiedName~ExamineXmlSitemapProvider|FullyQualifiedName~ExamineUrlRenderer|FullyQualifiedName~ServiceComposition"
```

## Development notes

- Keep ExternalIndex `pathKeys` indexing and querying aligned.
- Preserve the culture-specific hostname fallback for alternate links.
- Keep root and path resolution on `IHostUrlProvider` and `IDocumentUrlService`.
- Update tests when index selection, URL grouping, culture selection, or rendering behavior changes.

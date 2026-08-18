---
project: Casko.XmlSitemapsForUmbraco.DemoSite
type: web-application
language: C#
framework: net10.0
solution_role: Demo Umbraco site that consumes the packaged XML sitemaps NuGet dependency
depends_on: []
used_by: []
---

# Casko.XmlSitemapsForUmbraco.DemoSite

## Purpose

This project is a runnable Umbraco demo site for trying the XML sitemaps package as a package dependency. It loads sitemap configuration from a dedicated JSON file, includes uSync content and domains, and provides local host names for multi-site sitemap scenarios.

The demo site is useful when checking consumer behavior after the package has been built or restored, rather than validating source-level project references.

## Responsibilities

- Boot an Umbraco 17 site with backoffice, website, delivery API, and composers enabled in `Program.cs`.
- Load `appsettings.XmlSitemapsForUmbraco.json` before Umbraco composers run so rewrite and sitemap configuration is available during composition.
- Consume the `Casko.XmlSitemapsForUmbraco` package through a NuGet package reference.
- Provide demo sitemap settings for multiple local hosts, sitemap indexes, content sitemaps, and a custom sitemap in `appsettings.XmlSitemapsForUmbraco.json`.
- Provide uSync content, languages, domains, templates, data types, and media under `uSync/v17`.
- Provide simple Razor templates in `Views`.

## Non-responsibilities

- This project is not a source-level package host. Its local `ProjectReference` entries are commented out in the project file.
- This project should not contain reusable package implementation. Package behavior belongs in `../Casko.XmlSitemapsForUmbraco.Package` and the supporting library projects.
- This project is not the automated integration test host; that role belongs to `../Casko.XmlSitemapsForUmbraco.TestSite`.

## Project relationships

This project has no direct project references and no direct project consumers.

It has an important package dependency on `Casko.XmlSitemapsForUmbraco`, with its floating version configured in `Directory.Packages.props` as `2.*`.

## Important files and entry points

| Path | Purpose |
|---|---|
| `Casko.XmlSitemapsForUmbraco.DemoSite.csproj` | Defines the Umbraco web host and package dependencies used by the demo site. |
| `Directory.Packages.props` | Central package versions for the demo, including `Casko.XmlSitemapsForUmbraco` version `2.*`. |
| `Program.cs` | Loads dedicated sitemap configuration and boots Umbraco with backoffice, website, delivery API, and composers. |
| `CustomSitemapProvider.cs` | Demonstrates a custom sitemap provider alias and sample URL generation against a configured host. |
| `appsettings.XmlSitemapsForUmbraco.json` | Main demo sitemap, index, rewrite, culture, and custom sitemap configuration. |
| `appsettings.Development.XmlSitemapsForUmbraco.json` | Development overrides for local sitemap host names. |
| `appsettings.json` | Base Umbraco settings for the demo site. |
| `Properties/launchSettings.json` | Local launch profiles and URLs for `https://localhost:44317`, `https://localhost:44318`, and `https://localhost:44319`. |
| `Views` | Simple Razor templates used by demo content. |
| `uSync/v17` | Serialized Umbraco demo content, domains, languages, templates, media, data types, and content types. |

## Configuration

`Program.cs` explicitly loads `appsettings.XmlSitemapsForUmbraco.json` before `.AddComposers()`. Keep that order intact when changing startup code because sitemap rewrite registration depends on configuration being available during composition.

The demo sitemap configuration includes:

- `xmlsitemapindex`, `xmlsitemapindex1`, and `xmlsitemapindex2` indexes.
- Host-specific content sitemaps for `https://localhost:44317`, `https://localhost:44318`, and `https://localhost:44319`.
- A custom sitemap key named `custom-sitemap` with provider alias `custom-sitemap-provider`.
- Culture coverage for `en`, `da`, and `pl`.

## Build and test

From the repository root:

```bash
dotnet build src/Casko.XmlSitemapsForUmbraco.DemoSite/Casko.XmlSitemapsForUmbraco.DemoSite.csproj
dotnet run --project src/Casko.XmlSitemapsForUmbraco.DemoSite/Casko.XmlSitemapsForUmbraco.DemoSite.csproj
```

There is no dedicated automated test project for the demo site. Use `../Casko.XmlSitemapsForUmbraco.Tests` for automated package behavior checks.

## Agent guidance

When modifying this project:

1. Preserve the package-reference behavior unless the task explicitly asks to test local source projects.
2. Keep sitemap host names aligned between `appsettings.XmlSitemapsForUmbraco.json`, `appsettings.Development.XmlSitemapsForUmbraco.json`, launch settings, and uSync domains.
3. Keep startup configuration loading before Umbraco composer registration.
4. Treat uSync files as demo content state; avoid broad churn unless content or domain behavior is intentionally changing.
5. Update this README when the package dependency mode, local ports, custom provider alias, or sitemap configuration layout changes.

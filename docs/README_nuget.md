# XML Sitemaps for Umbraco

[![Downloads](https://img.shields.io/nuget/dt/Casko.XmlSitemapsForUmbraco?color=cc9900)](https://www.nuget.org/packages/Casko.XmlSitemapsForUmbraco/)
[![NuGet](https://img.shields.io/nuget/vpre/Casko.XmlSitemapsForUmbraco?color=0273B3)](https://www.nuget.org/packages/Casko.XmlSitemapsForUmbraco)
[![GitHub license](https://img.shields.io/github/license/hallojoe/xml-sitemaps-for-umbraco?color=8AB803)](https://img.shields.io/github/license/hallojoe/xml-sitemaps-for-umbraco)

XML Sitemaps for Umbraco helps Umbraco projects publish XML sitemaps in a structured, configurable way. It is designed for teams that want reliable sitemap delivery for search engines without having to build and maintain the full solution themselves.

## What It Does

- Creates XML sitemaps from Umbraco content.
- Supports sitemap indexes for larger or multilingual sites.
- Publishes friendly URLs such as `/xmlsitemap.xml`.
- Stores generated sitemap files in Umbraco media.
- Refreshes stored sitemap files automatically in the background.
- Supports custom sitemap providers for content that does not live in the Umbraco tree.

## Why Teams Use It

- Improves SEO readiness by making sitemap delivery consistent.
- Reduces custom development for standard sitemap needs.
- Supports multilingual and multi-site setups through configuration.
- Leaves room for project-specific extensions when content comes from external systems.

## Typical Use Cases

- Corporate websites that need a standard XML sitemap setup.
- Multilingual Umbraco solutions with separate sitemap outputs per culture.
- Solutions with multiple sitemap files collected into one sitemap index.
- Projects that need to include products, listings, or other external data in a sitemap.

## Installation

```powershell
dotnet add package Casko.XmlSitemapsForUmbraco
```

## Implementation Summary

The package is configured in `appsettings.json` and can expose sitemap files directly on the site using rewrite URLs. Teams can define:

- Which cultures to include or exclude.
- Which parts of the content tree should appear in each sitemap.
- Whether pages marked with values such as `noindex` should be excluded.
- Which sitemap files should be grouped into a sitemap index.
- Additional custom XML sitemap providers can easily be implemented for project-specific needs.

## Good Fit If

- You want a reusable sitemap solution for Umbraco.
- You need more control than a simple one-file sitemap.
- You want sitemap behavior to be configuration-led rather than hardcoded.

## Full Documentation

For setup examples and technical configuration details, see the main README:

[README.md](../.github/README.md)

using Asp.Versioning;
using Casko.XmlSitemapsForUmbraco.Delivery.Authorization;
using Casko.XmlSitemapsForUmbraco.Http;
using Casko.XmlSitemapsForUmbraco.Models;
using Casko.XmlSitemapsForUmbraco.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Controllers;

[ApiExplorerSettings(GroupName = XmlSitemapApiConstants.ApiName)]
[ApiController]
[ApiVersion(XmlSitemapApiConstants.ApiVersion)]
[MapToApi($"{XmlSitemapApiConstants.ApiName}")]
[Route(XmlSitemapApiConstants.ApiRoute)]
[XmlSitemapsDeliveryApiAccess]
public class XmlSitemapDeliveryApiController(IXmlSitemapProvider xmlSitemapProvider) : ControllerBase
{
    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified alias.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("")]
    public async Task<IResult> GetXmlSiteMap(
        [FromQuery(Name = "name")]
        string sitemapName)
    {
        try
        {
            var xmlSiteMap = await xmlSitemapProvider.GetConfiguredAsync(sitemapName) as XmlSitemap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSitemap>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }

    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified alias.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("path")]
    public async Task<IResult> GetXmlSiteMapByPath(
        [FromQuery(Name = "path")]
        string path,
        [FromQuery(Name = "hostname")]
        string? hostname = null,
        [FromHeader(Name = "culture")] string? culture = null)
    {
        try
        {
            var xmlSiteMap = await xmlSitemapProvider.GetByPathAsync(path, culture, hostname) as XmlSitemap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSitemap>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }

    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified key.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("key")]
    public async Task<IResult> GetXmlSiteMapByKey([FromQuery(Name = "key")] string key)
    {
        try
        {
            var xmlSiteMap = await xmlSitemapProvider.GetConfiguredAsync(key) as XmlSitemap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSitemap>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }

    /// <summary>
    /// Returns the XML sitemap for the specified root content key.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("root-key")]
    public async Task<IResult> GetXmlSiteMapByRootKey([FromQuery(Name = "key")] Guid key)
    {
        try
        {
            var xmlSiteMap = await xmlSitemapProvider.GetByRootKeyAsync(key) as XmlSitemap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSitemap>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }
    
    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified key.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("index/key")]
    public IResult GetXmlSiteMapIndexByKey([FromQuery(Name = "key")] string key)
    {
        try
        {
            var xmlSiteMap = xmlSitemapProvider.GetIndex(key) as XmlSitemapIndex;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSitemapIndex>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }
    
}

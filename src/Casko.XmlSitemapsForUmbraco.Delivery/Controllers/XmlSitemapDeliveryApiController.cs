using Casko.XmlSitemapsForUmbraco.Common;
using Asp.Versioning;
using Casko.XmlSitemapsForUmbraco.Common.Http;
using Casko.XmlSitemapsForUmbraco.Common.Services;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Controllers;

[ApiExplorerSettings(GroupName = XmlSitemapApiConstants.ApiTitle)]
[ApiController]
[ApiVersion(XmlSitemapApiConstants.ApiVersion)]
[MapToApi($"{XmlSitemapApiConstants.ApiName}")]
[Route(XmlSitemapApiConstants.ApiRoute)]
// [Authorize(Policy = "RequireDeliveryApiKey")]
public class XmlSitemapDeliveryApiController(
    IXmlSitemapService xmlSitemapService) : ControllerBase
{
    private const string ApiKey = "Api-Key";

    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified alias.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("")]
    public async Task<IResult> GetXmlSiteMap(
        [FromQuery(Name = "name")]
        string sitemapName,
        [FromHeader(Name = ApiKey)]
        string? apiKey = null)
    {
        try
        {
            var xmlSiteMap = await xmlSitemapService.GetConfiguredAsync(sitemapName) as XmlSiteMap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSiteMap>(xmlSiteMap);
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
        [FromHeader(Name = "culture")]
        string? culture = null,
        [FromHeader(Name = ApiKey)]
        string? apiKey = null)
    {
        try
        {
            var xmlSiteMap = await xmlSitemapService.GetByPathAsync(path, culture, hostname) as XmlSiteMap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSiteMap>(xmlSiteMap);
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
            var xmlSiteMap = await xmlSitemapService.GetConfiguredAsync(key) as XmlSiteMap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSiteMap>(xmlSiteMap);
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
            var xmlSiteMap = await xmlSitemapService.GetByRootKeyAsync(key) as XmlSiteMap;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSiteMap>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }
    //
    // /// <summary>
    // /// Returns the stored XML sitemap for the specified key, creating it when it does not exist.
    // /// </summary>
    // [Produces(Modeling.Constants.XmlMimeType)]
    // [HttpGet("stored/key")]
    // public async Task<IResult> GetStoredXmlSiteMapByKey([FromQuery(Name = "key")] string key)
    // {
    //     try
    //     {
    //         var xmlSiteMap = await storedXmlSitemapService.GetConfiguredAsync(key) as XmlSiteMap;
    //         if (xmlSiteMap is null)
    //         {
    //             return Results.NotFound();
    //         }
    //
    //         return new XmlResult<XmlSiteMap>(xmlSiteMap);
    //     }
    //     catch (Exception exception)
    //     {
    //         return Results.BadRequest(exception.Message);
    //     }
    // }

    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified key.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("index/key")]
    public IResult GetXmlSiteMapIndexByKey([FromQuery(Name = "key")] string key)
    {
        try
        {
            var xmlSiteMap = xmlSitemapService.GetIndex(key) as XmlSiteMapIndex;
            if (xmlSiteMap is null)
            {
                return Results.NotFound();
            }

            return new XmlResult<XmlSiteMapIndex>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }
    //
    // /// <summary>
    // /// Returns the stored XML sitemap index for the specified key, creating it when it does not exist.
    // /// </summary>
    // [Produces(Modeling.Constants.XmlMimeType)]
    // [HttpGet("stored/index/key")]
    // public async Task<IResult> GetStoredXmlSiteMapIndexByKey([FromQuery(Name = "key")] string key)
    // {
    //     try
    //     {
    //         var xmlSiteMap = storedXmlSitemapService.GetIndex(key) as XmlSiteMapIndex;
    //         if (xmlSiteMap is null)
    //         {
    //             return Results.NotFound();
    //         }
    //
    //         return new XmlResult<XmlSiteMapIndex>(xmlSiteMap);
    //     }
    //     catch (Exception exception)
    //     {
    //         return Results.BadRequest(exception.Message);
    //     }
    // }

}

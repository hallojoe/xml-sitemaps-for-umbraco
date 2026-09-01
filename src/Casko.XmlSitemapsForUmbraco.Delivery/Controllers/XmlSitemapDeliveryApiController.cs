using Asp.Versioning;
using Casko.XmlSitemapsForUmbraco.Common;
using Casko.XmlSitemapsForUmbraco.Common.Http;
using Casko.XmlSitemapsForUmbraco.Delivery.Authorization;
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
            return await xmlSitemapProvider.GetConfiguredAsync(sitemapName) is not XmlSitemap xmlSiteMap 
                ? Results.NotFound() 
                : new XmlResult<XmlSitemap>(xmlSiteMap);
        }
        catch (Exception exception)
        {
            return Results.BadRequest(exception.Message);
        }
    }

    /// <summary>
    /// Returns the XML sitemap or sitemap index for the specified configured key.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("xmlsitemap")]
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
    /// Returns the XML sitemap or sitemap index for the specified configured key.
    /// </summary>
    [Produces(Constants.XmlMimeType)]
    [HttpGet("xmlsitemapindex")]
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

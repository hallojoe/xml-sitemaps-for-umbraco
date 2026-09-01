using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Package.Models;
using Casko.XmlSitemapsForUmbraco.Storage.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Authorization;

namespace Casko.XmlSitemapsForUmbraco.Package.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class XmlSitemapsApiController(
    IOptions<XmlSitemapsOptions> xmlSitemapOptions,
    IOptions<XmlSitemapStorageOptions> xmlSitemapStorageOptions,
    IConfiguration configuration)
    : XmlSitemapsApiControllerBase
{
    [HttpGet("configuration")]
    [ProducesResponseType<XmlSitemapConfigurationResponse>(StatusCodes.Status200OK)]
    public ActionResult<XmlSitemapConfigurationResponse> GetConfiguration()
    {
        var storageOptions = configuration.GetSection(XmlSitemapStorageOptions.Key).Exists()
            ? xmlSitemapStorageOptions.Value
            : null;

        return XmlSitemapConfigurationResponse.FromOptions(xmlSitemapOptions.Value, storageOptions);
    }
}

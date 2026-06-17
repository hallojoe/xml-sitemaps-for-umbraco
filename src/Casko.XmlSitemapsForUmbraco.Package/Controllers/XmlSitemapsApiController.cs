using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Package.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Web.Common.Authorization;

namespace Casko.XmlSitemapsForUmbraco.Package.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class XmlSitemapsApiController(IOptions<XmlSitemapsOptions> xmlSitemapOptions)
    : XmlSitemapsApiControllerBase
{
    [HttpGet("configuration")]
    [ProducesResponseType<XmlSitemapConfigurationResponse>(StatusCodes.Status200OK)]
    public ActionResult<XmlSitemapConfigurationResponse> GetConfiguration()
    {
        return XmlSitemapConfigurationResponse.FromOptions(xmlSitemapOptions.Value);
    }
}

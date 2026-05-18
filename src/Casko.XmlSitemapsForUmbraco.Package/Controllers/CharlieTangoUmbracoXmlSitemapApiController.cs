using Asp.Versioning;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Package.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Casko.XmlSitemapsForUmbraco.Package.Controllers;

[ApiController]
[ApiVersion("1.0")]
[VersionedApiBackOfficeRoute("xmlsitemapsforumbraco/api")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[MapToApi(XmlSitemapConstants.ApiName)]
public class CharlieTangoUmbracoXmlSitemapApiController(IOptions<XmlSitemapsOptions> xmlSitemapOptions)
    : ManagementApiControllerBase
{
    [HttpGet("configuration")]
    [ProducesResponseType<XmlSitemapConfigurationResponse>(StatusCodes.Status200OK)]
    public ActionResult<XmlSitemapConfigurationResponse> GetConfiguration()
    {
        return XmlSitemapConfigurationResponse.FromOptions(xmlSitemapOptions.Value);
    }
}

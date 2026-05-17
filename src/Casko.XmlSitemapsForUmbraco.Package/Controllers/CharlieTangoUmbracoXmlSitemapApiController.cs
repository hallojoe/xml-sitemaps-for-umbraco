using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;

namespace Casko.XmlSitemapsForUmbraco.Package.Controllers;

[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = XmlSitemapConstants.ApiGroup)]
public class CharlieTangoUmbracoXmlSitemapApiController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    : CharlieTangoUmbracoXmlSitemapApiControllerBase
{
}

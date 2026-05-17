using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Casko.XmlSitemapsForUmbraco.Package.Controllers;

[ApiController]
[BackOfficeRoute("charlietangoumbracoxmlsitemap/api/v{version:apiVersion}")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
[MapToApi(XmlSitemapConstants.ApiName)]
public class CharlieTangoUmbracoXmlSitemapApiControllerBase : ControllerBase
{
}
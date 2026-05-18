using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Web.Common.Authorization;

namespace Casko.XmlSitemapsForUmbraco.Package.Controllers;

[Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
public class XmlSitemapsApiControllerBase : ManagementApiControllerBase
{
}

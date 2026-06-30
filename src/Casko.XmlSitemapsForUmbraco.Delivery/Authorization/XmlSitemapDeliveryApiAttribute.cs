using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.DeliveryApi;

namespace Casko.XmlSitemapsForUmbraco.Delivery.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class XmlSitemapsDeliveryApiAccessAttribute() : TypeFilterAttribute(typeof(XmlSitemapDeliveryApiFilter))
{
    private sealed class XmlSitemapDeliveryApiFilter(
        IOptions<XmlSitemapsOptions> settings,
        IApiAccessService apiAccessService,
        IRequestPreviewService requestPreviewService)
        : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!settings.Value.UseDeliveryApiAccessPolicy)
            {
                return;
            }

            var hasAccess = requestPreviewService.IsPreview()
                ? apiAccessService.HasPreviewAccess()
                : apiAccessService.HasPublicAccess();

            if (!hasAccess)
            {
                context.Result = new UnauthorizedResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
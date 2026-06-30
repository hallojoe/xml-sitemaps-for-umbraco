using System.Reflection;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Casko.XmlSitemapsForUmbraco.Delivery.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;

namespace Casko.XmlSitemapsForUmbraco.Tests.Unit;

[TestFixture]
public sealed class XmlSitemapsDeliveryApiAccessAttributeTests
{
    [Test]
    public void OnActionExecuting_WhenPolicyDisabled_AllowsRequestToProceed()
    {
        var context = CreateActionExecutingContext();
        var sut = CreateFilter(
            new XmlSitemapsOptions { UseDeliveryApiAccessPolicy = false },
            apiAccessService: null,
            isPreview: false);

        sut.OnActionExecuting(context);

        Assert.That(context.Result, Is.Null);
    }

    [Test]
    public void OnActionExecuting_WhenPolicyEnabledAndPublicAccessDenied_ReturnsUnauthorized()
    {
        var context = CreateActionExecutingContext();
        var sut = CreateFilter(
            new XmlSitemapsOptions { UseDeliveryApiAccessPolicy = true },
            hasPublicAccess: false,
            isPreview: false);

        sut.OnActionExecuting(context);

        Assert.That(context.Result, Is.TypeOf<UnauthorizedResult>());
    }

    private static IActionFilter CreateFilter(
        XmlSitemapsOptions options,
        bool hasPublicAccess = true,
        bool hasPreviewAccess = true,
        bool isPreview = false,
        IApiAccessService? apiAccessService = null)
    {
        apiAccessService ??= Substitute.For<IApiAccessService>();
        apiAccessService.HasPublicAccess().Returns(hasPublicAccess);
        apiAccessService.HasPreviewAccess().Returns(hasPreviewAccess);

        var previewService = Substitute.For<IRequestPreviewService>();
        previewService.IsPreview().Returns(isPreview);

        var filterType = typeof(XmlSitemapsDeliveryApiAccessAttribute).GetNestedType(
            "XmlSitemapDeliveryApiFilter",
            BindingFlags.NonPublic);

        Assert.That(filterType, Is.Not.Null);

        var filter = Activator.CreateInstance(
            filterType!,
            Options.Create(options),
            apiAccessService,
            previewService);

        Assert.That(filter, Is.AssignableTo<IActionFilter>());
        return (IActionFilter)filter!;
    }

    private static ActionExecutingContext CreateActionExecutingContext()
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            [],
            new Dictionary<string, object?>(),
            controller: new object());
    }
}

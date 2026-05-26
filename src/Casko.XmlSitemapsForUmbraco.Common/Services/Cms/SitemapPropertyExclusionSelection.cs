using System.Collections;
using System.Globalization;
using Casko.XmlSitemapsForUmbraco.Common.Configuration;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Casko.XmlSitemapsForUmbraco.Common.Services.Cms;

public sealed class SitemapPropertyExclusionSelection
{
    private readonly string? _propertyAlias;
    private readonly string? _propertyValue;

    private SitemapPropertyExclusionSelection(string? propertyAlias, string? propertyValue)
    {
        _propertyAlias = Normalize(propertyAlias);
        _propertyValue = Normalize(propertyValue);
    }

    public bool IsEnabled =>
        string.IsNullOrWhiteSpace(_propertyAlias) is false &&
        string.IsNullOrWhiteSpace(_propertyValue) is false;

    public static SitemapPropertyExclusionSelection Resolve(XmlSitemapsOptions rootOptions)
    {
        return new SitemapPropertyExclusionSelection(
            rootOptions.ExcludingUrlPropertyAlias,
            rootOptions.ExcludingUrlPropertyValue);
    }

    public bool ShouldInclude(IPublishedContent content, string? culture)
    {
        if (IsEnabled is false)
        {
            return true;
        }

        var property = content.GetProperty(_propertyAlias!);
        if (property is null)
        {
            return true;
        }

        var propertyValue = property.GetValue(culture, segment: null);
        var propertyText = ConvertToText(propertyValue);
        if (string.IsNullOrWhiteSpace(propertyText))
        {
            return true;
        }

        return propertyText.Contains(_propertyValue!, StringComparison.OrdinalIgnoreCase) is false;
    }

    private static string? ConvertToText(object? value)
    {
        return value switch
        {
            null => null,
            string stringValue => stringValue,
            bool boolValue => boolValue ? bool.TrueString : bool.FalseString,
            IEnumerable enumerableValue => string.Join(",", enumerableValue.Cast<object?>().Select(ConvertToText)),
            IFormattable formattableValue => formattableValue.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

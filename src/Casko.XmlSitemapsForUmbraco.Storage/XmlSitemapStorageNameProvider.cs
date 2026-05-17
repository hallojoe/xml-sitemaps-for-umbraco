using System.Text;

namespace Casko.XmlSitemapsForUmbraco.Storage;

/// <summary>
/// Creates stable file names for stored XML sitemap documents.
/// </summary>
public interface IXmlSitemapStorageNameProvider
{
    /// <summary>
    /// Creates a file name for the supplied storage key.
    /// </summary>
    string GetFileName(XmlSitemapStorageKey key);
}

/// <inheritdoc />
public sealed class XmlSitemapStorageNameProvider : IXmlSitemapStorageNameProvider
{
    /// <inheritdoc />
    public string GetFileName(XmlSitemapStorageKey key)
    {
        key.Validate();

        var prefix = key.Kind == XmlSitemapDocumentKind.Sitemap
            ? "sitemap"
            : "sitemap-index";

        return $"{prefix}--{NormalizeHostName(key.HostName)}--{NormalizeSegment(key.Alias)}.xml";
    }

    internal static string NormalizeHostName(string? hostName)
    {
        if (string.IsNullOrWhiteSpace(hostName))
        {
            return "default";
        }

        var candidate = hostName.Trim();
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            Uri.TryCreate($"https://{candidate}", UriKind.Absolute, out uri);
        }

        var host = uri?.Host;
        return string.IsNullOrWhiteSpace(host)
            ? "default"
            : NormalizeSegment(host);
    }

    internal static string NormalizeSegment(string value)
    {
        var builder = new StringBuilder(value.Length);
        var lastWasSeparator = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastWasSeparator = false;
                continue;
            }

            if (lastWasSeparator || builder.Length == 0)
            {
                continue;
            }

            builder.Append('-');
            lastWasSeparator = true;
        }

        return builder.ToString().Trim('-');
    }
}

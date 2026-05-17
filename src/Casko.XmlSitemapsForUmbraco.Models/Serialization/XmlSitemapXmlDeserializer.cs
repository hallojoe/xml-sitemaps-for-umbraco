using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models.Serialization;

/// <inheritdoc />
public sealed class XmlSitemapXmlDeserializer : IXmlSitemapXmlDeserializer
{
    /// <inheritdoc />
    public T Deserialize<T>(string xml)
        where T : IXmlSiteMapModel
    {
        ArgumentNullException.ThrowIfNull(xml);

        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T)serializer.Deserialize(reader)!;
    }
}

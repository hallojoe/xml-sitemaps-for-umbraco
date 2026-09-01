using System.Xml.Serialization;
using Casko.XmlSitemapsForUmbraco.Models;

namespace Casko.XmlSitemapsForUmbraco.Common.Serialization;

/// <inheritdoc />
public sealed class XmlSitemapXmlDeserializer : IXmlSitemapXmlDeserializer
{
    /// <inheritdoc />
    public T Deserialize<T>(string xml)
        where T : IXmlSitemapModel
    {
        ArgumentNullException.ThrowIfNull(xml);

        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T)serializer.Deserialize(reader)!;
    }
}

using System.Text;
using System.Xml.Serialization;

namespace Casko.XmlSitemapsForUmbraco.Models.Serialization;

/// <inheritdoc />
public sealed class XmlSitemapXmlSerializer : IXmlSitemapXmlSerializer
{
    /// <inheritdoc />
    public string Serialize<T>(T model)
        where T : IXmlSitemapModel
    {
        var serializer = new XmlSerializer(typeof(T));
        using var stream = new MemoryStream();

        if (model is XmlSitemap siteMap)
        {
            serializer.Serialize(stream, model, siteMap.GetNamespaces());
        }
        else
        {
            serializer.Serialize(stream, model);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}

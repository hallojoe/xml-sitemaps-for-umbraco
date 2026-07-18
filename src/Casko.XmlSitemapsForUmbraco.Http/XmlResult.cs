using System.Xml.Serialization;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Casko.XmlSitemapsForUmbraco.Http;

public class XmlResult<T>(T result) : IResult
{
    private static readonly XmlSerializer _serializer = new(typeof(T));

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await using var fileBufferingWriteStream = new FileBufferingWriteStream();

        if (result is XmlSitemap siteMap)
        {
            _serializer.Serialize(fileBufferingWriteStream, result, siteMap.GetNamespaces());
        }
        else
        {
            _serializer.Serialize(fileBufferingWriteStream, result);
        }

        httpContext.Response.ContentType = Constants.XmlMimeType;

        await fileBufferingWriteStream.DrainBufferAsync(httpContext.Response.Body);
    }
}

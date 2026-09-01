using System.Xml.Serialization;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace Casko.XmlSitemapsForUmbraco.Common.Http;

public class XmlResult<T>(T result) : IResult
{
    private static readonly XmlSerializer Serializer = new(typeof(T));

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await using var fileBufferingWriteStream = new FileBufferingWriteStream();

        if (result is XmlSitemap siteMap)
        {
            Serializer.Serialize(fileBufferingWriteStream, result, siteMap.GetNamespaces());
        }
        else
        {
            Serializer.Serialize(fileBufferingWriteStream, result);
        }

        httpContext.Response.ContentType = Constants.XmlMimeType;

        await fileBufferingWriteStream.DrainBufferAsync(httpContext.Response.Body);
    }
}

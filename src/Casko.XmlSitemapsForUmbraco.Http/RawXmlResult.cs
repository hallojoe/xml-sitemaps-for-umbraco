using System.Text;
using Casko.XmlSitemapsForUmbraco.Models;
using Microsoft.AspNetCore.Http;

namespace Casko.XmlSitemapsForUmbraco.Http;

public sealed class RawXmlResult(string xml) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = Constants.XmlMimeType;
        await httpContext.Response.WriteAsync(xml, Encoding.UTF8);
    }
}

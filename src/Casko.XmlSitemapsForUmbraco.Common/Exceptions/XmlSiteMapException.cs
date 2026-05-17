namespace Casko.XmlSitemapsForUmbraco.Common.Exceptions;

/// <summary>
/// General XML sitemap related exception.
/// </summary>
public class XmlSiteMapException : Exception
{
    public XmlSiteMapException() 
        : base("General XML sitemap exception.") { }

    public XmlSiteMapException(string message) 
        : base(message) { }

    public XmlSiteMapException(string message, Exception innerException) 
        : base(message, innerException) { }
}
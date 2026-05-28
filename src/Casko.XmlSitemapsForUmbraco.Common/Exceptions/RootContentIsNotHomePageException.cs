namespace Casko.XmlSitemapsForUmbraco.Common.Exceptions;

/// <summary>
/// Thrown when root content is not home page.
/// </summary>
public class RootContentIsNotHomePageException : XmlSiteMapException
{
    public RootContentIsNotHomePageException() 
        : base("Root content is not of expected type HomePage.") { }

    public RootContentIsNotHomePageException(string message) 
        : base(message) { }

    public RootContentIsNotHomePageException(string message, Exception innerException) 
        : base(message, innerException) { }
}
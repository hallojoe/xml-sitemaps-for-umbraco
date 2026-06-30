namespace Casko.XmlSitemapsForUmbraco.Common.Exceptions;

/// <summary>
/// Thrown when a root content exists but contains no content.
/// </summary>
public class RootContentHasNoContentException : XmlSiteMapException
{
    public RootContentHasNoContentException() 
        : base("No content found under root content.") { }

    public RootContentHasNoContentException(string message) 
        : base(message) { }

    public RootContentHasNoContentException(string message, Exception innerException) 
        : base(message, innerException) { }
}
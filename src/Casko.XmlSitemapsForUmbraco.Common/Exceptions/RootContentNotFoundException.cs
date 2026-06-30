namespace Casko.XmlSitemapsForUmbraco.Common.Exceptions;

/// <summary>
/// Thrown when no root content is found under the specified key.
/// </summary>
public class RootContentNotFoundException : XmlSiteMapException
{
    public RootContentNotFoundException() 
        : base("No root found under key.") { }

    public RootContentNotFoundException(string message) 
        : base(message) { }

    public RootContentNotFoundException(string message, Exception innerException) 
        : base(message, innerException) { }
}
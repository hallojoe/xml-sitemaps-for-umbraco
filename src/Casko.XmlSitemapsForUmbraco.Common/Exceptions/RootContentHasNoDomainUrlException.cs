namespace Casko.XmlSitemapsForUmbraco.Common.Exceptions;

public class RootContentHasNoDomainUrlException : XmlSiteMapException
{
    public RootContentHasNoDomainUrlException() 
        : base("No domain found on root.") { }

    public RootContentHasNoDomainUrlException(string message) 
        : base(message) { }

    public RootContentHasNoDomainUrlException(string message, Exception innerException) 
        : base(message, innerException) { }
}
using System.Net;

namespace Pastebin.Exceptions;
 
public abstract class PastebinException : Exception
{
    public abstract HttpStatusCode StatusCode { get; }
    public virtual string ErrorCode => GetType().Name;

    protected PastebinException(string message) : base(message) { }
    protected PastebinException(string message, Exception innerException) : base(message, innerException) { }
}

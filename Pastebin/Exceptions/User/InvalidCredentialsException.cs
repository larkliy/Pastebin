using System.Net;

namespace Pastebin.Exceptions.User;

public class InvalidCredentialsException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public InvalidCredentialsException(string message) : base(message) { }
}

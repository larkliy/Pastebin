using System.Net;

namespace Pastebin.Exceptions.User;

public class InvalidRefreshTokenException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
    public InvalidRefreshTokenException(string message) : base(message) { }
}

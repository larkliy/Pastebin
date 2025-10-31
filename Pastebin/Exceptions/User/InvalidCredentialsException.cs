using System.Net;

namespace Pastebin.Exceptions.User;

public class InvalidCredentialsException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Unauthorized;
}

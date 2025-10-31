using System.Net;

namespace Pastebin.Exceptions.User;

public class UserNotFoundException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}

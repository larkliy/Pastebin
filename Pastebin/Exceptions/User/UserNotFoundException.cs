using System.Net;

namespace Pastebin.Exceptions.User;

public class UserNotFoundException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;

    public UserNotFoundException(string message) : base(message) { }
}

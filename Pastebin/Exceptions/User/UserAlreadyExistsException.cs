using System.Net;

namespace Pastebin.Exceptions.User;

public class UserAlreadyExistsException(string obj) : PastebinException(obj)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}


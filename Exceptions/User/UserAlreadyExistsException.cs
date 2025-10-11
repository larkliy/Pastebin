using System.Net;

namespace Pastebin.Exceptions.User;

public class UserAlreadyExistsException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;

    public UserAlreadyExistsException(string obj) : base(obj) { }
}


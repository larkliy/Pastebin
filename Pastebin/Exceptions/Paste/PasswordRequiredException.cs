using System.Net;

namespace Pastebin.Exceptions.Paste;

public class PasswordRequiredException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}

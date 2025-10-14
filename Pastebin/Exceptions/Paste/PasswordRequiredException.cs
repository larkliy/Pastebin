using System.Net;

namespace Pastebin.Exceptions.Paste;

public class PasswordRequiredException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public PasswordRequiredException(string message) : base(message) { }
}

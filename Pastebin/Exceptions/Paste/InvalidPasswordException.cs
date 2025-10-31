
using System.Net;

namespace Pastebin.Exceptions.Paste;

public class InvalidPasswordException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}
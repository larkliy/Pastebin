
using System.Net;

namespace Pastebin.Exceptions.Paste;

public class InvalidPasswordException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
    public InvalidPasswordException(string message) : base(message) { }
}
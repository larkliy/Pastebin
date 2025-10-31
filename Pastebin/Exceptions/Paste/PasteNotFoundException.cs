
using System.Net;

namespace Pastebin.Exceptions.Paste;

public class PasteNotFoundException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}

using System.Net;

namespace Pastebin.Exceptions.Paste;

public class PasteNotFoundException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public PasteNotFoundException(string message) : base(message) { }
}
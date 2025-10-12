
using System.Net;

namespace Pastebin.Exceptions.Like;

public class LikeNotFoundException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    
    public LikeNotFoundException(string message) : base(message) { }
}
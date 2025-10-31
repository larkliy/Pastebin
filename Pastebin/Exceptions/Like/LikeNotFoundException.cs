
using System.Net;

namespace Pastebin.Exceptions.Like;

public class LikeNotFoundException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}
using System.Net;

namespace Pastebin.Exceptions.Like;

public class LikeAlreadyExistsException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}

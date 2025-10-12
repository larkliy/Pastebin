using System.Net;

namespace Pastebin.Exceptions.Like;

public class LikeAlreadyExistsException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public LikeAlreadyExistsException(string message) : base(message) { }
}

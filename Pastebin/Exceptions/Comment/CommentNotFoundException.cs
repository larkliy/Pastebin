
using System.Net;

namespace Pastebin.Exceptions.Comment;

public class CommentNotFoundException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}
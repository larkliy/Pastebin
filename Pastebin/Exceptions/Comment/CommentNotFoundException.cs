
using System.Net;

namespace Pastebin.Exceptions.Comment;

public class CommentNotFoundException : PastebinException
{
    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    public CommentNotFoundException(string message) : base(message) {}

}
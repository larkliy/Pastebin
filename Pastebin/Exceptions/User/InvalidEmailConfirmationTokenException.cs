using System;
using System.Net;

namespace Pastebin.Exceptions.User;

public class InvalidEmailConfirmationTokenException(string message) : PastebinException(message)
{
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}

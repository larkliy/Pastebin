﻿using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pastebin.Exceptions;
using System.Net;

namespace Pastebin.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
                                                Exception exception,
                                                CancellationToken cancellationToken)
    {
        if (exception is PastebinException)
        {
            logger.LogWarning(exception, "A handled business exception occurred: {Message}", exception.Message);
        }
        else
        {
            logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }

        var (statusCode, errorCode, message) = GetResponseDetails(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = errorCode,
            Detail = message,
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string ErrorCode, string Message) GetResponseDetails(Exception ex) 
        => ex switch
    {
        PastebinException pastebinEx => ((int)pastebinEx.StatusCode, pastebinEx.ErrorCode, pastebinEx.Message),
        _ => ((int)HttpStatusCode.InternalServerError, "InternalServerError", "An unexpected internal server error has occurred.")
    };
}

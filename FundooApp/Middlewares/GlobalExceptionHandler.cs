using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FundooApp.Middlewares;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        const int status = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails()
            {
                Status = status,
                Detail = exception.Message
            }
            , cancellationToken: cancellationToken);
        logger.LogError("Unhandled exception occured : {Message} in {Source}", exception.Message, exception.Source);
        return true;
    }
}
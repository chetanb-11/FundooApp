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
                Detail = "An unexpected error occurred while processing your request."
            }
            , cancellationToken: cancellationToken);
        logger.LogError(exception, "Unhandled exception occurred while processing request");
        return true;
    }
}

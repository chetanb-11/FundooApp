using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FundooApp.Filters;

public class LogExecutionTimeFilter(ILogger<LogExecutionTimeFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sw = Stopwatch.StartNew();
        var executedContext = await next();
        sw.Stop();
        
        logger.LogInformation("Action {ActionName} took {ExecutionTime}ms to execute", context.ActionDescriptor.DisplayName, sw.ElapsedMilliseconds);
    }
}
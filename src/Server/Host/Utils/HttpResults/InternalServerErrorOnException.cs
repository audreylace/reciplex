using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Reciplex.Server.Host.Utils.HttpResults;

/// <summary>
/// Generates a <see cref="CustomProblemHttpResults.InternalServerError"/> when the controller throws
/// </summary>
/// <param name="logger">logger used to record the exception</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public partial class InternalServerErrorOnException(
    ILogger<InternalServerErrorOnException> logger,
    IHostEnvironment hostEnvironment
) : ExceptionFilterAttribute
{
    [LoggerMessage(LogLevel.Debug, "Unhandled exception thrown by {ControllerName}")]
    private static partial void LogError(
        ILogger<InternalServerErrorOnException> logger,
        Exception ex,
        string? ControllerName
    );

    /// <inheritdoc />
    public override void OnException(ExceptionContext context)
    {
        string? exceptionMessage = null;
        string? stackTrace = null;
        if (hostEnvironment.IsDevelopment()) // provide more info if running in development
        {
            exceptionMessage = context.Exception.Message;
            stackTrace = context.Exception.StackTrace;
        }
        LogError(logger, context.Exception, context.ActionDescriptor.DisplayName);

        context.Result = new ObjectResult(
            CustomProblemHttpResults.InternalServerError(exceptionMessage, stackTrace)
        );
        context.ExceptionHandled = true;
    }
}

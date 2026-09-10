using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Configurations;

/// <summary>
/// Captura qualquer exceção não tratada no pipeline e devolve um ProblemDetails (RFC 7807)
/// em vez de um 500 sem corpo. Loga a exceção com o TraceId para correlação.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method, httpContext.Request.Path, traceId);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno ao processar a requisição.",
            Detail = "Ocorreu um erro inesperado. Use o traceId para localizar o log correspondente.",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

using GestionSaaS.Negocio.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GestionSaaS.Api.Infraestructura;

/// <summary>Convierte las excepciones de Negocio en respuestas HTTP (ProblemDetails).</summary>
public class ManejoErrores(ILogger<ManejoErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, titulo) = ex switch
        {
            NoAutorizadoException => (StatusCodes.Status401Unauthorized, ex.Message),
            NoEncontradoException => (StatusCodes.Status404NotFound, ex.Message),
            NegocioException      => (StatusCodes.Status400BadRequest, ex.Message),
            _                     => (StatusCodes.Status500InternalServerError, "Error interno del servidor.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Error no controlado");

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = titulo }, ct);
        return true;
    }
}

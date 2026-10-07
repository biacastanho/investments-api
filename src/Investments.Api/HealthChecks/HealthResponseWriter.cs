using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Investments.Api.HealthChecks;

/// <summary>Monta a resposta do /health com o status de cada verificação e a versão publicada.</summary>
public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions Options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static readonly string Version =
        typeof(HealthResponseWriter).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "desconhecida";

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var body = new
        {
            status = report.Status.ToString(),
            version = Version,
            environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName,
            checks = report.Entries.ToDictionary(e => e.Key, e => e.Value.Status.ToString())
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(body, Options));
    }
}

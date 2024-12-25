using System.Diagnostics;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace KernelMemory.Ecommerce.Sample.Api.Infrastructure.Logging;

public static class OpenTelemetryRegistrationExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(
        this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("kernelmemory.ecommerce.sample.api"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            if (request.RequestUri != null)
                            {
                                activity.DisplayName = $"{request.Method.Method} {request.RequestUri.AbsoluteUri}";
                            }
                        };

                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.DisplayName = $"{activity.DisplayName} {(int)response.StatusCode}";
                        };
                    })
                    .AddAspNetCoreInstrumentation()
                    .AddNpgsql();

                // Exporter is configured in docker-compose via
                // OTEL_EXPORTER_OTLP_ENDPOINT variable.
                tracing.AddOtlpExporter();
            });

        return builder;
    }
}

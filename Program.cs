using GeoLocationAPI.V1.HealthChecks;
using GeoLocationAPI.V1.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddSingleton<IGeoLocationService, GeoLocationService>();
builder.Services.AddHealthChecks()
    .AddTypeActivatedCheck<GeoLocationHealthCheck>(
        "GeoLocationHealthCheck",
        args: [
            builder.Configuration.GetValue<string>("HealtcheckBaseURL"),
            builder.Configuration.GetValue<string>("HealtcheckIPToTest")
            ]
        );

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/hc", new HealthCheckOptions
{
    ResponseWriter = WriteResponse
});

app.Run();

Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("Status", healthReport.Status.ToString());
        jsonWriter.WriteString("Duration", healthReport.TotalDuration.ToString());
        jsonWriter.WriteString("FrameworkDescription", RuntimeInformation.FrameworkDescription);
        jsonWriter.WriteString("ProcessArchitecture", RuntimeInformation.ProcessArchitecture.ToString());
        jsonWriter.WriteStartObject("Results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("Status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("Description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("Data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}

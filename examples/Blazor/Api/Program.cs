using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBufJsonConverter;
using Serilog;

namespace Api;

public class Program
{
    public static void Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", true, false)
            .AddEnvironmentVariables()
            .Build();

        // Register Serilog provider
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
            .ReadFrom.Configuration(config)
            .CreateLogger();

        var hostBuilder = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults();

        hostBuilder.ConfigureServices((_, s) => s
            .AddLogging(lb => lb.AddSerilog(logger, dispose: true))
            .AddSingleton<IConverter, Converter>()
        );

        hostBuilder.Build().Run();
    }
}
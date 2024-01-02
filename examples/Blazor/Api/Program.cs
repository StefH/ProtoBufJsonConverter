using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBufJsonConverter;

namespace Api;

public class Program
{
    public static void Main()
    {
        var hostBuilder = new HostBuilder().ConfigureFunctionsWorkerDefaults();

        hostBuilder.ConfigureServices((_, s) => s
            .AddSingleton<IJsonConverter, NewtonsoftJsonConverter>()
            .AddSingleton<IConverter, Converter>()
        );

        hostBuilder.Build().Run();
    }
}
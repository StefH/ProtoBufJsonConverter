using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Api;

public class Program
{
    public static void Main()
    {
        var hostBuilder = new HostBuilder().ConfigureFunctionsWorkerDefaults();

        hostBuilder.ConfigureServices((_, s) => s.AddSingleton<ImageService>());

        hostBuilder.Build().Run();
    }
}
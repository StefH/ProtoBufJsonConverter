using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Blazorise.RichTextEdit;
using HighlightBlazor;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.BlazorWasm;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace ProtoBufJsonConverter.Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services
            .AddHighlight()
            .AddBlazorise(options =>
            {
                options.Immediate = true;
            })
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons()
            .AddBlazoriseRichTextEdit();

        builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton<IMetadataReferenceService, BlazorWasmMetadataReferenceService>();
        builder.Services.AddSingleton<IConverter, Converter>();

        await builder.Build().RunAsync();
    }
}
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Client8;
using Client8.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestEase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddBlazorise(options =>
    {
        options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

var baseAddress = builder.HostEnvironment.BaseAddress;
Console.WriteLine("HostEnvironment.BaseAddress = " + baseAddress);

var isLocalHost = baseAddress.Contains("localhost");
Console.WriteLine("isLocalHost = " + isLocalHost);

var isAzure = baseAddress.Contains("azurestaticapps.net");
Console.WriteLine("isAzure = " + isAzure);

var httpClientBaseAddress = isLocalHost ? "http://localhost:7071/" : baseAddress;
Console.WriteLine("httpClientBaseAddress = " + httpClientBaseAddress);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseAddress) });
builder.Services.AddScoped(_ =>
{
    var httpClient = new HttpClient
    {
        BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? httpClientBaseAddress)
    };
    return new RestClient(httpClient).For<IProtoBufConverterApi>();
});

await builder.Build().RunAsync();

Skclusive.Script.Prism
=============================

Code Highliter component library for Blazor. Integrating PrismJs.

## Installation

Add a reference to the library from [![NuGet](https://img.shields.io/nuget/v/Skclusive.Script.Prism.svg)](https://www.nuget.org/packages/Skclusive.Script.Prism/)

## Usage

Add the following in `_Imports.razor`:

```cs
@using Skclusive.Script.Prism
```

Make the registration in **Startup.cs**

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddPrism();
    }

    public void Configure(IComponentsApplicationBuilder app)
    {
        app.AddComponent<App>("app");
    }
}
```

Add the resources in **App.razor**

```html
<PrismStyles />
<PrismScript />

<Router AppAssembly="@typeof(Program).Assembly">
    ....
</Router>
```

you can use the **PrismCode** component to render code block as blow

```html
<PrismCode
  Code="@(@"<body>
    <div style=""padding: 20px;"">
      <Grid Container Spacing=""@Spacing.Five"">
        //...
      </Grid>
    </div>
  </body>")"
/>
```

Following is the rendered output in dark theme.

![Code Rendered](images/rendered.png)

## License

Skclusive.Script.Prism is licensed under [MIT license](http://www.opensource.org/licenses/mit-license.php)

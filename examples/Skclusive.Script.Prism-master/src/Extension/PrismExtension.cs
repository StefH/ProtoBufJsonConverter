using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Skclusive.Core.Component;

namespace Skclusive.Script.Prism
{
    public static class PrismExtension
    {
        public static void TryAddPrismServices(this IServiceCollection services, ICoreConfig config)
        {
            services.TryAddCoreServices(config);
            services.TryAddScoped<IPrismHighlighter, PrismHighlighter>();
            services.TryAddScriptTypeProvider<PrismScriptProvider>();
            services.TryAddStyleTypeProvider<PrismStyleProvider>();
        }
    }
}

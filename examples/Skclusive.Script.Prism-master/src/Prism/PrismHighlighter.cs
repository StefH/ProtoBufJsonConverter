using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Skclusive.Script.Prism
{
    public class PrismHighlighter : IPrismHighlighter
    {
        private IJSRuntime JSRuntime { get; }

        public PrismHighlighter(IJSRuntime jsruntime)
        {
            JSRuntime = jsruntime;
        }

        public async Task<MarkupString> HighlightAsync(string code, string language)
        {
            string hilighted = await JSRuntime.InvokeAsync<string>("Skclusive.Script.Prism.highlight", code, language);

            return new MarkupString(hilighted);
        }

        public async Task HighlighterAsync(ElementReference element, string code, string language)
        {
            await JSRuntime.InvokeVoidAsync("Skclusive.Script.Prism.highlighter", element, code, language);
        }
    }
}

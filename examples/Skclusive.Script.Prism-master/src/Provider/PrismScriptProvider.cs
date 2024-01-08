using Skclusive.Core.Component;

namespace Skclusive.Script.Prism
{
    public class PrismScriptProvider : ScriptTypeProvider
    {
        public PrismScriptProvider() : base(priority: 1200, typeof(PrismScript))
        {
        }
    }
}

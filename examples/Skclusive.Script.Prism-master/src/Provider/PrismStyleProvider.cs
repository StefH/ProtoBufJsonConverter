using Skclusive.Core.Component;

namespace Skclusive.Script.Prism
{
    public class PrismStyleProvider : StyleTypeProvider
    {
        public PrismStyleProvider() : base(priority: 1200, typeof(PrismLightStyle), typeof(PrismDarkStyle))
        {
        }
    }
}

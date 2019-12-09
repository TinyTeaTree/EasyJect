using EasyJect.Internal;

namespace EasyJect
{
    public class EasySupplier : BaseInjectBehaviour
    {
        public override bool ConsumeInject { get { return false; } }
        public override bool SupplyInject { get { return true; } }
    }
}
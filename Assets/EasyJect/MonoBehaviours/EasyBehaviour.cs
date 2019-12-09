using EasyJect.Internal;

namespace EasyJect
{
    public class EasyBehaviour : BaseInjectBehaviour
    {
        public override bool ConsumeInject { get { return true; } }
        public override bool SupplyInject { get { return true; } }
    }
}
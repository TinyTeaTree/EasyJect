using EasyJect.Internal;

namespace EasyJect
{
    public class EasyConsumer : BaseInjectBehaviour
    {
        public override bool ConsumeInject { get { return true; } }
        public override bool SupplyInject { get { return false; } }
    }
}

using EasyJect;

namespace Example_CommandAutoListen
{
    public class BasicSceneBinder : BaseInjectionBinder
    {
        protected override bool ShouldAutoCreateAttributedEntities => true;

        protected override void InitializeBindings()
        {
        }
    }
}

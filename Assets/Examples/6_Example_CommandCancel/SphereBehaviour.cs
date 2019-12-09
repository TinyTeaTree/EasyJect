using EasyJect;

namespace Example_CommandCancel
{

    public class SphereBehaviour : EasyBehaviour
    {
        [Inject] Listener<MoveCubeCommand> MoveCubeListener { get; set; }

        private void Start()
        {
            MoveCubeListener.AddListener(() =>
            {
                transform.localScale *= 1.2f;
            });
        }
    }
}
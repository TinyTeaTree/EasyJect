using EasyJect;

namespace Example_InterfaceInjection
{
    public class CubeMover : EasyBehaviour
    {
        [Inject] ICube SelectedCube { get; set; }

        //Clicked from Button
        public void Move()
        {
            SelectedCube.Move();
        }
    }
}
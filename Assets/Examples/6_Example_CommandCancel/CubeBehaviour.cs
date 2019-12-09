using EasyJect;
using UnityEngine;

namespace Example_CommandCancel
{

    public class CubeBehaviour : EasyBehaviour
    {
        [Inject] Invoker<MoveCubeCommand> MoveCubeInvoker { get; set; }

        public void Move()
        {
            transform.Translate(Vector3.right * Time.deltaTime * 3, Space.World);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                MoveCubeInvoker.Invoke();
            }
        }
    }
}
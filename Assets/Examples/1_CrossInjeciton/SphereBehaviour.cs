using EasyJect;
using UnityEngine;

namespace Example_CrossInjection
{
    public class SphereBehaviour : EasyBehaviour
    {
        [Inject] CubeBehaviour Cube { get; set; }

        public void Move()
        {
            transform.Translate(Vector3.left * Time.deltaTime, Space.World);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                Cube.Move();
            }
        }
    }
}
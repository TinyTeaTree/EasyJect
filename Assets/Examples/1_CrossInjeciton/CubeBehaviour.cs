using EasyJect;
using UnityEngine;

namespace Example_CrossInjection
{
    public class CubeBehaviour : EasyBehaviour
    {
        [Inject] SphereBehaviour Sphere { get; set; }

        public void Move()
        {
            transform.Translate(Vector3.right * Time.deltaTime, Space.World);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                Sphere.Move();
            }
        }
    }
}
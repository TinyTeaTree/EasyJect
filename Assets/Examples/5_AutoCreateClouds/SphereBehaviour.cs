using EasyJect;
using UnityEngine;


namespace Example_AutoCreateClouds
{
    public class SphereBehaviour : EasyBehaviour
    {
        [Inject] GeometryCloud GeometryCloud { get; set; }

        public void Move()
        {
            transform.Translate(Vector3.left * Time.deltaTime, Space.World);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                GeometryCloud.SpherePressed();
            }
        }
    }
}
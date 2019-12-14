using EasyJect;
using UnityEngine;


namespace Example_AutoCreateClouds
{
    public class CubeBehaviour : EasyBehaviour
    {
        [Inject] GeometryCloud GeometryCloud { get; set; }

        public void Move()
        {
            transform.Translate(Vector3.right * Time.deltaTime, Space.World);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                GeometryCloud.CubePressed();
            }
        }
    }
}
using EasyJect;
using UnityEngine;

namespace Example_SignalListener
{
    public class CubeBehaviour : EasyConsumer
    {
        [Inject] Invoker<CubeClickedSignal> CubeInvoker { get; set; }
        [Inject] Listener<SphereClickedSignal> SphereListener { get; set; }

        private void Start()
        {
            SphereListener.AddListener(Move);
        }

        public void Move()
        {
            transform.Translate(Vector3.right * Time.deltaTime, Space.World);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                CubeInvoker.Invoke();
            }
        }
    }
}



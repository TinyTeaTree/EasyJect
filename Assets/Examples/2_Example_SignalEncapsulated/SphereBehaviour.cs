using EasyJect;
using UnityEngine;

namespace Example_SignalListener
{
    public class SphereBehaviour : EasyConsumer
    {
        [Inject] Invoker<SphereClickedSignal> SphereInvoker { get; set; }
        [Inject] Listener<CubeClickedSignal> CubeListener { get; set; }

        private void Start()
        {
            CubeListener.AddListener(Move);
        }

        public void Move()
        {
            transform.Translate(Vector3.left * Time.deltaTime, Space.World);
        }

        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                SphereInvoker.Invoke();
            }
        }
    }
}
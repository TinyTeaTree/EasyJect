using EasyJect;
using UnityEngine;

namespace Example_CommandAutoListen
{
    public class CubeBehaviour : EasyBehaviour
    {
        [Inject] ClickAnywhereSignal ClickAnywhereSignal { get; set; }

        public void Move()
        {
            transform.Translate(Vector3.right * Time.deltaTime, Space.World);
        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(0))
            {
                ClickAnywhereSignal.Invoke();
            }
        }
    }
}
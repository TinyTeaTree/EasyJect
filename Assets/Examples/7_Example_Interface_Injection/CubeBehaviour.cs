using EasyJect;
using UnityEngine;

namespace Example_InterfaceInjection
{
    public class CubeBehaviour : EasyBehaviour, ICube
    {
        public override bool SupplyInject => false;

        void OnSelectCube()
        {
            Debug.Log("Selecting cube");
            SupplyBehaviour();
        }

        void OnMouseUpAsButton()
        {
            OnSelectCube();
        }

        public void Move()
        {
            transform.position = transform.position + Vector3.up * 0.3f;
        }
    }
}
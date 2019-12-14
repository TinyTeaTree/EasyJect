using EasyJect;
using UnityEngine;

namespace Example_ExposingMembers
{

    public class CarBehaviour : EasyBehaviour
    {
        [SerializeField] [Register] CarModel CarModel;
    }

    [System.Serializable]
    public class CarModel
    {
        [SerializeField] [Register] CarEngine CarEngine;
        public string CarId;
    }

    [System.Serializable]
    public class CarEngine
    {
        [SerializeField, Register] EngineOil EngineOil;
        public string EngineId;
    }

    [System.Serializable]
    public class EngineOil
    {
        public string OilId;
    }
}
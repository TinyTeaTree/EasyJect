using EasyJect;
using UnityEngine;
using UnityEngine.UI;

namespace Example_ExposingMembers
{
    public class UI : EasyBehaviour
    {
        [Inject] CarModel CarModel { get; set; }
        [Inject] CarEngine CarEngine { get; set; }
        [Inject] EngineOil EngineOil { get; set; }

        [SerializeField] Button CarButton, EngineButton, OilButton;
        [SerializeField] Text InfoLabel;

        private void Start()
        {
            CarButton.onClick.AddListener(() =>
            {
                InfoLabel.text = CarModel.CarId;
            });

            EngineButton.onClick.AddListener(() =>
            {
                InfoLabel.text = CarEngine.EngineId;
            });

            OilButton.onClick.AddListener(() =>
            {
                InfoLabel.text = EngineOil.OilId;
            });
        }
    }
}
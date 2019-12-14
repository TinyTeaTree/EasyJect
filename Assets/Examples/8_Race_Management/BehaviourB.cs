using EasyJect;
using UnityEngine;

namespace Scene8
{
    public class BehaviourB : EasyBehaviour
    {
        [Inject] BehaviourA BehaviourA { get; set; }
        [Inject] BehaviourB BehaviourB_myself { get; set; }
        [Inject] CloudA CloudA { get; set; }
        [Inject] CloudA CloudB { get; set; }
        [Inject] CommandA CommandA { get; set; }
        [Inject] CommandB CommandB { get; set; }

        void Start()
        {
            if (BehaviourA == null)
            {
                Debug.LogError($"Could Not Find BehaviourA in {this.GetType()}");
            }
            if (BehaviourB_myself == null)
            {
                Debug.LogError($"Could Not Find BehaviourB in {this.GetType()}");
            }
            if (CloudA == null)
            {
                Debug.LogError($"Could Not Find CloudA in {this.GetType()}");
            }
            if (CloudB == null)
            {
                Debug.LogError($"Could Not Find CloudB in {this.GetType()}");
            }
            if (CommandA == null)
            {
                Debug.LogError($"Could Not Find CommandA in {this.GetType()}");
            }
            if (CommandB == null)
            {
                Debug.LogError($"Could Not Find CommandB in {this.GetType()}");
            }
        }
    }
}
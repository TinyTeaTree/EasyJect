using EasyJect;
using UnityEngine;

namespace Scene8
{
    public class CommandA : Command
    {
        [Inject] BehaviourA BehaviourA { get; set; }
        [Inject] BehaviourB BehaviourB { get; set; }
        [Inject] CloudA CloudA { get; set; }
        [Inject] CloudA CloudB { get; set; }
        [Inject] CommandA CommandA_myself { get; set; }
        [Inject] CommandB CommandB { get; set; }

        [Start]
        void Start()
        {
            if (BehaviourA == null)
            {
                Debug.LogError($"Could Not Find BehaviourA in {this.GetType()}");
            }
            if (BehaviourB == null)
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
            if (CommandA_myself == null)
            {
                Debug.LogError($"Could Not Find CommandA in {this.GetType()}");
            }
            if (CommandB == null)
            {
                Debug.LogError($"Could Not Find CommandB in {this.GetType()}");
            }
        }

        protected override void Execute()
        {
            SetDone();
        }
    }
}
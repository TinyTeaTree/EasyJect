using EasyJect;
using System.Collections;
using UnityEngine;

namespace Example_CommandAutoListen
{
    [AutoCreate(BinderType = typeof(BasicSceneBinder))]
    public class MoveCubeCommand : Command
    {
        [Inject] CubeBehaviour Cube { get; set; }
        [Inject] ClickAnywhereSignal ClickAnywhereSignal { get; set; }

        [Start]
        void Start()
        {
            Debug.Log("MoveCubeCommand has been Start()ed");
            ClickAnywhereSignal.AddOnce(Invoke);
        }

        protected override void Execute()
        {
            Cube.StartCoroutine(MoveRoutine());
        }

        IEnumerator MoveRoutine()
        {
            float timePassed = 0;
            while(timePassed < 1f)
            {
                Cube.Move();
                yield return null;
                timePassed += Time.deltaTime;
            }
            SetDone();
            ClickAnywhereSignal.AddOnce(Invoke);
        }
    }
}

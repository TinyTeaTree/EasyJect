using EasyJect;
using System.Collections;
using UnityEngine;

namespace Example_CommandCancel
{

    public class MoveCubeCommand : Command
    {
        [Inject] CubeBehaviour Cube { get; set; }
        [Inject] SphereBehaviour Sphere { get; set; }

        bool IsEndReached { get { return Cube.transform.position.x > Sphere.transform.position.x - 1f; } }

        protected override void Execute()
        {
            if(IsEndReached)
            {
                SetCancel();
                return;
            }

            Cube.StartCoroutine(MoveRoutine());
        }

        IEnumerator MoveRoutine()
        {
            float timePassed = 0;
            while (timePassed < 0.5f)
            {
                Cube.Move();
                if (IsEndReached)
                {
                    break;
                }

                yield return null;
                timePassed += Time.deltaTime;
            }
            SetDone();
        }
    }
}
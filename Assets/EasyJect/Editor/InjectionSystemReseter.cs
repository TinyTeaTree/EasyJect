using UnityEditor;

namespace EasyJect.Internal
{
    [InitializeOnLoad]
    class InjectionSystemReseter
    {
        static InjectionSystemReseter()
        {
            EditorApplication.playModeStateChanged += StateChange;
        }

        static void StateChange(PlayModeStateChange change)
        {
            if( change == PlayModeStateChange.ExitingEditMode || change == PlayModeStateChange.ExitingPlayMode)
            {
                InjectionSystem.Reset();
            }
        }
    }
}

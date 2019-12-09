using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EasyJect.Internal
{
    public class InjectionWindow : EditorWindow
    {
        [MenuItem("Window/Injection System Diagnostic")]
        static void Init()
        {
            InjectionWindow window = (InjectionWindow)EditorWindow.GetWindow(typeof(InjectionWindow));
            window.titleContent = new GUIContent("Injection System");
            window.Show();           
        }

        void OnGUI()
        {
            var injectSystem = InjectionSystem._systemDelegate;

            DisplayCounter("Behaviours #", injectSystem.BehaviourDictionary.Count);

            DisplayCounter("Signals #", injectSystem.TypesSignalDictionary.Count);

            DisplayCounter("Clouds #", injectSystem.CloudDictionary.Count);

            DisplayCounter("Injections #", injectSystem.GetInjectionsCount);

            DisplayCounter("Singletons #", injectSystem.SingletonDictionary.Count);
        }

        private void DisplayCounter(string counterTitle, object counter)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(counterTitle);
                GUILayout.Label(counter.ToString());
            }
            GUILayout.EndHorizontal();
        }
    }
}

// If you wrap an entire file with #if UNITY_EDITOR,
// it will be completely excluded from the build and runtime — it won’t be part of the compiled code at all.
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Magenta_Framework
{
    // [InitializeOnLoad] and [InitializeOnLoadMethod] are attributes that only work in the Unity Editor and are not included in runtime builds.
    [InitializeOnLoad]
    public class InitializeOnLoadClassEx : _InitializeOnLoadClass
    {
        private static string LOG_FORMAT = "<b><color=#F1760D>[InitializeOnLoadClassEx]</color></b> {0}";

        static InitializeOnLoadClassEx()
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");

            EditorSceneManager.sceneOpened += OnSceneOpenedInEditMode;
        }

        public static new void OnSceneOpenedInEditMode(Scene scene, OpenSceneMode mode)
        {
            Debug.LogFormat(LOG_FORMAT, "OnSceneOpenedInEditMode(), scene : <color=yellow><b>" + scene.name + "</b></color>, mode : <color=yellow>" + mode + "</color>");
        }
    }
}
#endif // UNITY_EDITOR
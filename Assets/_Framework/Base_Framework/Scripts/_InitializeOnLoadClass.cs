
// If you wrap an entire file with #if UNITY_EDITOR,
// it will be completely excluded from the build and runtime — it won’t be part of the compiled code at all.
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// [InitializeOnLoad] and [InitializeOnLoadMethod] are attributes that only work in the Unity Editor and are not included in runtime builds.
// [InitializeOnLoad]
public class _InitializeOnLoadClass
{
    // private static string LOG_FORMAT = "<b><color=#F1760D>[_InitializeOnLoadClass]</color></b> {0}";
    private static string LOG_FORMAT = "<b><color=yellow>[_InitializeOnLoadClass]</color></b> {0}";

    static _InitializeOnLoadClass()
    {
        Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!");

        EditorSceneManager.sceneOpened += OnSceneOpenedInEditMode;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        // forcelly call
        Scene activeScene = EditorSceneManager.GetActiveScene();
        OnSceneOpenedInEditMode(activeScene, OpenSceneMode.Single);
        OnPlayModeStateChanged(PlayModeStateChange.EnteredEditMode);
    }

    public static void OnSceneOpenedInEditMode(Scene scene, OpenSceneMode mode)
    {
        Debug.LogWarningFormat(LOG_FORMAT, "OnSceneOpenedInEditMode(), scene : <color=yellow><b>" + scene.name + "</b></color>, mode : <color=yellow>" + mode + "</color>");
    }

    public static void OnPlayModeStateChanged(PlayModeStateChange mode)
    {
        Debug.LogWarningFormat(LOG_FORMAT, "OnPlayModeStateChanged(), mode : <color=yellow><b>" + mode + "</b></color>");

        // EnteredEditMode (Edit mode)
        // ExitingEditMode (Occurs when EXITING edit mode, before the Editor is in play mode.)
        // EnteredPlayMode (Play mode)
        // ExitingPlayMode (Occurs when EXITING play mode, before the Editor is in edit mode.)
    }
}
#endif // UNITY_EDITOR
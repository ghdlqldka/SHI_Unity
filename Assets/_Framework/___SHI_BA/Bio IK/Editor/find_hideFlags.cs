using UnityEditor;
using UnityEngine;

public class CheckHideFlags : EditorWindow
{
    [MenuItem("Tools/Check Selected HideFlags")]
    static void Init()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            Debug.Log("Please select a GameObject in the Hierarchy.");
            return;
        }
        CheckFlagsRecursively(selectedObject.transform);
    }

    static void CheckFlagsRecursively(Transform t)
    {
        Debug.Log("Object: " + t.name + ", HideFlags: " + t.gameObject.hideFlags);
        Component[] components = t.gameObject.GetComponents<Component>();
        foreach (Component comp in components)
        {
            if (comp != null)
            { // Null check for missing scripts
                Debug.Log("-- Component: " + comp.GetType().Name + ", HideFlags: " + comp.hideFlags);
            }
        }
        foreach (Transform child in t)
        {
            CheckFlagsRecursively(child);
        }
    }
}
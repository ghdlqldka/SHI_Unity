using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

// [InitializeOnLoad]
public class _TagManager
{
    private static string LOG_FORMAT = "<color=#EC00B1><b>[_TagManager]</b></color> {0}";

    static _TagManager()
    {
        CheckTags();
        CheckLayers();
    }

    public static void CheckTags()
    {
        Debug.LogFormat(LOG_FORMAT, "CheckTags()");

        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = manager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            Debug.Log("Tag " + i + "\t<color=yellow>" + t.stringValue + "</color>");
        }
    }

    public static void AddTags(string[] tagNames)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = manager.FindProperty("tags");

        List<string> DefaultTags = new List<string>() { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };

        foreach (string name in tagNames)
        {
            if (DefaultTags.Contains(name))
                continue;

            // check if tag is present
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(name))
                {
                    found = true;
                    break;
                }
            }

            // if not found, add it
            if (found == false)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                n.stringValue = name;
            }
        }

        // save
        manager.ApplyModifiedProperties();
    }

    public static void CheckSortLayers(string[] tagNames)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty sortLayersProp = manager.FindProperty("m_SortingLayers");

        foreach (string name in tagNames)
        {
            // check if tag is present
            bool found = false;
            for (int i = 0; i < sortLayersProp.arraySize; i++)
            {
                SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex(i);
                SerializedProperty t = entry.FindPropertyRelative("name");
                if (t.stringValue.Equals(name))
                {
                    found = true;
                    break;
                }
            }

            // if not found, add it
            if (found == false)
            {
                manager.ApplyModifiedProperties();
                AddSortingLayer();
                manager.Update();

                int idx = sortLayersProp.arraySize - 1;
                SerializedProperty entry = sortLayersProp.GetArrayElementAtIndex(idx);
                SerializedProperty t = entry.FindPropertyRelative("name");
                t.stringValue = name;
            }
        }

        // save
        manager.ApplyModifiedProperties();
    }

    public static void CheckLayers()
    {
        Debug.LogFormat(LOG_FORMAT, "CheckLayers()");

        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = manager.FindProperty("layers");

        for (int i = 0; i <= 31; i++)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

            if (sp != null && string.IsNullOrEmpty(sp.stringValue) == false)
            {
                Debug.Log("Layer " + i + "\t<color=yellow>" + sp.stringValue + "</color>");
            }
        }
    }

    public static void AddLayers(string[] layerNames)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = manager.FindProperty("layers");

        foreach (string name in layerNames)
        {
            // check if layer is present
            bool found = false;
            for (int i = 0; i <= 31; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                if (sp != null && name.Equals(sp.stringValue))
                {
                    found = true;
                    break;
                }
            }

            // not found, add into 1st open slot
            if (found == false)
            {
                SerializedProperty slot = null;
                for (int i = 8; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                    if (sp != null && string.IsNullOrEmpty(sp.stringValue) == true)
                    {
                        slot = sp;
                        break;
                    }
                }

                if (slot != null)
                {
                    slot.stringValue = name;
                }
                else
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "Could not find an open Layer Slot for: " + name);
                }
            }
        }

        // save
        manager.ApplyModifiedProperties();
    }

    // you need 'using System.Reflection;' for these
    private static Assembly editorAsm;
    private static MethodInfo AddSortingLayer_Method;

    /// <summary> add a new sorting layer with default name </summary>
    public static void AddSortingLayer()
    {
        if (AddSortingLayer_Method == null)
        {
            if (editorAsm == null)
                editorAsm = Assembly.GetAssembly(typeof(Editor));

            System.Type t = editorAsm.GetType("UnityEditorInternal.InternalEditorUtility");
            AddSortingLayer_Method = t.GetMethod("AddSortingLayer", (BindingFlags.Static | BindingFlags.NonPublic), null, new System.Type[0], null);
        }
        AddSortingLayer_Method.Invoke(null, null);
    }
}
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DeepMissingScriptFinder
{
    [MenuItem("Tools/Find ALL Missing Scripts in Scene (Deep Scan)")]
    static void FindAllMissingScripts()
    {
        int goCount = 0, componentsCount = 0, missingCount = 0;

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.isLoaded) continue;

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject go in rootObjects)
            {
                Traverse(go, ref goCount, ref componentsCount, ref missingCount);
            }
        }

        Debug.Log($"✅ 탐색 완료: {goCount}개의 GameObject 중 {missingCount}개의 Missing Script 발견됨.");
    }

    static void Traverse(GameObject go, ref int goCount, ref int componentsCount, ref int missingCount)
    {
        goCount++;
        Component[] components = go.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            componentsCount++;
            if (components[i] == null)
            {
                missingCount++;
                Debug.LogWarning($"⚠️ Missing Script: {GetGameObjectPath(go)}", go);
            }
        }

        foreach (Transform child in go.transform)
        {
            Traverse(child.gameObject, ref goCount, ref componentsCount, ref missingCount);
        }
    }

    static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }
        return path;
    }
}
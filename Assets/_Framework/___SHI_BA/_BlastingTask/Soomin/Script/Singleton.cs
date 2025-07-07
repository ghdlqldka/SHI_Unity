using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                T[] objs = FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                T[] objs = FindObjectsOfType<T>(includeInactive: false);
#endif

                if (objs.Length > 0)
                    _instance = objs[0];

                if (objs.Length > 1)
                {
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }

                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    obj.name = typeof(T).Name + " (Singleton)";
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}
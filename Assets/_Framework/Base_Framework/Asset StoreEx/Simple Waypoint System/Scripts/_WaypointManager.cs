/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SWS
{
    using AYellowpaper.SerializedCollections;
    using DG.Tweening;
    using Unity.VisualScripting;

    /// <summary>
    /// The editor part of this class allows you to create paths in 2D or 3D space.
    /// At runtime, it manages path instances for easy lookup of references.
    /// <summary>
    public class _WaypointManager : WaypointManager
    {
        private static string LOG_FORMAT = "<color=#5E92FF><b>[_WaypointManager]</b></color> {0}";

        protected static _WaypointManager _instance;
        public static _WaypointManager Instance
        {
            get
            { 
                return _instance;
            }
            protected set 
            { 
                _instance = value;
            }
        }

        // public static readonly Dictionary<string, PathManager> Paths = new Dictionary<string, PathManager>();
        [ReadOnly]
        [SerializeField]
        [SerializedDictionary("Path Name", "Path")]
        protected SerializedDictionary<string, PathManager> pathDic = new SerializedDictionary<string, PathManager>();
        public SerializedDictionary<string, PathManager> PathDic
        {
            get 
            { 
                return pathDic;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                Paths = null;

                //http://dotween.demigiant.com/documentation.php#init
                //initialize DOTween immediately instead than having it being
                //automatically initialized when the first Tweener is created.
                //set up specific settings in the DOTween utility panel!
                DOTween.Init();
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            // Paths.Clear();
            pathDic.Clear();
            Instance = null;
        }

        public static new void AddPath(GameObject path)
        {
            WaypointManager.AddPath(path);
            throw new System.NotSupportedException("");
        }

        public void _AddPath(GameObject path)
        {
            //check if the path has been instantiated,
            //then remove the clone naming scheme
            string pathName = path.name;
            if (pathName.Contains("(Clone)"))
            {
                pathName = pathName.Replace("(Clone)", "");
            }

            //try to get PathManager component
            PathManager pathMan = path.GetComponentInChildren<PathManager>();
            if (pathMan == null)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Called AddPath() but GameObject " + pathName + " has no PathManager attached.");
                return;
            }

            _CleanUp();

            //check if our dictionary already contains this path
            //in case it exists already we add a unique number to the end
            // if (Paths.ContainsKey(pathName))
            if (pathDic.ContainsKey(pathName) == true)
            {
                //find unique naming for it
                int i = 1;
                // while (Paths.ContainsKey(pathName + "#" + i))
                while (pathDic.ContainsKey(pathName + "#" + i))
                {
                    i++;
                }

                pathName += "#" + i;
                Debug.LogFormat(LOG_FORMAT, "Renamed " + path.name + " to " + pathName + " because a path with the same name was found.");
            }

            //rename path and add it to dictionary
            path.name = pathName;
            // Paths.Add(pathName, pathMan);
            pathDic.Add(pathName, pathMan);
        }

        public static new void CleanUp()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void _CleanUp()
        {
#if false //
            // string[] keys = Paths.Where(p => p.Value == null).Select(p => p.Key).ToArray();
#else
            List<string> keyList = new List<string>();

            // foreach (KeyValuePair<string, PathManager> p in Paths)
            foreach (KeyValuePair<string, PathManager> p in pathDic)
            {
                if (p.Value == null)
                {
                    keyList.Add(p.Key);
                }
            }

            string[] keys = keyList.ToArray();
#endif
            for (int i = 0; i < keys.Length; i++)
            {
                // Paths.Remove(keys[i]);
                pathDic.Remove(keys[i]);
            }
        }
    }

}
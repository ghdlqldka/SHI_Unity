/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;

namespace SWS
{
    using DG.Tweening;

    /// <summary>
    /// Example: demonstrates the programmatic use at runtime.
    /// <summary>
    public class _RuntimeDemo : RuntimeDemo
    {
        private static string LOG_FORMAT = "<color=#ABDB33><b>[_RuntimeDemo]</b></color> {0}";

        protected override void OnGUI()
        {
            DrawExample1();
            DrawExample2();
            DrawExample3();
            DrawExample4();
            DrawExample5();
            DrawExample6();
            DrawExample7();
        }

        protected override void DrawExample1()
        {
            GUI.Label(new Rect(10, 10, 20, 20), "1:");

            string walkerName = "Walker (Path1)";
            Vector3 pos = new Vector3(-25, 0, 10);

            //instantiation
            if (example1.done == false && GUI.Button(new Rect(30, 10, 100, 20), "Instantiate"))
            {
                //instantiate walker prefab
                GameObject walker = (GameObject)Instantiate(example1.walkerPrefab, pos, Quaternion.identity);
                walker.name = walkerName;

                //instantiate path prefab
                GameObject path = (GameObject)Instantiate(example1.pathPrefab, pos, Quaternion.identity);

                //start movement on the new path
                Debug.LogFormat(LOG_FORMAT, "path.name : " + path.name);
                walker.GetComponent<_SplineMove>().SetPath(_WaypointManager.Instance.PathDic[path.name]);

                //example only
                example1.done = true;
            }

            //destruction
            if (example1.done && GUI.Button(new Rect(30, 10, 100, 20), "Destroy"))
            {
                Destroy(GameObject.Find(walkerName));
                Destroy(GameObject.Find(example1.pathPrefab.name));

                //example only
                example1.done = false;
            }
        }

        protected override void DrawExample2()
        {
            GUI.Label(new Rect(10, 30, 20, 20), "2:");

            //change path from path1 to path2 or vice versa
            if (GUI.Button(new Rect(30, 30, 100, 20), "Switch Path"))
            {
                //get current path name
                string currentPath = example2.moveRef.pathContainer.name;
                //toggle movement to the path on the movement script
                example2.moveRef.moveToPath = true;

                //switch paths based on the name,
                //actual path switching by calling SetPath(...)
                if (currentPath == example2.pathName1)
                    example2.moveRef.SetPath(_WaypointManager.Instance.PathDic[example2.pathName2]);
                else
                    example2.moveRef.SetPath(_WaypointManager.Instance.PathDic[example2.pathName1]);
            }
        }
    }
}
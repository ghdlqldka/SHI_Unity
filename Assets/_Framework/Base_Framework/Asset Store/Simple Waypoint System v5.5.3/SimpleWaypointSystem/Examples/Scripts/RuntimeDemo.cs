﻿/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
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
    public class RuntimeDemo : MonoBehaviour
    {
        /// <summary>
        /// Instantiation example variables.
        /// <summary>
        public ExampleClass1 example1;

        /// <summary>
        /// Switch path example variables.
        /// <summary>
        public ExampleClass2 example2;

        /// <summary>
        /// Start, stop, reset example variables.
        /// <summary>
        public ExampleClass3 example3;

        /// <summary>
        /// Pause, resume example variables.
        /// <summary>
        public ExampleClass4 example4;

        /// <summary>
        /// Change speed example variables.
        /// <summary>
        public ExampleClass5 example5;

        /// <summary>
        /// Add message example variables.
        /// <summary>
        public ExampleClass6 example6;

        /// <summary>
        /// Path creation example variables.
        /// <summary>
        public ExampleClass6 example7;


        //draw buttons for each example
        protected virtual void OnGUI()
        {
            DrawExample1();
            DrawExample2();
            DrawExample3();
            DrawExample4();
            DrawExample5();
            DrawExample6();
            DrawExample7();
        }


        //Example 1: Path & Walker Instantiation
        protected virtual void DrawExample1()
        {
            GUI.Label(new Rect(10, 10, 20, 20), "1:");

            string walkerName = "Walker (Path1)";
            Vector3 pos = new Vector3(-25, 0, 10);

            //instantiation
            if (!example1.done && GUI.Button(new Rect(30, 10, 100, 20), "Instantiate"))
            {
                //instantiate walker prefab
                GameObject walker = (GameObject)Instantiate(example1.walkerPrefab, pos, Quaternion.identity);
                walker.name = walkerName;

                //instantiate path prefab
                GameObject path = (GameObject)Instantiate(example1.pathPrefab, pos, Quaternion.identity);

                //start movement on the new path
                walker.GetComponent<splineMove>().SetPath(WaypointManager.Paths[path.name]);

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


        //Example 2: Switch Path
        protected virtual void DrawExample2()
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
                    example2.moveRef.SetPath(WaypointManager.Paths[example2.pathName2]);
                else
                    example2.moveRef.SetPath(WaypointManager.Paths[example2.pathName1]);
            }
        }


        //Example 3: Start, Stop, Reset
        protected void DrawExample3()
        {
            GUI.Label(new Rect(10, 50, 20, 20), "3:");

            if (example3.moveRef.tween == null && GUI.Button(new Rect(30, 50, 100, 20), "Start"))
            {
                example3.moveRef.StartMove();
            }

            if (example3.moveRef.tween != null && GUI.Button(new Rect(30, 50, 100, 20), "Stop"))
            {
                example3.moveRef.Stop();
            }

            if (example3.moveRef.tween != null && GUI.Button(new Rect(30, 70, 100, 20), "Reset"))
            {
                example3.moveRef.ResetToStart();
            }
        }


        //Example 4: Pause, Resume
        protected void DrawExample4()
        {
            GUI.Label(new Rect(10, 90, 20, 20), "4:");

            if (example4.moveRef.tween != null && example4.moveRef.tween.IsPlaying()
                && GUI.Button(new Rect(30, 90, 100, 20), "Pause"))
            {
                example4.moveRef.Pause();
            }

            if (example4.moveRef.tween != null && !example4.moveRef.tween.IsPlaying()
                && GUI.Button(new Rect(30, 90, 100, 20), "Resume"))
            {
                example4.moveRef.Resume();
            }
        }


        //Example 5: Change Speed
        protected void DrawExample5()
        {
            GUI.Label(new Rect(10, 110, 20, 20), "5:");

            if (GUI.Button(new Rect(30, 110, 100, 20), "Change Speed"))
            {
                //get current speed and increase/decrease new speed
                float currentSpeed = example5.moveRef.speed;
                float newSpeed = 1.5f;
                if (currentSpeed == newSpeed) newSpeed = 4f;

                example5.moveRef.ChangeSpeed(newSpeed);
            }
        }


        //Example 6: Adding messages
        protected void DrawExample6()
        {
            GUI.Label(new Rect(10, 130, 20, 20), "6:");

            if (!example6.done && GUI.Button(new Rect(30, 130, 100, 20), "Add Event"))
            {
                //subscribe to event at the path end to call our method
                example6.moveRef.movementEndEvent += EventListener;
                //example only
                example6.done = true;
            }
        }

        protected void EventListener()
        {
            //get receiving script
            EventReceiver receiver = example6.moveRef.GetComponent<EventReceiver>();
            receiver.ActivateForTime(example6.target);
        }


        protected void DrawExample7()
        {
            GUI.Label(new Rect(10, 150, 20, 20), "7:");

            if (!example7.done && GUI.Button(new Rect(30, 150, 100, 20), "Create Path"))
            {
                //create path manager game object
                GameObject newPath = new GameObject("Path7 (Runtime Creation)");
                PathManager path = newPath.AddComponent<PathManager>();

                //declare waypoint positions
                Vector3[] positions = new Vector3[] { new Vector3(-25, 0, -20), new Vector3(-15, 3, -20), new Vector3(-4, 0, -20) };
                Transform[] waypoints = new Transform[positions.Length];

                //instantiate waypoints
                for (int i = 0; i < positions.Length; i++)
                {
                    GameObject newPoint = new GameObject("Waypoint " + i);
                    waypoints[i] = newPoint.transform;
                    waypoints[i].position = positions[i];
                }

                //assign waypoints to path
                path.Create(waypoints, true);

                //optional for visibility in the build
                newPath.AddComponent<PathRenderer>();
                newPath.GetComponent<LineRenderer>().material = new Material(Shader.Find("Sprites/Default"));

                //example only
                example7.done = true;
            }
        }


        [System.Serializable]
        public class ExampleClass1
        {
            public GameObject walkerPrefab;
            public GameObject pathPrefab;
            public bool done = false;
        }

        [System.Serializable]
        public class ExampleClass2
        {
            public splineMove moveRef;
            public string pathName1;
            public string pathName2;
        }

        [System.Serializable]
        public class ExampleClass3
        {
            public splineMove moveRef;
        }

        [System.Serializable]
        public class ExampleClass4
        {
            public splineMove moveRef;
        }

        [System.Serializable]
        public class ExampleClass5
        {
            public splineMove moveRef;
        }

        [System.Serializable]
        public class ExampleClass6
        {
            public splineMove moveRef;
            public GameObject target;
            public bool done = false;
        }

        [System.Serializable]
        public class ExampleClass7
        {
            public bool done = false;
        }
    }
}
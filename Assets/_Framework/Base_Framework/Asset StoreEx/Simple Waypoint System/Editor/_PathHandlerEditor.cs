/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace SWS
{
    /// <summary>
    /// Custom path inspector.
    /// <summary>
    [CustomEditor(typeof(_PathHandler))]
    public class _PathHandlerEditor : PathEditor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty drawCurved;
        protected SerializedProperty drawDirection;

        public override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // base.OnEnable();
            //we create a reference to our script object by passing in the target
            m_Object = new SerializedObject(target);

            //from this object, we pull out the properties we want to use
            //these are just the names of our variables in the manager
            // m_Check1 = m_Object.FindProperty("drawCurved");
            // m_Check2 = m_Object.FindProperty("drawDirection");
            drawCurved = m_Object.FindProperty("drawCurved");
            drawDirection = m_Object.FindProperty("drawDirection");
            m_Color1 = m_Object.FindProperty("color1");
            m_Color2 = m_Object.FindProperty("color2");
            m_SkipNames = m_Object.FindProperty("skipCustomNames");
            m_WaypointPref = m_Object.FindProperty("replaceObject");

            //set serialized waypoint array count by passing in the path to our array size
            m_WaypointsCount = m_Object.FindProperty(wpArraySize);
            //reset selected waypoints
            isControlPressed = false;
            activeNode.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspectorGUI();
            //this pulls the relative variables from unity runtime and stores them in the object
            m_Object.Update();

            //get waypoint array
            var waypoints = GetWaypointArray();

            //don't draw inspector fields if the path contains less than 2 points
            //(a path with less than 2 points really isn't a path)
            if (m_WaypointsCount.intValue < 2)
            {
                //button to create path manually
                if (GUILayout.Button("Create Path from Children"))
                {
                    Undo.RecordObjects(waypoints, "Create Path");
                    (m_Object.targetObject as PathManager).Create();
                    SceneView.RepaintAll();
                }

                return;
            }

            //create new checkboxes for path gizmo property 
            // m_Check1.boolValue = EditorGUILayout.Toggle("Draw Smooth Lines", m_Check1.boolValue);
            // m_Check2.boolValue = EditorGUILayout.Toggle("Draw Direction", m_Check2.boolValue);
            drawCurved.boolValue = EditorGUILayout.Toggle("Draw Smooth Lines", drawCurved.boolValue);
            drawDirection.boolValue = EditorGUILayout.Toggle("Draw Direction", drawDirection.boolValue);

            //create new property fields for editing waypoint gizmo colors
            EditorGUILayout.PropertyField(m_Color1);
            EditorGUILayout.PropertyField(m_Color2);

            //calculate path length of all waypoints
            Vector3[] wpPositions = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
                wpPositions[i] = waypoints[i].position;

            float pathLength = WaypointManager.GetPathLength(wpPositions);
            //path length label, show calculated path length
            GUILayout.Label("Path Length: " + pathLength);

            //button for switching over to the WaypointManager for further path editing
            if (GUILayout.Button("Continue Editing"))
            {
                Selection.activeGameObject = FindAnyObjectByType<WaypointManager>().gameObject;
                WaypointEditor.ContinuePath(m_Object.targetObject as PathManager);
            }

            //more path modifiers
            DrawPathOptions();
            EditorGUILayout.Space();

            //waypoint index header
            GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);

            //loop through the waypoint array
            for (int i = 0; i < waypoints.Length; i++)
            {
                GUILayout.BeginHorizontal();
                //indicate each array slot with index number in front of it
                GUILayout.Label(i + ".", GUILayout.Width(20));
                //create an object field for every waypoint
                EditorGUILayout.ObjectField(waypoints[i], typeof(Transform), true);

                //display an "Add Waypoint" button for every array row except the last one
                if (i < waypoints.Length && GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    AddWaypointAtIndex(i);
                    break;
                }

                //display an "Remove Waypoint" button for every array row except the first and last one
                if (i > 0 && i < waypoints.Length - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
                {
                    RemoveWaypointAtIndex(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            //we push our modified variables back to our serialized object
            m_Object.ApplyModifiedProperties();
        }

        protected override void OnSceneGUI()
        {
            //again, get waypoint array
            var waypoints = GetWaypointArray();
            //do not execute further code if we have no waypoints defined
            //(just to make sure, practically this can not occur)
            if (waypoints.Length == 0)
                return;
            Vector3 wpPos = Vector3.zero;
            float size = 1f;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.modifiers == EventModifiers.Control)
                isControlPressed = true;

            //loop through waypoint array
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (!waypoints[i]) 
                    continue;
                wpPos = waypoints[i].position;

                size = HandleUtility.GetHandleSize(wpPos) * 0.4f;

                //do not draw waypoint header if too far away
                if (size < 3f)
                {
                    //begin 2D GUI block
                    Handles.BeginGUI();
                    //translate waypoint vector3 position in world space into a position on the screen
                    var guiPoint = HandleUtility.WorldToGUIPoint(wpPos);
                    //create rectangle with that positions and do some offset
                    var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
                    //draw box at position with current waypoint name
                    GUI.Box(rect, waypoints[i].name);
                    Handles.EndGUI(); //end GUI block
                }

                //draw handles per waypoint, clamp size
                Handles.color = m_Color2.colorValue;
                size = Mathf.Clamp(size, 0, 1.2f);

                Handles.FreeMoveHandle(wpPos, size, Vector3.zero, (controlID, position, rotation, hSize, eventType) =>
                {
                    Handles.SphereHandleCap(controlID, position, rotation, hSize, eventType);
                    if (Event.current.type == EventType.Layout && GUIUtility.hotControl != 0 && controlID == GUIUtility.hotControl)
                    {
                        //do not execute multiple times per layout call
                        if (EditorApplication.timeSinceStartup - lastMouseClickTime < 0.5f)
                            return;

                        if (isControlPressed)
                        {
                            //toggle multi selection click
                            if (activeNode.Contains(i))
                                activeNode.Remove(i);
                            else
                                activeNode.Add(i);
                        }
                        else
                        {
                            //single selection click
                            activeNode.Clear();
                            if (!activeNode.Contains(i))
                                activeNode.Add(i);
                        }

                        lastMouseClickTime = EditorApplication.timeSinceStartup;
                        isControlPressed = false;
                    }
                });
                Handles.RadiusHandle(waypoints[i].rotation, wpPos, size / 2);
            }

            for (int i = 0; i < activeNode.Count; i++)
            {
                wpPos = waypoints[activeNode[i]].position;
                Quaternion wpRot = waypoints[activeNode[i]].rotation;
                switch (Tools.current)
                {
                    case Tool.Move:
                        if (Tools.pivotRotation == PivotRotation.Global)
                            wpRot = Quaternion.identity;

                        Vector3 newPos = Handles.PositionHandle(wpPos, wpRot);
                        Vector3 offset = newPos - wpPos;

                        if (offset != Vector3.zero)
                        {
                            Undo.RecordObjects(waypoints, "Move Handle");
                            //Undo.RecordObject(waypoints[activeNode], "Move Handle");

                            for (int j = 0; j < activeNode.Count; j++)
                                waypoints[activeNode[j]].position += offset;
                        }
                        break;

                    case Tool.Rotate:
                        Quaternion newRot = Handles.RotationHandle(wpRot, wpPos);

                        if (wpRot != newRot)
                        {
                            Undo.RecordObject(waypoints[activeNode[i]], "Rotate Handle");
                            waypoints[activeNode[i]].rotation = newRot;
                        }
                        break;
                }
            }

            //waypoint direction handles drawing
            // if (!m_Check2.boolValue) 
            if (drawDirection.boolValue == false)
                return;
            Vector3[] pathPoints = new Vector3[waypoints.Length];
            for (int i = 0; i < pathPoints.Length; i++)
                pathPoints[i] = waypoints[i].position;

            //create list of path segments (list of Vector3 list)
            List<List<Vector3>> segments = new List<List<Vector3>>();
            int curIndex = 0;
            float lerpVal = 0f;

            //differ between linear and curved display
            // switch (m_Check1.boolValue)
            switch (drawCurved.boolValue)
            {
                case true:
                    //convert waypoints to curved path points
                    pathPoints = WaypointManager.GetCurved(pathPoints);
                    //calculate approximate path point amount per segment
                    int detail = Mathf.FloorToInt((pathPoints.Length - 1f) / (waypoints.Length - 1f));

                    for (int i = 0; i < waypoints.Length - 1; i++)
                    {
                        float dist = Mathf.Infinity;
                        //loop over path points to find single segments
                        segments.Add(new List<Vector3>());

                        //we are not checking for absolute path points on standard paths, because 
                        //path points could also be located before or after waypoint positions.
                        //instead a minimum distance is searched which marks the nearest path point
                        for (int j = curIndex; j < pathPoints.Length; j++)
                        {
                            //add path point to current segment
                            segments[i].Add(pathPoints[j]);

                            //start looking for distance after a certain amount of path points of this segment
                            if (j >= (i + 1) * detail)
                            {
                                //calculate distance of current path point to waypoint
                                float pointDist = Vector3.Distance(waypoints[i].position, pathPoints[j]);
                                //we are getting closer to the waypoint
                                if (pointDist < dist)
                                    dist = pointDist;
                                else
                                {
                                    //current path point is more far away than the last one
                                    //the segment ends here, continue with new segment
                                    curIndex = j + 1;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case false:
                    //detail for arrows between waypoints
                    int lerpMax = 16;
                    //loop over waypoints to add intermediary points
                    for (int i = 0; i < waypoints.Length - 1; i++)
                    {
                        segments.Add(new List<Vector3>());
                        for (int j = 0; j < lerpMax; j++)
                        {
                            //linear lerp between waypoints to get additional points for drawing arrows at
                            segments[i].Add(Vector3.Lerp(pathPoints[i], pathPoints[i + 1], j / (float)lerpMax));
                        }
                    }
                    break;
            }

            //loop over segments
            for (int i = 0; i < segments.Count; i++)
            {
                //loop over single positions on the segment
                for (int j = 0; j < segments[i].Count; j++)
                {
                    //get current lerp value for interpolating rotation
                    //draw arrow handle on current position with interpolated rotation
                    size = Mathf.Clamp(HandleUtility.GetHandleSize(segments[i][j]) * 0.4f, 0, 1.2f);
                    lerpVal = j / (float)segments[i].Count;
                    Handles.ArrowHandleCap(0, segments[i][j], Quaternion.Lerp(waypoints[i].rotation, waypoints[i + 1].rotation, lerpVal), size, EventType.Repaint);
                }
            }
        }
    }
}
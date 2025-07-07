/*  This file is part of the "Simple Waypoint System" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SWS
{
    /// <summary>
    /// Custom bezier path inspector.
    /// <summary>
    [CustomEditor(typeof(_BezierPathHandler))]
    public class _BezierPathHandlerEditor : BezierPathEditor
    {
        protected SerializedProperty m_Script;

        // protected BezierPathManager script;
        protected _BezierPathHandler _script
        {
            get
            {
                return script as _BezierPathHandler;
            }
        }

        public override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            //we create a reference to our script object by passing in the target
            script = (_BezierPathHandler)target;
            if (_script.bPoints.Count == 0)
                return;

            //reposition handles of the first and last point to the waypoint
            //they only have one control point so we set the other one to zero
            BezierPoint first = _script.bPoints[0];
            first.cp[0].position = first.wp.position;
            BezierPoint last = _script.bPoints[_script.bPoints.Count - 1];
            last.cp[1].position = last.wp.position;

            //recalculate path points
            _script.CalculatePath();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            //don't draw inspector fields if the path contains less than 2 points
            //(a path with less than 2 points really isn't a path)
            if (_script.bPoints.Count < 2)
            {
                //button to create path manually
                if (GUILayout.Button("Create Path from Children"))
                {
                    Undo.RecordObject(_script, "Create Path");
                    _script.Create();
                    SceneView.RepaintAll();
                }

                return;
            }

            //create new checkboxes for path gizmo property 
            _script.showHandles = EditorGUILayout.Toggle("Show Handles", _script.showHandles);
            _script.connectHandles = EditorGUILayout.Toggle("Connect Handles", _script.connectHandles);
            _script.drawCurved = EditorGUILayout.Toggle("Draw Smooth Lines", _script.drawCurved);
            _script.drawDirection = EditorGUILayout.Toggle("Draw Direction", _script.drawDirection);

            //create new color fields for editing path gizmo colors 
            _script.color1 = EditorGUILayout.ColorField("Color1", _script.color1);
            _script.color2 = EditorGUILayout.ColorField("Color2", _script.color2);
            _script.color3 = EditorGUILayout.ColorField("Color3", _script.color3);

            //calculate path length of all waypoints
            float pathLength = WaypointManager.GetPathLength(_script.pathPoints);
            GUILayout.Label("Path Length: " + pathLength);

            float thisDetail = _script.pathDetail;
            //slider to modify the smoothing factor of the final path,
            //round because of path point imprecision placement (micro loops)
            _script.pathDetail = EditorGUILayout.Slider("Path Detail", _script.pathDetail, 0.5f, 10);
            _script.pathDetail = Mathf.Round(_script.pathDetail * 10f) / 10f;
            //toggle custom detail when modifying the whole path
            if (thisDetail != _script.pathDetail)
                _script.customDetail = false;
            //draw custom detail settings
            DetailSettings();

            //button for switching over to the WaypointManager for further path editing
            if (GUILayout.Button("Continue Editing"))
            {
                Selection.activeGameObject = FindAnyObjectByType<_WaypointManager>().gameObject;
                WaypointEditor.ContinuePath(_script);
            }

            //more path modifiers
            DrawPathOptions();
            EditorGUILayout.Space();

            //waypoint index header
            GUILayout.Label("Waypoints: ", EditorStyles.boldLabel);

            //loop through the waypoint array
            for (int i = 0; i < _script.bPoints.Count; i++)
            {
                GUILayout.BeginHorizontal();
                //indicate each array slot with index number in front of it
                GUILayout.Label(i + ".", GUILayout.Width(20));

                //create an object field for every waypoint
                EditorGUILayout.ObjectField(_script.bPoints[i].wp, typeof(Transform), true);

                //display an "Add Waypoint" button for every array row except the last one
                //on click we call AddWaypointAtIndex() to insert a new waypoint slot AFTER the selected slot
                if (i < _script.bPoints.Count && GUILayout.Button("+", GUILayout.Width(30f)))
                {
                    AddWaypointAtIndex(i);
                    break;
                }

                //display an "Remove Waypoint" button for every array row except the first and last one
                //on click we call RemoveWaypointAtIndex() to delete the selected waypoint slot
                if (i > 0 && i < _script.bPoints.Count - 1 && GUILayout.Button("-", GUILayout.Width(30f)))
                {
                    RemoveWaypointAtIndex(i);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            //recalculate on inspector changes
            if (GUI.changed)
            {
                _script.CalculatePath();
                EditorUtility.SetDirty(target);
            }
        }
    }
}
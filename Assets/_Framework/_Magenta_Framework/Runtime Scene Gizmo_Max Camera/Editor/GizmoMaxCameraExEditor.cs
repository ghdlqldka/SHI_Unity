using _Base_Framework;
using UnityEditor;
using UnityEngine;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(GizmoMaxCameraEx))]
    public class GizmoMaxCameraExEditor : _Base_Framework._GizmoMaxCameraEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            _maxCamera = target as GizmoMaxCameraEx;
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(targetOffset);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(xSpeed);
            EditorGUILayout.PropertyField(ySpeed);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(yMinLimit);
            EditorGUILayout.PropertyField(yMaxLimit);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(zoomRate, new GUIContent("Zoom Speed"));
            EditorGUILayout.PropertyField(zoomDampening);
            if (Application.isPlaying == false)
            {
                // EditorGUILayout.PropertyField(distance, new GUIContent("Init Distance"));
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(distance, new GUIContent("Init Distance"));
                EditorGUILayout.LabelField("desiredDistance : " + ((GizmoMaxCameraEx)_maxCamera).desiredDistance);
                EditorGUILayout.LabelField("_currentDistance : " + ((GizmoMaxCameraEx)_maxCamera)._currentDistance);
                EditorGUILayout.Space();
                GUI.enabled = true;
            }
            EditorGUILayout.PropertyField(maxDistance);
            EditorGUILayout.PropertyField(minDistance);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(panSpeed);

            // EditorGUILayout.Space();
            // EditorGUILayout.PropertyField(layerMask);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_camTargetTransform);
            EditorGUILayout.PropertyField(orgCamTargetPosition);

            /*
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=====> Extra <=====", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(plane);
            */

#if DEBUG
            EditorGUILayout.PropertyField(show_DEBUG_Sphere);
            EditorGUILayout.PropertyField(show_DEBUG_Plane);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(DEBUG_xDeg);
            EditorGUILayout.PropertyField(DEBUG_yDeg);
#endif

            serializedObject.ApplyModifiedProperties();
        }
    }
}
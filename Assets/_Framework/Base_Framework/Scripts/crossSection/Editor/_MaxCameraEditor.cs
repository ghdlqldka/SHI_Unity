using UnityEditor;
using UnityEngine;

namespace _Base_Framework
{
    [CustomEditor(typeof(_MaxCamera))]
    public class _MaxCameraEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _target;
        protected SerializedProperty targetOffset;

        protected SerializedProperty distance;
        protected SerializedProperty maxDistance;
        protected SerializedProperty minDistance;

        protected SerializedProperty xSpeed;
        protected SerializedProperty ySpeed;

        protected SerializedProperty yMinLimit;
        protected SerializedProperty yMaxLimit;

        protected SerializedProperty zoomRate;
        protected SerializedProperty panSpeed;
        protected SerializedProperty zoomDampening;

        // protected SerializedProperty layerMask;

        protected SerializedProperty plane;

        protected SerializedProperty _camTargetTransform;
        protected SerializedProperty orgCamTargetPosition;

#if DEBUG
        protected SerializedProperty show_DEBUG_Sphere;
        protected SerializedProperty show_DEBUG_Plane;

        protected SerializedProperty DEBUG_xDeg;
        protected SerializedProperty DEBUG_yDeg;
#endif

        protected _MaxCamera _maxCamera;

        protected virtual void OnEnable()
        {
            _maxCamera = target as _MaxCamera;

            m_Script = serializedObject.FindProperty("m_Script");

            _target = serializedObject.FindProperty("target");
            targetOffset = serializedObject.FindProperty("targetOffset");

            distance = serializedObject.FindProperty("distance");
            maxDistance = serializedObject.FindProperty("maxDistance");
            minDistance = serializedObject.FindProperty("minDistance");

            xSpeed = serializedObject.FindProperty("xSpeed");
            ySpeed = serializedObject.FindProperty("ySpeed");

            yMinLimit = serializedObject.FindProperty("yMinLimit");
            yMaxLimit = serializedObject.FindProperty("yMaxLimit");

            zoomRate = serializedObject.FindProperty("zoomRate");
            panSpeed = serializedObject.FindProperty("panSpeed");
            zoomDampening = serializedObject.FindProperty("zoomDampening");

            // layerMask = serializedObject.FindProperty("layerMask");

            plane = serializedObject.FindProperty("plane");

            _camTargetTransform = serializedObject.FindProperty("_camTargetTransform");
            orgCamTargetPosition = serializedObject.FindProperty("orgCamTargetPosition");

#if DEBUG
            show_DEBUG_Sphere = serializedObject.FindProperty("show_DEBUG_Sphere");
            show_DEBUG_Plane = serializedObject.FindProperty("show_DEBUG_Plane");

            DEBUG_xDeg = serializedObject.FindProperty("DEBUG_xDeg");
            DEBUG_yDeg = serializedObject.FindProperty("DEBUG_yDeg");
#endif
        }

#if true //
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
                EditorGUILayout.LabelField("desiredDistance : " + _maxCamera.desiredDistance);
                EditorGUILayout.LabelField("_currentDistance : " + _maxCamera._currentDistance);
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=====> Extra <=====", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(plane);

#if DEBUG
            EditorGUILayout.PropertyField(show_DEBUG_Sphere);
            EditorGUILayout.PropertyField(show_DEBUG_Plane);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(DEBUG_xDeg);
            EditorGUILayout.PropertyField(DEBUG_yDeg);
#endif

            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}
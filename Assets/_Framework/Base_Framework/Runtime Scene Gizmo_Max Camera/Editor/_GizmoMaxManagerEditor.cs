using RuntimeSceneGizmo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    [CustomEditor(typeof(_GizmoMaxManager))]
    public class _GizmoMaxManagerEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _camParentTransform;
        protected SerializedProperty _camera;

        // protected SerializedProperty _gizmoListener;
        protected SerializedProperty _maxCamera;
        protected SerializedProperty _gizmoRenderer;
        
        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _camParentTransform = serializedObject.FindProperty("_camParentTransform");
            _camera = serializedObject.FindProperty("_camera");

            // _gizmoListener = serializedObject.FindProperty("_gizmoListener");
            _maxCamera = serializedObject.FindProperty("_maxCamera");
            _gizmoRenderer = serializedObject.FindProperty("_gizmoRenderer");
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

            EditorGUILayout.PropertyField(_camParentTransform);
            EditorGUILayout.PropertyField(_camera);

            EditorGUILayout.Space();
            // EditorGUILayout.PropertyField(_gizmoListener);
            EditorGUILayout.PropertyField(_maxCamera);
            EditorGUILayout.PropertyField(_gizmoRenderer);

            serializedObject.ApplyModifiedProperties();
        }
#endif
    }
}
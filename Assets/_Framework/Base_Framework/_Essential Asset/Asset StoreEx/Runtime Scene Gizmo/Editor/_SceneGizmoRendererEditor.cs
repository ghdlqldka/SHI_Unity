using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeSceneGizmo
{
    [CustomEditor(typeof(_SceneGizmoRenderer))]
    public class _SceneGizmoRendererEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty imageHolder;
        // protected SerializedProperty controller;
        protected SerializedProperty gizmoPrefab;

        protected SerializedProperty highlightHoveredComponents;
        protected SerializedProperty m_referenceTransform;

        // protected SerializedProperty m_onComponentClicked;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            imageHolder = serializedObject.FindProperty("imageHolder");
            // controller = serializedObject.FindProperty("controller");
            gizmoPrefab = serializedObject.FindProperty("gizmoPrefab");
            highlightHoveredComponents = serializedObject.FindProperty("highlightHoveredComponents");
            m_referenceTransform = serializedObject.FindProperty("m_referenceTransform");

            // m_onComponentClicked = serializedObject.FindProperty("m_onComponentClicked");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(imageHolder);
            // EditorGUILayout.PropertyField(controller);
            EditorGUILayout.PropertyField(gizmoPrefab);
            EditorGUILayout.PropertyField(highlightHoveredComponents);
            EditorGUILayout.PropertyField(m_referenceTransform);

            // EditorGUILayout.Space();
            // EditorGUILayout.PropertyField(m_onComponentClicked);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
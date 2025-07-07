using UnityEditor;
using EA.Editor;
using UnityEngine;
using System;

namespace EA.Line3D.Editor
{
    [CustomEditor(typeof(_LineRenderer3D))]
    public class _LineRenderer3DEditor : LineRenderer3DEditor
    {
        protected SerializedProperty m_Script;

        // protected LineRenderer3D line;
        protected _LineRenderer3D _line
        {
            get
            {
                return line as _LineRenderer3D;
            }
        }

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        public override void OnInspectorGUI()
        {
            line = target as _LineRenderer3D;

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            var circlePoints = serializedObject.FindProperty("circlePoints");
            var capSmoothPoints = serializedObject.FindProperty("capSmoothPoints");
            var turnSmoothPoints = serializedObject.FindProperty("turnSmoothPoints");
            var sideVectorRotation = serializedObject.FindProperty("sideVectorRotation");
            var width = serializedObject.FindProperty("width");
            var updateNormals = serializedObject.FindProperty("updateNormals");
            var loop = serializedObject.FindProperty("loop");

            var points = serializedObject.FindProperty("_points");

            var renderer = serializedObject.FindProperty("_meshRenderer");
            var vertexColor = serializedObject.FindProperty("vertexColor");
            var scaleUV = serializedObject.FindProperty("scaleUV");

            if (editorMeshRenderer == null)
            {
                editorMeshRenderer = CreateEditor(renderer.objectReferenceValue);
            }

            ec.data_subcategory("Base Configuration", () =>
            {
                EditorGUILayout.PropertyField(circlePoints);
                EditorGUILayout.PropertyField(capSmoothPoints);
                EditorGUILayout.PropertyField(turnSmoothPoints);
                EditorGUILayout.PropertyField(sideVectorRotation);
                EditorGUILayout.PropertyField(width);
                EditorGUILayout.PropertyField(updateNormals);
                EditorGUILayout.PropertyField(loop);

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(points);
                EditorGUI.indentLevel--;
            });

            _line.CalculateUV = ec.data_category(_line.CalculateUV, "Calculate UV", () => EditorGUILayout.PropertyField(scaleUV));
            _line.CalculateVertexColor = ec.data_category(_line.CalculateVertexColor, "Vertex Color", () => EditorGUILayout.PropertyField(vertexColor));
            // showMaterials = ec.data_category(showMaterials, "Material Settings", editorMeshRenderer.OnInspectorGUI);
            // showDebug = ec.data_category(showDebug, "Debug Options", DrawOptions);
            // showStats = ec.data_category(showStats, "Stats", DrawStats);
            ec.data_category(true, "Material Settings", editorMeshRenderer.OnInspectorGUI);
            ec.data_category(true, "Debug Options", DrawOptions);
            ec.data_category(true, "Stats", DrawStats);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
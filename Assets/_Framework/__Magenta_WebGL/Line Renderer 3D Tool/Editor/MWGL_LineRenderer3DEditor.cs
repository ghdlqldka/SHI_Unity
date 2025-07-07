using UnityEditor;
using EA.Editor;
using UnityEngine;
using System;

namespace _Magenta_WebGL
{
    [CustomEditor(typeof(MWGL_LineRenderer3D))]
    public class MWGL_LineRenderer3DEditor : _Magenta_Framework.LineRenderer3D_ExEditor
    {
        // protected LineRenderer3D line;
        protected new MWGL_LineRenderer3D _line
        {
            get
            {
                return line as MWGL_LineRenderer3D;
            }
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            line = target as MWGL_LineRenderer3D;

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            var gizmoMode = serializedObject.FindProperty("_gizmoMode");
            var circlePoints = serializedObject.FindProperty("circlePoints");
            var capSmoothPoints = serializedObject.FindProperty("capSmoothPoints");
            var turnSmoothPoints = serializedObject.FindProperty("turnSmoothPoints");
            var sideVectorRotation = serializedObject.FindProperty("sideVectorRotation");
            // var width = serializedObject.FindProperty("width");
            var _width = serializedObject.FindProperty("_width");
            // var updateNormals = serializedObject.FindProperty("updateNormals");
            // var loop = serializedObject.FindProperty("loop");

            var points = serializedObject.FindProperty("_points");
            var pointObjPrefab = serializedObject.FindProperty("pointObjPrefab");
            var _pointObjList = serializedObject.FindProperty("_pointObjList");

            var renderer = serializedObject.FindProperty("_meshRenderer");
            var vertexColor = serializedObject.FindProperty("vertexColor");
            var scaleUV = serializedObject.FindProperty("scaleUV");

            if (editorMeshRenderer == null)
            {
                editorMeshRenderer = CreateEditor(renderer.objectReferenceValue);
            }

            ec.data_subcategory("Base Configuration", () =>
            {
                EditorGUILayout.PropertyField(gizmoMode);
                EditorGUILayout.PropertyField(circlePoints);
                EditorGUILayout.PropertyField(capSmoothPoints);
                EditorGUILayout.PropertyField(turnSmoothPoints);
                EditorGUILayout.PropertyField(sideVectorRotation);
                // EditorGUILayout.PropertyField(width);
                EditorGUILayout.PropertyField(_width);
                // EditorGUILayout.PropertyField(updateNormals);
                // EditorGUILayout.PropertyField(loop);

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(points);
                EditorGUILayout.PropertyField(pointObjPrefab);
                EditorGUILayout.PropertyField(_pointObjList);
                EditorGUI.indentLevel--;
            });

            _line.CalculateUV = ec.data_category(_line.CalculateUV, "Calculate UV", () => EditorGUILayout.PropertyField(scaleUV));
            // _line.CalculateVertexColor = ec.data_category(_line.CalculateVertexColor, "Vertex Color", () => EditorGUILayout.PropertyField(vertexColor));
            _line.CalculateVertexColor = false;
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
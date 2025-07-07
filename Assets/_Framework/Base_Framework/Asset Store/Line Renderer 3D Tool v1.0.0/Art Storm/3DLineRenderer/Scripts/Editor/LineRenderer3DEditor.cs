using UnityEditor;
using UEditor = UnityEditor.Editor;
using EA.Editor;
using UnityEngine;

namespace EA.Line3D.Editor
{
    [CustomEditor(typeof(LineRenderer3D))]
    public class LineRenderer3DEditor : UEditor
    {
        protected UEditor editorMeshRenderer;

        protected bool showMaterials
        {
            get => EditorPrefs.GetBool("liner_renderer_3d_material_settings_visible");
            set => EditorPrefs.SetBool("liner_renderer_3d_material_settings_visible", value);
        }

        protected bool showStats
        {
            get => EditorPrefs.GetBool("liner_renderer_3d_stats_visible");
            set => EditorPrefs.SetBool("liner_renderer_3d_stats_visible", value);
        }

        protected bool showDebug
        {
            get => EditorPrefs.GetBool("liner_renderer_3d_debug_visible");
            set => EditorPrefs.SetBool("liner_renderer_3d_debug_visible", value);
        }

        protected LineRenderer3D line;

        public override void OnInspectorGUI()
        {
            line = target as LineRenderer3D;

            var circlePoints = serializedObject.FindProperty("circlePoints");
            var capSmoothPoints = serializedObject.FindProperty("capSmoothPoints");
            var turnSmoothPoints = serializedObject.FindProperty("turnSmoothPoints");
            var sideVectorRotation = serializedObject.FindProperty("sideVectorRotation");
            var width = serializedObject.FindProperty("width");
            var updateNormals = serializedObject.FindProperty("updateNormals");
            var loop = serializedObject.FindProperty("loop");            

            var points = serializedObject.FindProperty("points");

            var renderer = serializedObject.FindProperty("_meshRenderer");
            var vertexColor = serializedObject.FindProperty("vertexColor");
            var scaleUV = serializedObject.FindProperty("scaleUV");

            if (editorMeshRenderer == null)
                editorMeshRenderer = CreateEditor(renderer.objectReferenceValue);

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

            line.CalculateUV = ec.data_category(line.CalculateUV, "Calculate UV", () => EditorGUILayout.PropertyField(scaleUV));
            line.CalculateVertexColor = ec.data_category(line.CalculateVertexColor, "Vertex Color", () => EditorGUILayout.PropertyField(vertexColor));
            showMaterials = ec.data_category(showMaterials, "Material Settings", editorMeshRenderer.OnInspectorGUI);
            showDebug = ec.data_category(showDebug, "Debug Options", DrawOptions);
            showStats = ec.data_category(showStats, "Stats", DrawStats);

            serializedObject.ApplyModifiedProperties();
        }

        void OnDisable()
        {
            if (editorMeshRenderer != null)
                DestroyImmediate(editorMeshRenderer);
        }

        protected void DrawOptions()
        {
            line.DrawMesh = EditorGUILayout.Toggle("Draw Mesh", line.DrawMesh);
            line.DrawLine = EditorGUILayout.Toggle("Draw Line", line.DrawLine);
            line.DrawCircles = EditorGUILayout.Toggle("Draw Circles", line.DrawCircles);
            line.DrawNormals = EditorGUILayout.Toggle("Draw Normals", line.DrawNormals);
            line.DrawSideVectors = EditorGUILayout.Toggle("Draw Side Vectors", line.DrawSideVectors);
        }

        protected void DrawStats()
        {
            GUI.enabled = false;
            {
                EditorGUILayout.IntField("Vertex Count", line.VCount);
                EditorGUILayout.IntField("Triangle Count", line.ICount / 3);
                EditorGUILayout.FloatField("Line Length", line.Length);
            }
            GUI.enabled = true;
        }
    }
}
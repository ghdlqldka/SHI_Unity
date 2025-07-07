using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using System;

#if UNITY_EDITOR
namespace PaintIn3D
{
    using UnityEditor;
    // using TARGET = _CwPaintableMeshAtlas;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(_CwPaintableAtlas))]
    public class _CwPaintableAtlasEditor : CwPaintableMeshAtlas_Editor
    {
        protected SerializedProperty m_Script;

        /*
#if DEBUG
        protected SerializedProperty DEBUG_preparedMesh;

        protected SerializedProperty DEBUG_cachedRendererSet;
        protected SerializedProperty DEBUG_cachedRenderer;
#endif
        */

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            /*
#if DEBUG
            DEBUG_preparedMesh = serializedObject.FindProperty("DEBUG_preparedMesh");

            DEBUG_cachedRendererSet = serializedObject.FindProperty("DEBUG_cachedRendererSet");
            DEBUG_cachedRenderer = serializedObject.FindProperty("DEBUG_cachedRenderer");
#endif
            */
        }


        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwPaintableAtlas tgt;
            _CwPaintableAtlas[] tgts; 
            GetTargets(out tgt, out tgts);

            BeginError(Any(tgts, t => t.Parent == null));
            Draw("parent", "The paintable this separate paintable is associated with.");
            EndError();

            // base.OnInspector();
            // TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

            Draw("includeScale", "Transform the mesh with its position, rotation, and scale? Some skinned mesh setups require this to be disabled.");
            Draw("useMesh", "This allows you to choose how the Mesh attached to the current Renderer is used when painting.\n\nAsIs = Use what is currently set in the renderer.\n\nAutoSeamFix = Use (or automatically generate) a seam-fixed version of the mesh currently set in the renderer.");
            Draw("hash", "The hash code for this model used for de/serialization of this instance.");

            Separator();

            if (Button("Analyze Mesh") == true)
            {
                CwMeshAnalysis.OpenWith(tgt.gameObject, tgt.PreparedMesh);
            }

            var mesh = PaintCore.CwCommon.GetMesh(tgt.gameObject, tgt.PreparedMesh);

            if (mesh != null && mesh.isReadable == false)
            {
                Error("You must set the Read/Write Enabled setting in this object's Mesh import settings.");
            }
        }
    }
}
#endif
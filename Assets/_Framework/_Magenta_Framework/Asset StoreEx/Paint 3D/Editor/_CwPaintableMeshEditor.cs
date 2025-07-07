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
	// using TARGET = _CwPaintableMesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(_CwPaintableMesh))]
	public class _CwPaintableMeshEditor : CwPaintable_Editor
    {
        protected SerializedProperty m_Script;


#if DEBUG
        protected SerializedProperty DEBUG_preparedMesh;
        
        protected SerializedProperty DEBUG_cachedRendererSet;
        protected SerializedProperty DEBUG_cachedRenderer;
#endif

        protected virtual void OnEnable()
		{
            m_Script = serializedObject.FindProperty("m_Script");

#if DEBUG
            DEBUG_preparedMesh = serializedObject.FindProperty("DEBUG_preparedMesh");

            DEBUG_cachedRendererSet = serializedObject.FindProperty("DEBUG_cachedRendererSet");
            DEBUG_cachedRenderer = serializedObject.FindProperty("DEBUG_cachedRenderer");
#endif
		}

		protected override void OnInspector()
		{
			GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwPaintableMesh tgt;
            _CwPaintableMesh[] tgts; 
			GetTargets(out tgt, out tgts);

			if (Any(tgts, t => t.IsActivated == true))
			{
				Info("This component has been activated.");
			}

			if (Any(tgts, t => t.IsActivated == true && Application.isPlaying == false))
			{
				Error("This component shouldn't be activated during edit mode. Deactivate it from the component context menu.");
			}

			Draw("activation", "This allows you to control when this component actually activates and becomes ready for painting. You probably don't need to change this.");

			Separator();

			if (Any(tgts, t => t.GetComponentInChildren<_CwPaintableTexture>() == null))
			{
				Warning("Your paintable doesn't have any paintable textures!");
			}

			if (Any(tgts, t => t.MaterialApplication == CwPaintableMesh.MaterialApplicationType.ClonerAndTextures))
			{
				if (Button("Add Material Cloner") == true)
				{
					Each(tgts, t => { 
						if (t.MaterialApplication == CwPaintableMesh.MaterialApplicationType.ClonerAndTextures) 
							t.gameObject.AddComponent<CwMaterialCloner>(); 
					});
				}
			}

			if (Button("Add Paintable Mesh Texture") == true)
			{
				Each(tgts, t => t.gameObject.AddComponent<_CwPaintableTexture>());
			}

			if (Button("Analyze Mesh") == true)
			{
				_CwMeshAnalysisWindow.OpenWith(tgt.gameObject, tgt.PreparedMesh);
			}

			Mesh mesh = PaintCore.CwCommon.GetMesh(tgt.gameObject, tgt.PreparedMesh);

			if (mesh != null && mesh.isReadable == false)
			{
				Error("You must set the Read/Write Enabled setting in this object's Mesh import settings.");
			}

			Separator();

			if (DrawFoldout("Advanced", "Show advanced settings?") == true)
			{
				BeginIndent();
					Draw("baseScale", "If you want the paintable texture width/height to be multiplied by the scale of this GameObject, this allows you to set the scale where you want the multiplier to be 1.");
					Draw("includeScale", "Transform the mesh with its position, rotation, and scale? Some skinned mesh setups require this to be disabled.");
					Draw("useMesh", "This allows you to choose how the Mesh attached to the current Renderer is used when painting.\n\nAsIs = Use what is currently set in the renderer.\n\nAutoSeamFix = Use (or automatically generate) a seam-fixed version of the mesh currently set in the renderer.");
					Draw("materialApplication", "This allows you to specify how the paintable textures will be applied to this paintable object.\n\nPropertyBlock = Using <b>MaterialSetPropertyBlock</b> feature.\n\nClonerAndTextures = Using (Optional) <b>CwMaterialCloner</b> and <b>Material.SetTexture</b> calls.");
					Draw("hash", "The hash code for this model used for de/serialization of this instance.");
					Draw("otherRenderers", "If this material is used in multiple renderers, you can specify them here. This usually happens with different LOD levels.");
					Draw("onActivating");
					Draw("onActivated");
					Draw("onDeactivating");
					Draw("onDeactivated");
				EndIndent();
			}

#if DEBUG
            EditorGUILayout.PropertyField(DEBUG_preparedMesh);

            EditorGUILayout.PropertyField(DEBUG_cachedRendererSet);
            EditorGUILayout.PropertyField(DEBUG_cachedRenderer);
#endif
        }
    }
}
#endif
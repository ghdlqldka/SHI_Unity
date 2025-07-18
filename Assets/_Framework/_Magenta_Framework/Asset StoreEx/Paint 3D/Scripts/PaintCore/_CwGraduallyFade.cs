﻿using UnityEngine;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to fade the pixels of the specified CwPaintableTexture.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwGraduallyFade")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Gradually Fade")]
	public class _CwGraduallyFade : CwGraduallyFade
    {
		
		//
	}
}

#if false //
#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwGraduallyFade;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwGraduallyFade_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.PaintableTexture == null));
				Draw("paintableTexture", "This allows you to choose which paintable texture will be modified by this component.");
			EndError();
			BeginError(Any(tgts, t => t.Threshold <= 0.0f));
				Draw("threshold", "Once this component has accumulated this amount of fade, it will be applied to the PaintableTexture. The lower this value, the smoother the fading will appear, but also the higher the performance cost.");
			EndError();
			BeginError(Any(tgts, t => t.Speed <= 0.0f));
				Draw("speed", "The speed of the fading.\n\n1 = 1 Second.\n\n2 = 0.5 Seconds.");
			EndError();

			Separator();

			Draw("blendMode", "This component will paint using this blending mode.\n\nNOTE: See CwBlendMode documentation for more information.");
			Draw("blendTexture", "The texture that will be faded toward.");
			Draw("blendPaintableTexture", "The paintable texture that will be faded toward.");
			if (Any(tgts, t => t.BlendTexture != null && t.BlendPaintableTexture != null))
			{
				Warning("You have set two blend textures. Only the BlendPaintableTexture will be used.");
			}
			if (Any(tgts, t => t.PaintableTexture != null && t.PaintableTexture == t.BlendTexture))
			{
				Error("The PaintableTexture and BlendPaintableTexture are the same.");
			}
			Draw("blendColor", "The color that will be faded toward.");

			Separator();

			EditorGUILayout.BeginHorizontal();
				Draw("maskTexture", "If you want the gradually fade effect to be masked by a texture, then specify it here.");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maskChannel"), GUIContent.none, GUILayout.Width(50));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
				Draw("maskPaintableTexture", "If you want the gradually fade effect to be masked by a paintable texture, then specify it here.");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maskChannel"), GUIContent.none, GUILayout.Width(50));
			EditorGUILayout.EndHorizontal();
			if (Any(tgts, t => t.MaskTexture != null && t.MaskTexture != null && t.MaskPaintableTexture != null))
			{
				Warning("You have set two mask textures. Only the MaskPaintableTexture will be used.");
			}
			if (Any(tgts, t => t.PaintableTexture != null && t.PaintableTexture == t.MaskPaintableTexture))
			{
				Error("The PaintableTexture and MaskPaintableTexture are the same.");
			}
			if (Any(tgts, t => t.BlendPaintableTexture != null && t.BlendPaintableTexture == t.MaskPaintableTexture))
			{
				Error("The TargetPaintableTexture and MaskPaintableTexture are the same.");
			}
		}
	}
}
#endif
#endif
using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;


#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = _CwTapThrow;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(_CwTapThrow))]
	public class _CwTapThrowEditor : CwTapThrow_Editor
    {
        protected SerializedProperty m_Script;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
		{
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspector();
            _CwTapThrow tgt;
            _CwTapThrow[] tgts; 
			GetTargets(out tgt, out tgts);

			Draw("key", "The key that must be held for this component to activate on desktop platforms.\n\nNone = Any mouse button.");
			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");

			Separator();

			BeginError(Any(tgts, t => t.Prefab == null));
				Draw("prefab", "The prefab that will be thrown.");
			EndError();
			Draw("speed", "Rotate the decal to the hit normal?");
			Draw("storeStates", "Should painting triggered from this component be eligible for being undone?");
		}
	}
}
#endif
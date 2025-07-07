//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{

	[CustomEditor(typeof(_TweenAlpha))]
	public class _TweenAlphaEditor : _UITweenerEditor
	{
		protected SerializedProperty m_Script;

		protected virtual void OnEnable()
		{
			m_Script = serializedObject.FindProperty("m_Script");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(m_Script);

			GUILayout.Space(6f);
			NGUIEditorTools.SetLabelWidth(120f);

			_TweenAlpha tw = target as _TweenAlpha;
			GUI.changed = false;

			var from = EditorGUILayout.Slider("From", tw.from, 0f, 1f);
			var to = EditorGUILayout.Slider("To", tw.to, 0f, 1f);

			var ds = tw.autoCleanup;
			var pn = tw.colorProperty;

			if (tw.GetComponent<MeshRenderer>() != null)
			{
				ds = EditorGUILayout.Toggle("Auto-cleanup", tw.autoCleanup);
				pn = EditorGUILayout.TextField("Color Property", tw.colorProperty);
			}

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Tween Change", tw);
				tw.from = from;
				tw.to = to;
				tw.autoCleanup = ds;
				tw.colorProperty = pn;
				NGUITools.SetDirty(tw);
			}

			DrawCommonProperties();
		}
	}
}
//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{
	[CustomEditor(typeof(_TweenColor))]
	public class _TweenColorEditor : _UITweenerEditor
	{
		public override void OnInspectorGUI()
		{
			GUILayout.Space(6f);
			NGUIEditorTools.SetLabelWidth(120f);

			_TweenColor tw = target as _TweenColor;
			GUI.changed = false;

			Color from = EditorGUILayout.ColorField("From", tw.from);
			Color to = EditorGUILayout.ColorField("To", tw.to);

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Tween Change", tw);
				tw.from = from;
				tw.to = to;
				NGUITools.SetDirty(tw);
			}

			DrawCommonProperties();
		}
	}
}
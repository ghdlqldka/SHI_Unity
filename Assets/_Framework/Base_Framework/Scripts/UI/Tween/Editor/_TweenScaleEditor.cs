//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{
	[CustomEditor(typeof(_TweenScale))]
	public class _TweenScaleEditor : _UITweenerEditor
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

			_TweenScale tw = target as _TweenScale;
			GUI.changed = false;

			Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
			Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);
			// bool table = EditorGUILayout.Toggle("Update Table", tw.updateTable);

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Tween Change", tw);
				tw.from = from;
				tw.to = to;
				// tw.updateTable = table;
				NGUITools.SetDirty(tw);
			}

			DrawCommonProperties();
		}
	}
}
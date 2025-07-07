//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{
	[CustomEditor(typeof(_TweenRotation))]
	public class _TweenRotationEditor : _UITweenerEditor
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

			_TweenRotation tw = target as _TweenRotation;
			GUI.changed = false;

			Vector3 from = EditorGUILayout.Vector3Field("From", tw.from);
			Vector3 to = EditorGUILayout.Vector3Field("To", tw.to);
			var quat = EditorGUILayout.Toggle("Quaternion", tw.quaternionLerp);

			if (GUI.changed)
			{
				NGUIEditorTools.RegisterUndo("Tween Change", tw);
				tw.from = from;
				tw.to = to;
				tw.quaternionLerp = quat;
				NGUITools.SetDirty(tw);
			}

			DrawCommonProperties();
		}
	}
}
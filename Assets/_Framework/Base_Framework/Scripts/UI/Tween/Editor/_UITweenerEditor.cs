//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2023 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

namespace _Base_Framework
{

	[CustomEditor(typeof(_UITweener), true)]
	public class _UITweenerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			GUILayout.Space(6f);
			NGUIEditorTools.SetLabelWidth(110f);
			base.OnInspectorGUI();
			DrawCommonProperties();
		}

		protected void DrawCommonProperties()
		{
			_UITweener tw = target as _UITweener;

			if (NGUIEditorTools.DrawHeader("Tweener"))
			{
				NGUIEditorTools.BeginContents();
				NGUIEditorTools.SetLabelWidth(110f);

				GUI.changed = false;

				_UITweener.Style style = (_UITweener.Style)EditorGUILayout.EnumPopup("Play Style", tw.style);
				AnimationCurve curve = EditorGUILayout.CurveField("Animation Curve", tw.animationCurve, GUILayout.Width(170f), GUILayout.Height(62f));
				//UITweener.Method method = (UITweener.Method)EditorGUILayout.EnumPopup("Play Method", tw.method);

				GUILayout.BeginHorizontal();
				float dur = EditorGUILayout.FloatField("Duration", tw.duration, GUILayout.Width(170f));
				GUILayout.Label("seconds");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				float del = EditorGUILayout.FloatField("Start Delay", tw.delay, GUILayout.Width(170f));
				GUILayout.Label("seconds");
				GUILayout.EndHorizontal();

				var deff = (_UITweener.DelayAffects)EditorGUILayout.EnumPopup("Delay Affects", tw.delayAffects);

				int tg = EditorGUILayout.IntField("Tween Group", tw.tweenGroup, GUILayout.Width(170f));
				bool ts = EditorGUILayout.Toggle("Ignore TimeScale", tw.ignoreTimeScale);
				bool fx = EditorGUILayout.Toggle("Use Fixed Update", tw.useFixedUpdate);

				if (GUI.changed)
				{
					NGUIEditorTools.RegisterUndo("Tween Change", tw);
					tw.animationCurve = curve;
					//tw.method = method;
					tw.style = style;
					tw.ignoreTimeScale = ts;
					tw.tweenGroup = tg;
					tw.duration = dur;
					tw.delay = del;
					tw.delayAffects = deff;
					tw.useFixedUpdate = fx;
					NGUITools.SetDirty(tw);
				}
				NGUIEditorTools.EndContents();
			}

			NGUIEditorTools.SetLabelWidth(80f);
			NGUIEditorTools.DrawEvents("On Finished", tw, tw.onFinished);
		}
	}
}
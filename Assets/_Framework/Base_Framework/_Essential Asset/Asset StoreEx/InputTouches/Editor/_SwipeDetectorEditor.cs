using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

namespace _InputTouches
{
	[CustomEditor(typeof(_SwipeDetector))]
	public class _SwipeDetectorEditor : SwipeDetectorEditor
	{
		protected static _SwipeDetector Instance
		{
			get
			{
				return instance as _SwipeDetector;
			}
		}

		public override void OnInspectorGUI()
		{
			GUI.changed = false;
			EditorGUILayout.Space();

			serializedObject.Update();
			SerializedProperty prop = serializedObject.FindProperty("m_Script");
			EditorGUILayout.PropertyField(prop, true, new GUILayoutOption[0]);
			serializedObject.ApplyModifiedProperties();

			instance = (_SwipeDetector)target;
			if (!Instance.enabled)
				return;

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("MaxSwipeDuration:", "Maximum duration in section for a swipe");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.maxSwipeDuration = EditorGUILayout.FloatField(Instance.maxSwipeDuration);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("MinSpeed:", "Minimum relative speed required for a swipe. This is calculated using (pixel-travelled)/(time- swiped)");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.minSpeed = EditorGUILayout.FloatField(Instance.minSpeed);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("MinDistance:", "Minimum distance in pixels required from the beginning to the end of the swipe");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.minDistance = EditorGUILayout.FloatField(Instance.minDistance);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("MaxDirectionChange:", "Maximum change of direction allowed during the swipe. This is the angle difference measured from the initial swipe direction");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.maxDirectionChange = EditorGUILayout.FloatField(Instance.maxDirectionChange);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("OnlyFireWhenLiftCursor:", "Only fire swipe onSwipeEndE event when the finger/cursor is lifted");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.onlyFireWhenLiftCursor = EditorGUILayout.Toggle(Instance.onlyFireWhenLiftCursor);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("EnableMultiSwipe:", "When checked, there can be multiple Instance of swipe on the screen simultaneously. Otherwise only the first one will be registered");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.enableMultiSwipe = EditorGUILayout.Toggle(Instance.enableMultiSwipe);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			cont = new GUIContent("SwipeCooldown:", "The minimum cooldown duration between 2 subsequent swipe. During the cooldown, no swipe event will be registered");
			EditorGUILayout.LabelField(cont, GUILayout.Width(width));
			Instance.minDurationBetweenSwipe = EditorGUILayout.FloatField(Instance.minDurationBetweenSwipe);
			EditorGUILayout.EndHorizontal();



			/*
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showDefaultFlag=EditorGUILayout.Foldout(showDefaultFlag, "Show default editor");
			EditorGUILayout.EndHorizontal();
			if(showDefaultFlag) DrawDefaultInspector();
			*/

			EditorGUILayout.Space();

			if (GUI.changed)
				EditorUtility.SetDirty(Instance);
		}
	}

}
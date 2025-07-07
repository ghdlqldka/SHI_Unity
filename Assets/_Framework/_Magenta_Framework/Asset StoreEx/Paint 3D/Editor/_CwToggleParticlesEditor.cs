using UnityEngine;
using CW.Common;
using PaintCore;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = _CwToggleParticles;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(_CwToggleParticles))]
	public class _CwToggleParticlesEditor : CwToggleParticles_Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty key;
        protected SerializedProperty _target;
        protected SerializedProperty storeStates;

        protected SerializedProperty _targets;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            key = serializedObject.FindProperty("key");
            _target = serializedObject.FindProperty("target");
            storeStates = serializedObject.FindProperty("storeStates");

            _targets = serializedObject.FindProperty("_targets");
        }

        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspector();
            EditorGUILayout.PropertyField(key);
            // EditorGUILayout.PropertyField(_target);
            EditorGUILayout.PropertyField(_targets);
            EditorGUILayout.PropertyField(storeStates);
        }
	}
}
#endif
using UnityEngine;
using CW.Common;
using PaintCore;
using UnityEngine.InputSystem;


#if UNITY_EDITOR
namespace _Magenta_WebGL
{
	using UnityEditor;
	using TARGET = MWGL_CwToggleParticles;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MWGL_CwToggleParticles))]
	public class MWGL_CwToggleParticlesEditor : PaintIn3D._CwToggleParticlesEditor
    {
        protected override void OnEnable()
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
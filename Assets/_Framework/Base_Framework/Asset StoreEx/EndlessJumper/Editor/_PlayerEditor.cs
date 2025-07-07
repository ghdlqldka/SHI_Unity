using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{
    [CustomEditor(typeof(_Player))]
    public class _PlayerEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty jumpBlue;
        protected SerializedProperty jumpOrange;

        protected SerializedProperty _jumpClip;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            jumpBlue = serializedObject.FindProperty("jumpBlue");
            jumpOrange = serializedObject.FindProperty("jumpOrange");

            _jumpClip = serializedObject.FindProperty("_jumpClip");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(jumpBlue);
            EditorGUILayout.PropertyField(jumpOrange);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_jumpClip);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
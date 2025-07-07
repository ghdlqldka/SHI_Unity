using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    [CustomEditor(typeof(_UdpSender))]
    public class _UdpSenderEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty ip;
        protected SerializedProperty port;

        
        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            ip = serializedObject.FindProperty("ip");
            port = serializedObject.FindProperty("port");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(m_Script);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ip);
            EditorGUILayout.PropertyField(port);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
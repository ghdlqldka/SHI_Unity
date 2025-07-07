using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    [CustomEditor(typeof(_UdpReceiver))]
    public class _UdpReceiverEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty port;

        protected SerializedProperty lastReceivedUDPPacket;
        protected SerializedProperty allReceivedUDPPackets;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            port = serializedObject.FindProperty("port");

            lastReceivedUDPPacket = serializedObject.FindProperty("lastReceivedUDPPacket");
            allReceivedUDPPackets = serializedObject.FindProperty("allReceivedUDPPackets");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(port);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(lastReceivedUDPPacket);
            EditorGUILayout.PropertyField(allReceivedUDPPackets);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
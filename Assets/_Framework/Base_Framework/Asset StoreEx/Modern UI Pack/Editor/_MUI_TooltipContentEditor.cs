using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_TooltipContent))]
    public class _MUI_TooltipContentEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty description;
        protected SerializedProperty delay;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            description = serializedObject.FindProperty("description");
            delay = serializedObject.FindProperty("delay");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(delay);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
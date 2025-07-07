using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
#endif

#if UNITY_EDITOR
namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_Manager))]
    [System.Serializable]
    public class _MUI_ManagerEditor : UIManagerEditor
    {
        protected SerializedProperty m_Script;

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}
#endif
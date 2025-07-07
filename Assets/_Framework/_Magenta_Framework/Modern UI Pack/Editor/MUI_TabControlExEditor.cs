using UnityEngine;
using UnityEditor;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_TabControlEx))]
    public class MUI_TabControlExEditor : Michsky.MUIP._MUI_TabControlEditor
    {
        // protected SerializedProperty m_Script;

#if false
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            wmTarget = (TabControlEx)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
#endif
    }
}
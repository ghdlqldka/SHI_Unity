using UnityEngine;
using UnityEditor;

namespace _Magenta_WebGL
{
    [CustomEditor(typeof(MWGL_MUI_TabControl))]
    public class MWGL_MUI_TabControlEditor : _Magenta_Framework.MUI_TabControlExEditor
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
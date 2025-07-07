#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_DropdownListMultiSelect))]
    public class _MUI_DropdownListMultiSelectEditor : DropdownMultiSelectEditor
    {
        protected SerializedProperty m_Script;

        // protected DropdownMultiSelect dTarget;
        protected _MUI_DropdownListMultiSelect ddlmsTarget
        {
            get
            {
                return dTarget as _MUI_DropdownListMultiSelect;
            }
        }
        // protected UIManagerDropdown tempUIM;
        protected _MUI_DropdownListManager ddlManager
        {
            get
            {
                return tempUIM as _MUI_DropdownListManager;
            }
        }

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            dTarget = (_MUI_DropdownListMultiSelect)target;

            try
            {
                tempUIM = ddlmsTarget.GetComponent<_MUI_DropdownListManager>();
            }
            catch
            {
                //
            }

            if (EditorGUIUtility.isProSkin == true)
            {
                customSkin = MUIPEditorHandler.GetDarkEditor(customSkin);
            }
            else
            {
                customSkin = MUIPEditorHandler.GetLightEditor(customSkin);
            }
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
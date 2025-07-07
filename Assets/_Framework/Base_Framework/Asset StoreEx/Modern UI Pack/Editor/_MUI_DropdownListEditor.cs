using UnityEngine;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_DropdownList))]
    public class _MUI_DropdownListEditor : CustomDropdownEditor
    {
        protected SerializedProperty m_Script;

        // protected CustomDropdown dTarget;
        protected _MUI_DropdownList ddlTarget
        {
            get
            {
                return dTarget as _MUI_DropdownList;
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

            dTarget = (_MUI_DropdownList)target;

            try 
            { 
                tempUIM = ddlTarget.GetComponent<_MUI_DropdownListManager>();
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

            if (ddlTarget.selectedItemIndex > ddlTarget.items.Count - 1)
            {
                ddlTarget.selectedItemIndex = 0;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.changed = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.changed = true;
            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

    }
}
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_Slider))]
    public class _MUI_SliderEditor : SliderManagerEditor
    {
        protected SerializedProperty m_Script;

        // protected SliderManager sTarget;
        protected _MUI_Slider _sTarget
        {
            get
            {
                return sTarget as _MUI_Slider;
            }
        }
        // protected UIManagerSlider tempUIM;
        protected _MUI_SliderManager sManager
        {
            get
            {
                return tempUIM as _MUI_SliderManager;
            }
        }

        protected override void OnEnable()
        {
            sTarget = (_MUI_Slider)target;

            try 
            { 
                tempUIM = _sTarget.GetComponent<_MUI_SliderManager>();
            }
            catch { }

            if (EditorGUIUtility.isProSkin == true)
            { 
                customSkin = MUIPEditorHandler.GetDarkEditor(customSkin);
            }
            else 
            { 
                customSkin = MUIPEditorHandler.GetLightEditor(customSkin);
            }

            m_Script = serializedObject.FindProperty("m_Script");
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();
            GUI.enabled = true;

            base.OnInspectorGUI();
        }
    }
}
#endif
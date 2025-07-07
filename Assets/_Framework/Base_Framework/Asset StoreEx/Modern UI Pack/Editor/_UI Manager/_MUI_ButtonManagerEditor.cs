#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V12;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_ButtonManager))]
    public class _MUI_ButtonManagerEditor : UIManagerButtonEditor
    {
        protected SerializedProperty m_Script;

        // protected UIManagerButton uimTarget;
        protected _MUI_ButtonManager _buttonManager
        {
            get
            {
                return uimTarget as _MUI_ButtonManager;
            }
        }
        
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            uimTarget = (_MUI_ButtonManager)target;
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            // DrawDefaultInspector();
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            // _buttonManager.buttonHandler = EditorGUILayout.ObjectField("Button Handler", _buttonManager.buttonHandler, typeof(_MUI_Button), true) as _MUI_Button;
            if (_buttonManager.buttonHandler == null)
            {
                _buttonManager.buttonHandler = _buttonManager.gameObject.GetComponent<_MUI_Button>();
            }
            EditorGUILayout.Space();

            EditorGUI.indentLevel++;

            if (_buttonManager.buttonHandler == null)
            {
                return;
            }

            _buttonManager.disabledBackground = EditorGUILayout.ObjectField("Background Disabled", _buttonManager.disabledBackground, typeof(Image), true) as Image;
            _buttonManager.normalBackground = EditorGUILayout.ObjectField("Background Normal", _buttonManager.normalBackground, typeof(Image), true) as Image;
            _buttonManager.highlightedBackground = EditorGUILayout.ObjectField("Background Highlighted", _buttonManager.highlightedBackground, typeof(Image), true) as Image;

            if (_buttonManager.buttonHandler.enableIcon)
            {
                _buttonManager.disabledIcon = EditorGUILayout.ObjectField("Icon Disabled", _buttonManager.disabledIcon, typeof(Image), true) as Image;
                _buttonManager.normalIcon = EditorGUILayout.ObjectField("Icon Normal", _buttonManager.normalIcon, typeof(Image), true) as Image;
                _buttonManager.highlightedIcon = EditorGUILayout.ObjectField("Icon Highlighted", _buttonManager.highlightedIcon, typeof(Image), true) as Image;
            }

            if (_buttonManager.buttonHandler.enableText)
            {
                _buttonManager.disabledText = EditorGUILayout.ObjectField("Text Disabled", _buttonManager.disabledText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                _buttonManager.normalText = EditorGUILayout.ObjectField("Text Normal", _buttonManager.normalText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                _buttonManager.highlightedText = EditorGUILayout.ObjectField("Text Highlighted", _buttonManager.highlightedText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
            }
        }
    }
}
#endif
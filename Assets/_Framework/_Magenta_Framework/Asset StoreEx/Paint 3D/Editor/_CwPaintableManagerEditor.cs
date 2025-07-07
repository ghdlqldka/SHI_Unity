using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
    // using TARGET = _CwPaintableManager;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(_CwPaintableManager))]
	public class _CwPaintableManagerEditor : CwPaintableManager_Editor
    {
        protected SerializedProperty m_Script;

        // 
        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
		{
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwPaintableManager tgt;
            _CwPaintableManager[] tgts; 
			GetTargets(out tgt, out tgts);

			Info("This component automatically updates all CwModel and CwPaintableTexture instances at the end of the frame, batching all paint operations together.");

            Draw("readPixelsBudget", "If the current GPU doesn't support async texture reading, this setting allows you to limit how many pixels are read per frame to reduce lag.");
            DrawEx("_paintableModelList");
            DrawEx("_paintableTextureList");
        }

        public static bool DrawEx(string propertyPath, string overrideTooltip = null, string overrideText = null)
        {
            SerializedProperty property = GetPropertyAndSetCustomContentEx(propertyPath, overrideTooltip, overrideText);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(property, customContent, true);

            return EditorGUI.EndChangeCheck();
        }

        protected static SerializedProperty GetPropertyAndSetCustomContentEx(string propertyPath, string overrideTooltip, string overrideText)
        {
            SerializedProperty property = GetProperty(propertyPath);
            Debug.Assert(property != null);

            // customContent.text = string.IsNullOrEmpty(overrideText) == false ? overrideText : property.displayName;
            if (string.IsNullOrEmpty(overrideText) == false)
            {
                customContent.text = overrideText;
            }
            else
            {
                customContent.text = property.displayName;
            }

            customContent.tooltip = string.IsNullOrEmpty(overrideTooltip) == false ? overrideTooltip : property.tooltip;
            customContent.tooltip = StripRichText(customContent.tooltip); // Tooltips can't display rich text for some reason, so strip it

            return property;
        }
    }
}
#endif

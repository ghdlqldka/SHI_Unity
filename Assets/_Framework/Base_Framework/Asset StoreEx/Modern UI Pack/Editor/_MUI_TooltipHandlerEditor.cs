using UnityEngine;
using UnityEditor;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_TooltipHandler))]
    public class _MUI_TooltipHandlerEditor : TooltipManagerEditor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _camera;

        // protected TooltipManager tooltipTarget;
        protected _MUI_TooltipHandler _tooltipTarget
        {
            get
            {
                return tooltipTarget as _MUI_TooltipHandler;
            }
        }
        // protected UIManagerTooltip tempUIM;
        protected _MUI_TooltipManager tooltipManager
        {
            get
            {
                return tempUIM as _MUI_TooltipManager;
            }
        }

        protected override void OnEnable()
        {
            // base.OnEnable();
            /*
            tooltipTarget = (TooltipManager)target;

            try { tempUIM = tooltipTarget.GetComponent<UIManagerTooltip>(); }
            catch { }

            if (EditorGUIUtility.isProSkin == true) { customSkin = MUIPEditorHandler.GetDarkEditor(customSkin); }
            else { customSkin = MUIPEditorHandler.GetLightEditor(customSkin); }
            */
            tooltipTarget = target as _MUI_TooltipHandler;
            try 
            { 
                tempUIM = _tooltipTarget.GetComponent<_MUI_TooltipManager>();
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

            _camera = serializedObject.FindProperty("_camera");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_camera);
            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}
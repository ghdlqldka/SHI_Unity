#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using static Michsky.MUIP.ContextMenuManager;

namespace Michsky.MUIP
{
    [CustomEditor(typeof(_MUI_ContextMenu))]
    public class _MUI_ContextMenuEditor : ContextMenuManagerEditor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty itemParent;

        // protected ContextMenuManager cmTarget;
        protected _MUI_ContextMenu _target
        {
            get
            {
                return cmTarget as _MUI_ContextMenu;
            }
        }

        // protected UIManagerContextMenu tempUIM;
        protected _MUI_ContextMenuManager contextMenuManager
        {
            get
            {
                return tempUIM as _MUI_ContextMenuManager;
            }
        }

        protected override void OnEnable()
        {
            cmTarget = (_MUI_ContextMenu)target;

            try 
            {
                tempUIM = _target.GetComponent<_MUI_ContextMenuManager>();
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
            itemParent = serializedObject.FindProperty("itemParent");
        }

        protected virtual void OnInspectorGUI_Content()
        {
            // var cameraSource = serializedObject.FindProperty("cameraSource");
            var targetCamera = serializedObject.FindProperty("targetCamera");
            var contextButton = serializedObject.FindProperty("contextButton");
            var contextSeparator = serializedObject.FindProperty("contextSeparator");
            var contextSubMenu = serializedObject.FindProperty("contextSubMenu");

            // MUIPEditorHandler.DrawProperty(cameraSource, customSkin, "Camera Source");

            // if (_target.cameraSource == ContextMenuManager.CameraSource.Custom)
            MUIPEditorHandler.DrawProperty(targetCamera, customSkin, "Target Camera");

            EditorGUILayout.Space(5);
            MUIPEditorHandler.DrawProperty(contextButton, customSkin, "Button Prefab");
            MUIPEditorHandler.DrawProperty(contextSeparator, customSkin, "Seperator Prefab");
            MUIPEditorHandler.DrawProperty(contextSubMenu, customSkin, "Sub Menu Prefab");

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(itemParent);
        }

        protected virtual void OnInspectorGUI_Settings()
        {
            // var debugMode = serializedObject.FindProperty("debugMode");
            // var autoSubMenuPosition = serializedObject.FindProperty("autoSubMenuPosition");
            var subMenuBehaviour = serializedObject.FindProperty("subMenuBehaviour");
            

            MUIPEditorHandler.DrawHeader(customSkin, "Options Header", 6);
            // debugMode.boolValue = MUIPEditorHandler.DrawToggle(debugMode.boolValue, customSkin, "Debug Mode");
            // autoSubMenuPosition.boolValue = MUIPEditorHandler.DrawToggle(autoSubMenuPosition.boolValue, customSkin, "Auto Sub Menu Position");
            MUIPEditorHandler.DrawProperty(subMenuBehaviour, customSkin, "Sub Menu Behaviour");
#if UNITY_2022_1_OR_NEWER
            EditorGUILayout.HelpBox("Due to an issue with the event system, the 'Hover' option will be temporarily disabled in Unity 2022.1.", MessageType.Info);
#endif

            MUIPEditorHandler.DrawHeader(customSkin, "UIM Header", 10);

            if (contextMenuManager != null)
            {
                MUIPEditorHandler.DrawUIManagerConnectedHeader();

                if (GUILayout.Button("Open UI Manager", customSkin.button))
                    EditorApplication.ExecuteMenuItem("Tools/Modern UI Pack/Show UI Manager");

                if (GUILayout.Button("Disable UI Manager Connection", customSkin.button))
                {
                    if (EditorUtility.DisplayDialog("Modern UI Pack", "Are you sure you want to disable UI Manager connection with the object? " +
                        "This operation cannot be undone.", "Yes", "Cancel"))
                    {
                        try { DestroyImmediate(contextMenuManager); }
                        catch { Debug.LogError("<b>[Context Menu]</b> Failed to delete UI Manager connection.", this); }
                    }
                }
            }

            else if (contextMenuManager == null) 
            { 
                MUIPEditorHandler.DrawUIManagerDisconnectedHeader();
            }
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();
            GUI.enabled = true;

            MUIPEditorHandler.DrawComponentHeader(customSkin, "CM Top Header");

            GUIContent[] toolbarTabs = new GUIContent[3];
            toolbarTabs[0] = new GUIContent("Content");
            toolbarTabs[1] = new GUIContent("Resources");
            toolbarTabs[2] = new GUIContent("Settings");

            currentTab = MUIPEditorHandler.DrawTabs(currentTab, toolbarTabs, customSkin);

            if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
                currentTab = 0;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
                currentTab = 1;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
                currentTab = 2;

            GUILayout.EndHorizontal();

            // var contextContent = serializedObject.FindProperty("contextContent");
            // var contextAnimator = serializedObject.FindProperty("contextAnimator");
            // var contextButton = serializedObject.FindProperty("contextButton");
            // var contextSeparator = serializedObject.FindProperty("contextSeparator");
            // var contextSubMenu = serializedObject.FindProperty("contextSubMenu");
            // var autoSubMenuPosition = serializedObject.FindProperty("autoSubMenuPosition");
            // var subMenuBehaviour = serializedObject.FindProperty("subMenuBehaviour");
            var vBorderTop = serializedObject.FindProperty("vBorderTop");
            var vBorderBottom = serializedObject.FindProperty("vBorderBottom");
            var hBorderLeft = serializedObject.FindProperty("hBorderLeft");
            var hBorderRight = serializedObject.FindProperty("hBorderRight");
            // var cameraSource = serializedObject.FindProperty("cameraSource");
            // var targetCamera = serializedObject.FindProperty("targetCamera");
            // var debugMode = serializedObject.FindProperty("debugMode");

            switch (currentTab)
            {
                case 0:
                    MUIPEditorHandler.DrawHeader(customSkin, "Content Header", 6);
                    MUIPEditorHandler.DrawProperty(vBorderTop, customSkin, "Vertical Top");
                    MUIPEditorHandler.DrawProperty(vBorderBottom, customSkin, "Vertical Bottom");
                    MUIPEditorHandler.DrawProperty(hBorderLeft, customSkin, "Horizontal Left");
                    MUIPEditorHandler.DrawProperty(hBorderRight, customSkin, "Horizontal Right");
                    OnInspectorGUI_Content();
                    break;

                case 1:
                    MUIPEditorHandler.DrawHeader(customSkin, "Core Header", 6);
                    // MUIPEditorHandler.DrawProperty(contextContent, customSkin, "Context Content");
                    // MUIPEditorHandler.DrawProperty(contextAnimator, customSkin, "Context Animator");
                    // MUIPEditorHandler.DrawProperty(contextButton, customSkin, "Button Prefab");
                    // MUIPEditorHandler.DrawProperty(contextSeparator, customSkin, "Seperator Prefab");
                    // MUIPEditorHandler.DrawProperty(contextSubMenu, customSkin, "Sub Menu Prefab");
                    break;

                case 2:
                    OnInspectorGUI_Settings();
                    
                    break;
            }

            if (Application.isPlaying == false)
            { 
                this.Repaint();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
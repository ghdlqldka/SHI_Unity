#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Michsky.MUIP;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_ListViewEx))]
    public class MUI_ListViewExEditor : Michsky.MUIP._MUI_ListViewEditor
    {
        // protected ListView lvTarget;
        protected new MUI_ListViewEx _lvTarget
        {
            get
            {
                return lvTarget as MUI_ListViewEx;
            }
        }

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // base.OnEnable();
            lvTarget = (MUI_ListViewEx)target;

            if (EditorGUIUtility.isProSkin == true)
            { 
                customSkin = MUIPEditorHandler.GetDarkEditor(customSkin);
            }
            else 
            { 
                customSkin = MUIPEditorHandler.GetLightEditor(customSkin);
            }
        }

        protected override void OnInspectorGUI_Content()
        {
            var rowCount = serializedObject.FindProperty("rowCount");
            var listItems = serializedObject.FindProperty("listItems");

            // Foldout style
            GUIStyle foldoutStyle = customSkin.FindStyle("UIM Foldout");

            MUIPEditorHandler.DrawHeader(customSkin, "Content Header", 6);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel = 1;
            EditorGUILayout.PropertyField(listItems, new GUIContent("List Items"), true);
            EditorGUI.indentLevel = 0;

            if (GUILayout.Button("+ Add a new list item", customSkin.button))
            {
                ListView.ListItem item = new ListView.ListItem();
                _lvTarget.listItems.Add(item);
                return;
            }

            GUILayout.EndVertical();

            MUIPEditorHandler.DrawHeader(customSkin, "Customization Header", 10);
            MUIPEditorHandler.DrawProperty(rowCount, customSkin, "Row Count");

            if (_lvTarget.listItems.Count == 0) 
            { 
                EditorGUILayout.HelpBox("There are no items in the list. ", MessageType.Info);
            }
            else
            {
                int tempHeight;
                if (_lvTarget.listItems.Count < 3) 
                { 
                    tempHeight = 0;
                }
                else 
                { 
                    tempHeight = 300;
                }

                // Scroll panel
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(tempHeight));
                GUILayout.BeginVertical(panelStyle);

                for (int i = 0; i < _lvTarget.listItems.Count; i++)
                {
                    // Start Item Background
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    if (string.IsNullOrEmpty(_lvTarget.listItems[i].itemTitle) == true) 
                    {
                        _lvTarget.listItems[i].isExpanded = EditorGUILayout.Foldout(_lvTarget.listItems[i].isExpanded, "Item #" + i.ToString(), true, foldoutStyle);
                    }
                    else 
                    {
                        _lvTarget.listItems[i].isExpanded = EditorGUILayout.Foldout(_lvTarget.listItems[i].isExpanded, _lvTarget.listItems[i].itemTitle, true, foldoutStyle);
                    }
                    _lvTarget.listItems[i].isExpanded = GUILayout.Toggle(_lvTarget.listItems[i].isExpanded, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);

                    if (_lvTarget.listItems[i].isExpanded)
                    {
                        // Row 1
                        GUILayout.BeginVertical(EditorStyles.helpBox);

                        // Row 1 Type
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent("Row #1 Type"), customSkin.FindStyle("Text"), GUILayout.Width(90));
                        _lvTarget.listItems[i].row0.rowType = (ListView.RowType)EditorGUILayout.EnumPopup(_lvTarget.listItems[i].row0.rowType);
                        GUILayout.EndHorizontal();

                        // Row 1 Content
                        EditorGUI.indentLevel++;

                        if (_lvTarget.listItems[i].row0.rowType == ListView.RowType.Icon)
                        {
                            _lvTarget.listItems[i].row0.rowIcon = EditorGUILayout.ObjectField(_lvTarget.listItems[i].row0.rowIcon, typeof(Sprite), true) as Sprite;
                            _lvTarget.listItems[i].row0.iconScale = EditorGUILayout.FloatField("Icon Scale", _lvTarget.listItems[i].row0.iconScale);
                        }

                        else if (_lvTarget.listItems[i].row0.rowType == ListView.RowType.Text)
                        {
                            _lvTarget.listItems[i].row0.rowText = EditorGUILayout.TextField("Title", _lvTarget.listItems[i].row0.rowText);
                        }

                        _lvTarget.listItems[i].row0.usePreferredWidth = EditorGUILayout.Toggle("Use Preferred Width", _lvTarget.listItems[i].row0.usePreferredWidth);
                        if (_lvTarget.listItems[i].row0.usePreferredWidth == true)
                        {
                            _lvTarget.listItems[i].row0.preferredWidth = EditorGUILayout.IntField("Preferred Width", _lvTarget.listItems[i].row0.preferredWidth);
                        }

                        EditorGUI.indentLevel--;
                        GUILayout.EndVertical();

                        // Row 2
                        if (rowCount.enumValueIndex > 0)
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);

                            // Row 2 Type
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Row #2 Type"), customSkin.FindStyle("Text"), GUILayout.Width(90));
                            _lvTarget.listItems[i].row1.rowType = (ListView.RowType)EditorGUILayout.EnumPopup(_lvTarget.listItems[i].row1.rowType);
                            GUILayout.EndHorizontal();

                            // Row 2 Content
                            EditorGUI.indentLevel++;

                            if (_lvTarget.listItems[i].row1.rowType == ListView.RowType.Icon)
                            {
                                _lvTarget.listItems[i].row1.rowIcon = EditorGUILayout.ObjectField(_lvTarget.listItems[i].row1.rowIcon, typeof(Sprite), true) as Sprite;
                                _lvTarget.listItems[i].row1.iconScale = EditorGUILayout.FloatField("Icon Scale", _lvTarget.listItems[i].row1.iconScale);
                            }

                            else if (_lvTarget.listItems[i].row1.rowType == ListView.RowType.Text)
                            {
                                _lvTarget.listItems[i].row1.rowText = EditorGUILayout.TextField("Title", _lvTarget.listItems[i].row1.rowText);
                            }

                            _lvTarget.listItems[i].row1.usePreferredWidth = EditorGUILayout.Toggle("Use Preferred Width", _lvTarget.listItems[i].row1.usePreferredWidth);
                            if (_lvTarget.listItems[i].row1.usePreferredWidth == true) 
                            {
                                _lvTarget.listItems[i].row1.preferredWidth = EditorGUILayout.IntField("Preferred Width", _lvTarget.listItems[i].row1.preferredWidth);
                            }

                            EditorGUI.indentLevel--;
                            GUILayout.EndVertical();
                        }

                        // Row 3
                        if (rowCount.enumValueIndex > 1)
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);

                            // Row 3 Type
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Row #3 Type"), customSkin.FindStyle("Text"), GUILayout.Width(90));
                            _lvTarget.listItems[i].row2.rowType = (ListView.RowType)EditorGUILayout.EnumPopup(_lvTarget.listItems[i].row2.rowType);
                            GUILayout.EndHorizontal();

                            // Row 3 Content
                            EditorGUI.indentLevel++;

                            if (_lvTarget.listItems[i].row2.rowType == ListView.RowType.Icon)
                            {
                                _lvTarget.listItems[i].row2.rowIcon = EditorGUILayout.ObjectField(_lvTarget.listItems[i].row2.rowIcon, typeof(Sprite), true) as Sprite;
                                _lvTarget.listItems[i].row2.iconScale = EditorGUILayout.FloatField("Icon Scale", _lvTarget.listItems[i].row2.iconScale);
                            }

                            else if (_lvTarget.listItems[i].row2.rowType == ListView.RowType.Text)
                            {
                                _lvTarget.listItems[i].row2.rowText = EditorGUILayout.TextField("Title", _lvTarget.listItems[i].row2.rowText);
                            }

                            _lvTarget.listItems[i].row2.usePreferredWidth = EditorGUILayout.Toggle("Use Preferred Width", _lvTarget.listItems[i].row2.usePreferredWidth);
                            if (_lvTarget.listItems[i].row2.usePreferredWidth == true) 
                            {
                                _lvTarget.listItems[i].row2.preferredWidth = EditorGUILayout.IntField("Preferred Width", _lvTarget.listItems[i].row2.preferredWidth);
                            }

                            EditorGUI.indentLevel--;
                            GUILayout.EndVertical();
                        }
                    }

                    // End item
                    GUILayout.EndVertical();
                }

                if (GUILayout.Button("Pre-Initialize Items", customSkin.button)) 
                {
                    _lvTarget.InitializeItems();
                }

                // Scroll Panel End
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
                if (GUI.enabled == true)
                { 
                    Repaint();
                }
            }
        }

        protected override void OnInspectorGUI_Resources()
        {
            var itemParent = serializedObject.FindProperty("itemParent");
            var itemPreset = serializedObject.FindProperty("itemPreset");
            var scrollbar = serializedObject.FindProperty("scrollbar");

            MUIPEditorHandler.DrawHeader(customSkin, "Core Header", 6);
            MUIPEditorHandler.DrawProperty(itemParent, customSkin, "Item Parent");
            MUIPEditorHandler.DrawProperty(itemPreset, customSkin, "Item Preset");
            MUIPEditorHandler.DrawProperty(scrollbar, customSkin, "Scrollbar");
        }

        protected override void OnInspectorGUI_Settings()
        {
            var initializeOnAwake = serializedObject.FindProperty("initializeOnAwake");
            var showScrollbar = serializedObject.FindProperty("showScrollbar");

            MUIPEditorHandler.DrawHeader(customSkin, "Options Header", 6);
            initializeOnAwake.boolValue = MUIPEditorHandler.DrawToggle(initializeOnAwake.boolValue, customSkin, "Initialize On Awake");
            showScrollbar.boolValue = MUIPEditorHandler.DrawToggle(showScrollbar.boolValue, customSkin, "Show Scrollbar");
            if (GUILayout.Button("Sort List By Name (A to Z)")) 
            {
                _lvTarget.listItems.Sort(SortByNameAtoZ);
            }
            if (GUILayout.Button("Sort List By Name (Z to A)")) 
            {
                _lvTarget.listItems.Sort(SortByNameZtoA);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspectorGUI();
            MUIPEditorHandler.DrawComponentHeader(customSkin, "LV Top Header");

            Color defaultColor = GUI.color;

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

            // Custom panel
            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.textColor = GUI.skin.label.normal.textColor;
            panelStyle.margin = new RectOffset(0, 0, 0, 0);
            panelStyle.padding = new RectOffset(0, 0, 0, 0);

            switch (currentTab)
            {
                case 0:
                    OnInspectorGUI_Content();
                    break;

                case 1:
                    OnInspectorGUI_Resources();
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
using HierarchyIcons;
using UnityEngine;
using UnityEditor;

namespace CGHierarchyIconsEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(_HierarchyIcon))]
    public class _HierarchyIconEditor : HierarchyIconEditor
    {
        // protected HierarchyIcon m_hierarchyIcon;
        protected _HierarchyIcon _hierarchyIcon
        {
            get
            {
                return m_hierarchyIcon as _HierarchyIcon;
            }
            set
            {
                m_hierarchyIcon = value;
            }
        }

        protected override void OnEnable()
        {
            m_iconProperty = serializedObject.FindProperty("icon");
            m_tooltipProperty = serializedObject.FindProperty("tooltip");
            m_positionProperty = serializedObject.FindProperty("position");
            m_directionProperty = serializedObject.FindProperty("direction");

            _hierarchyIcon = target as _HierarchyIcon;
        }

        public override void OnInspectorGUI()
        {
            // #region DRAW SCRIPT HEADER

            GUI.enabled = false;

            DrawPropertiesExcluding(
                serializedObject,
                m_iconProperty.name,
                m_tooltipProperty.name,
                m_positionProperty.name,
                m_directionProperty.name
            );

            GUI.enabled = true;

            // #endregion

            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            // #region PICK ICON BUTTON

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Icon");
                Rect btnRect = EditorGUILayout.GetControlRect(GUILayout.Width(ICON_BUTTON_SIZE), GUILayout.Height(ICON_BUTTON_SIZE));
                GUIContent btnContent = new GUIContent();

                // if selected multiple game objects show a _ character
                if (m_iconProperty.hasMultipleDifferentValues)
                {
                    btnContent.text = "_";
                    btnContent.tooltip = "Different values on selection";
                }
                else
                {
                    // show the selected icon
                    btnContent.image = _hierarchyIcon.icon;
                }

                if (GUI.Button(btnRect, btnContent))
                    PopupWindow.Show(btnRect, new PickIconWindow(m_iconProperty));
            }
            EditorGUILayout.EndHorizontal();

            // #endregion

            // #region POSITION

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_positionProperty);
            if (EditorGUI.EndChangeCheck())
            {
                // repaint the hierarchy
                EditorApplication.RepaintHierarchyWindow();
            }

            // #endregion

            // #region DIRECTION

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_directionProperty);
            if (EditorGUI.EndChangeCheck())
            {
                // repaint the hierarchy
                EditorApplication.RepaintHierarchyWindow();
            }

            // #endregion

            // #region TOOLTIP

            EditorGUILayout.PropertyField(m_tooltipProperty);

            // #endregion

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();

            // space at the bottom
            GUILayout.Space(5);
        }
    }
}
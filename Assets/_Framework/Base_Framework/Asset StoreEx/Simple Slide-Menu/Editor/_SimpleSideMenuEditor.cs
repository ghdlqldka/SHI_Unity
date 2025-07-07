// Simple Side-Menu - https://assetstore.unity.com/packages/tools/gui/simple-side-menu-143623
// Copyright (c) Daniel Lochner

using UnityEditor;
using UnityEngine;

namespace DanielLochner.Assets.SimpleSideMenu
{
    [CustomEditor(typeof(_SimpleSideMenu))]
    public class _SimpleSideMenuEditor : SimpleSideMenuEditor
    {
        protected SerializedProperty m_Script;

        protected _SimpleSideMenu sideMenuEx;

        protected SerializedProperty currentState;
        protected SerializedProperty targetState;

        protected SerializedProperty openHandleSprite;
        protected SerializedProperty closeHandleSprite;

        protected override void OnEnable()
        {
            base.OnEnable();

            sideMenuEx = target as _SimpleSideMenu;

            m_Script = serializedObject.FindProperty("m_Script");

            placement = serializedObject.FindProperty("placementEx");
            currentState = serializedObject.FindProperty("currentState");
            targetState = serializedObject.FindProperty("targetState");

            openHandleSprite = serializedObject.FindProperty("openHandleSprite");
            closeHandleSprite = serializedObject.FindProperty("closeHandleSprite");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space(10);
            // ShowCopyrightNotice();
            // ShowCurrentStateSettings();
            ShowBasicSettings();
            ShowDragSettings();
            ShowOverlaySettings();
            ShowEvents();

            serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(sideMenu);
        }

        protected override void ShowBasicSettings()
        {
            EditorLayoutUtility.Header(ref showBasicSettings, new GUIContent("Basic Settings"));
            if (showBasicSettings)
            {
                EditorGUILayout.PropertyField(placement, new GUIContent("Placement", "The position at which the menu will be placed, which determines how the menu will be opened and closed."));
                EditorGUILayout.LabelField("ClosedPosition : " + sideMenuEx.ClosedPosition);
                EditorGUILayout.LabelField("OpenPosition   : " + sideMenuEx.OpenPosition);

                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(defaultState, new GUIContent("Default State", "Determines whether the menu will be open or closed by default."));
                EditorGUILayout.PropertyField(currentState, new GUIContent("Current State", ""));
                EditorGUILayout.PropertyField(targetState, new GUIContent("Target State", ""));

                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(transitionSpeed, new GUIContent("Transition Speed", "The speed at which the menu will snap into position when transitioning to the next state."));
            }
            EditorGUILayout.Space();
        }

        protected override void ShowDragSettings()
        {
            EditorLayoutUtility.Header(ref showDragSettings, new GUIContent("Drag Settings"));
            if (showDragSettings)
            {
                EditorGUILayout.PropertyField(thresholdDragSpeed, new GUIContent("Threshold Drag Speed", "The minimum speed required when dragging that will allow a transition to the next state to occur."));
                EditorGUILayout.Slider(thresholdDraggedFraction, 0f, 1f, new GUIContent("Threshold Dragged Fraction", "The fraction of the fully opened menu that must be dragged before a transition will occur to the next state if the current drag speed does not exceed the threshold drag speed set."));
                EditorGUILayout.ObjectField(handle, typeof(GameObject), new GUIContent("Handle", "(Optional) GameObject used to open and close the side menu by dragging or pressing (when a \"Button\" component has been added)."));
                if (sideMenu.Handle != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(isHandleDraggable, new GUIContent("Is Draggable", "Should the handle be able to be used to drag the Side-Menu?"));
                    EditorGUILayout.PropertyField(handleToggleStateOnPressed, new GUIContent("Toggle State on Pressed", "Should the Side-Menu toggle its state (open/close) when the handle is pressed?"));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(isMenuDraggable, new GUIContent("Is Menu Draggable", "Should the Side-Menu (itself) be able to be used to drag the Side-Menu?"));

                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(openHandleSprite);
                EditorGUILayout.PropertyField(closeHandleSprite);
            }
            EditorGUILayout.Space();
        }
    }
}
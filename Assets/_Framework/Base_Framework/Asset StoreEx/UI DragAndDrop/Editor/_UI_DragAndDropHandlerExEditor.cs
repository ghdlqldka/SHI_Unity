using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    [CustomEditor(typeof(_UI_DragAndDropHandlerEx))]
    public class _UI_DragAndDropHandlerExEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty type;
        protected SerializedProperty _isDraggable;
        protected SerializedProperty cellSize;

        protected SerializedProperty ui_startInventory;
        protected SerializedProperty ui_destInventory;

        protected SerializedProperty itemDatas;
        protected SerializedProperty draggableItemList;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            type = serializedObject.FindProperty("type");
            _isDraggable = serializedObject.FindProperty("_isDraggable");
            cellSize = serializedObject.FindProperty("cellSize");

            ui_startInventory = serializedObject.FindProperty("ui_startInventory");
            ui_destInventory = serializedObject.FindProperty("ui_destInventory");
            itemDatas = serializedObject.FindProperty("itemDatas");
            draggableItemList = serializedObject.FindProperty("draggableItemList");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            _UI_DragAndDropHandlerEx handlerEx = (_UI_DragAndDropHandlerEx)target;

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(_isDraggable);
            if (handlerEx.Type == _UI_DragAndDropHandlerEx._Type.Static)
            {
                EditorGUILayout.PropertyField(ui_startInventory);
                EditorGUILayout.PropertyField(ui_destInventory);
            }
            else
            {
                EditorGUILayout.PropertyField(cellSize);

                EditorGUILayout.PropertyField(ui_startInventory);
                EditorGUILayout.PropertyField(ui_destInventory);

                EditorGUILayout.PropertyField(itemDatas);
            }
            EditorGUILayout.PropertyField(draggableItemList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{
    [CustomEditor(typeof(_Powerup))]
    public class _PowerupEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty itemPower;
        // protected SerializedProperty Game;
        protected SerializedProperty itemType;
        // protected SerializedProperty mySpriteRenderer;
        protected SerializedProperty mySprites;
        protected SerializedProperty myObjects;

        protected SerializedProperty _audioClip;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            itemType = serializedObject.FindProperty("itemType");
            itemPower = serializedObject.FindProperty("itemPower");
            // Game = serializedObject.FindProperty("Game");

            // mySpriteRenderer = serializedObject.FindProperty("mySpriteRenderer");
            mySprites = serializedObject.FindProperty("mySprites");
            myObjects = serializedObject.FindProperty("myObjects");

            _audioClip = serializedObject.FindProperty("_audioClip");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(itemType);
            EditorGUILayout.PropertyField(itemPower);
            // EditorGUILayout.PropertyField(Game);

            EditorGUILayout.Space(10);
            // EditorGUILayout.PropertyField(mySpriteRenderer);
            EditorGUILayout.PropertyField(mySprites);
            EditorGUILayout.PropertyField(myObjects);
            EditorGUILayout.PropertyField(_audioClip);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
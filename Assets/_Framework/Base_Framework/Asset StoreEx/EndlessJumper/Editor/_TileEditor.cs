using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{
    [CustomEditor(typeof(_Tile))]
    public class _TileEditor : Editor
    {
        protected SerializedProperty m_Script;

        // protected SerializedProperty Game;

        // protected SerializedProperty tileType;

        // protected SerializedProperty mySpriteRenderer;
        protected SerializedProperty mySprites;
        // protected SerializedProperty SFXManager;

        protected SerializedProperty _type;
        protected SerializedProperty brokenClip;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            // Game = serializedObject.FindProperty("Game");
            // tileType = serializedObject.FindProperty("tileType");

            mySprites = serializedObject.FindProperty("mySprites");
            // mySpriteRenderer = serializedObject.FindProperty("mySpriteRenderer");
            // SFXManager = serializedObject.FindProperty("SFXManager");

            _type = serializedObject.FindProperty("_type");
            brokenClip = serializedObject.FindProperty("brokenClip");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            // EditorGUILayout.PropertyField(Game);
            EditorGUILayout.PropertyField(_type);
            // EditorGUILayout.PropertyField(tileType);

            EditorGUILayout.PropertyField(mySprites);
            // EditorGUILayout.PropertyField(mySpriteRenderer, true);
            // EditorGUILayout.PropertyField(SFXManager);

            EditorGUILayout.PropertyField(brokenClip);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
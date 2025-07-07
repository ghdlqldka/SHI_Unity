using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{
    [CustomEditor(typeof(_GameManager))]
    public class _GameManagerEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty poolManagerEx;

        protected SerializedProperty _cam;
        protected SerializedProperty _state;

        protected SerializedProperty player;
        protected SerializedProperty ScreenDistance;
        // protected SerializedProperty score;
        // protected SerializedProperty coins;
        protected SerializedProperty floor;

        // protected SerializedProperty Advanced;

        // protected SerializedProperty defaultTile;
        // protected SerializedProperty defaultItem;
        // protected SerializedProperty defaultEnemy;
        // protected SerializedProperty defaultCoin;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            poolManagerEx = serializedObject.FindProperty("poolManagerEx");

            _cam = serializedObject.FindProperty("_cam");
            _state = serializedObject.FindProperty("_state");

            player = serializedObject.FindProperty("player");
            ScreenDistance = serializedObject.FindProperty("ScreenDistance");
            // score = serializedObject.FindProperty("score");
            // coins = serializedObject.FindProperty("coins");
            floor = serializedObject.FindProperty("floor");

            // Advanced = serializedObject.FindProperty("Advanced");

            // defaultTile = serializedObject.FindProperty("defaultTile");
            // defaultItem = serializedObject.FindProperty("defaultItem");
            // defaultEnemy = serializedObject.FindProperty("defaultEnemy");
            // defaultCoin = serializedObject.FindProperty("defaultCoin");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_cam);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_state);
            EditorGUILayout.PropertyField(poolManagerEx);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(player);
            EditorGUILayout.PropertyField(ScreenDistance);
            // EditorGUILayout.PropertyField(score);
            // EditorGUILayout.PropertyField(coins);
            EditorGUILayout.PropertyField(floor);
            EditorGUILayout.Space();

            // EditorGUILayout.PropertyField(defaultTile);
            // EditorGUILayout.PropertyField(defaultItem);
            // EditorGUILayout.PropertyField(defaultEnemy);
            // EditorGUILayout.PropertyField(defaultCoin);
            // EditorGUILayout.Space();

            // EditorGUILayout.PropertyField(Advanced, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
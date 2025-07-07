using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework._EndlessJumper
{
    [CustomEditor(typeof(_Enemy))]
    public class _EnemyEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty distance;
        protected SerializedProperty speed;

        protected SerializedProperty mySprites;
        protected SerializedProperty enemySpeed;
        protected SerializedProperty enemyDistance;

        protected SerializedProperty mySpriteRenderer;

        protected SerializedProperty enemyType;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            distance = serializedObject.FindProperty("distance");
            speed = serializedObject.FindProperty("speed");

            mySprites = serializedObject.FindProperty("mySprites");
            enemySpeed = serializedObject.FindProperty("enemySpeed");
            enemyDistance = serializedObject.FindProperty("enemyDistance");

            mySpriteRenderer = serializedObject.FindProperty("mySpriteRenderer");

            enemyType = serializedObject.FindProperty("enemyType");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            if (Application.isPlaying)
            {
                EditorGUILayout.PropertyField(distance);
                EditorGUILayout.PropertyField(speed);
            }
            else
            {
                EditorGUILayout.PropertyField(mySprites, true);
                EditorGUILayout.PropertyField(enemySpeed, true);
                EditorGUILayout.PropertyField(enemyDistance, true);
            }

            EditorGUILayout.PropertyField(mySpriteRenderer, true);

            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(enemyType);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
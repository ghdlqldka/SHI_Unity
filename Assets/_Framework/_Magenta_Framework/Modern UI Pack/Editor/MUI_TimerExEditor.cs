using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    [CustomEditor(typeof(MUI_TimerEx))]
    public class MUI_TimerExEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _isPlay;

        // protected SerializedProperty currentPercent;
        // protected SerializedProperty speed;
        protected SerializedProperty remaingTime;
        protected SerializedProperty maxValue;

        protected SerializedProperty loadingBar;
        protected SerializedProperty textPercent;

        // protected SerializedProperty isOn;
        // protected SerializedProperty restart;
        // protected SerializedProperty invert;
        // protected SerializedProperty isPercent;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _isPlay = serializedObject.FindProperty("_isPlay");

            // currentPercent = serializedObject.FindProperty("currentPercent");
            // speed = serializedObject.FindProperty("speed");
            remaingTime = serializedObject.FindProperty("remaingTime");
            maxValue = serializedObject.FindProperty("maxValue");

            loadingBar = serializedObject.FindProperty("loadingBar");
            textPercent = serializedObject.FindProperty("textPercent");

            // isOn = serializedObject.FindProperty("isOn");
            // restart = serializedObject.FindProperty("restart");
            // invert = serializedObject.FindProperty("invert");
            // isPercent = serializedObject.FindProperty("isPercent");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_isPlay);
            EditorGUILayout.Space();

            // EditorGUILayout.PropertyField(mySpriteRenderer, true);
            // EditorGUILayout.PropertyField(currentPercent);
            // EditorGUILayout.PropertyField(speed);
            EditorGUILayout.PropertyField(remaingTime);
            EditorGUILayout.PropertyField(maxValue);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(loadingBar);
            EditorGUILayout.PropertyField(textPercent);

            // EditorGUILayout.PropertyField(isOn);
            // EditorGUILayout.PropertyField(restart);
            // EditorGUILayout.PropertyField(invert);
            // EditorGUILayout.PropertyField(isPercent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
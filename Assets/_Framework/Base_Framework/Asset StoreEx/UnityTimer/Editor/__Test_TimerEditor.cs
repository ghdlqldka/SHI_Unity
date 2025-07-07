using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTimer.Examples
{
    [CustomEditor(typeof(__Test_Timer))]
    public class __Test_TimerEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty createTimerButton;
        protected SerializedProperty CancelTimerButton;
        protected SerializedProperty StartTimerButton;
        protected SerializedProperty PauseTimerButton;
        protected SerializedProperty ResumeTimerButton;

        protected SerializedProperty IsLoopedToggle;
        protected SerializedProperty UseGameTimeToggle;

        protected SerializedProperty TimescaleSlider;

        protected SerializedProperty DurationField;
        protected SerializedProperty NeedsRestartText;

        protected SerializedProperty TimeElapsedText;
        protected SerializedProperty TimeRemainingText;
        protected SerializedProperty PercentageCompletedText;
        protected SerializedProperty PercentageRemainingText;

        protected SerializedProperty NumberOfLoopsText;
        // protected SerializedProperty IsCancelledText;
        // protected SerializedProperty IsCompletedText;
        // protected SerializedProperty IsPausedText;
        // protected SerializedProperty IsDoneText;
        protected SerializedProperty statusText;
        protected SerializedProperty UpdateText;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            createTimerButton = serializedObject.FindProperty("createTimerButton");
            CancelTimerButton = serializedObject.FindProperty("CancelTimerButton");
            StartTimerButton = serializedObject.FindProperty("StartTimerButton");
            PauseTimerButton = serializedObject.FindProperty("PauseTimerButton");
            ResumeTimerButton = serializedObject.FindProperty("ResumeTimerButton");

            IsLoopedToggle = serializedObject.FindProperty("IsLoopedToggle");
            UseGameTimeToggle = serializedObject.FindProperty("UseGameTimeToggle");

            TimescaleSlider = serializedObject.FindProperty("TimescaleSlider");

            DurationField = serializedObject.FindProperty("DurationField");
            NeedsRestartText = serializedObject.FindProperty("NeedsRestartText");

            TimeElapsedText = serializedObject.FindProperty("TimeElapsedText");
            TimeRemainingText = serializedObject.FindProperty("TimeRemainingText");
            PercentageCompletedText = serializedObject.FindProperty("PercentageCompletedText");
            PercentageRemainingText = serializedObject.FindProperty("PercentageRemainingText");

            NumberOfLoopsText = serializedObject.FindProperty("NumberOfLoopsText");
            // IsCancelledText = serializedObject.FindProperty("IsCancelledText");
            // IsCompletedText = serializedObject.FindProperty("IsCompletedText");
            // IsPausedText = serializedObject.FindProperty("IsPausedText");
            // IsDoneText = serializedObject.FindProperty("IsDoneText");
            statusText = serializedObject.FindProperty("statusText");
            UpdateText = serializedObject.FindProperty("UpdateText");

        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(createTimerButton);
            EditorGUILayout.PropertyField(CancelTimerButton);
            EditorGUILayout.PropertyField(StartTimerButton);
            EditorGUILayout.PropertyField(PauseTimerButton);
            EditorGUILayout.PropertyField(ResumeTimerButton);

            EditorGUILayout.PropertyField(IsLoopedToggle);
            EditorGUILayout.PropertyField(UseGameTimeToggle);

            EditorGUILayout.PropertyField(TimescaleSlider);

            EditorGUILayout.PropertyField(DurationField);
            EditorGUILayout.PropertyField(NeedsRestartText);

            EditorGUILayout.PropertyField(TimeElapsedText);
            EditorGUILayout.PropertyField(TimeRemainingText);
            EditorGUILayout.PropertyField(PercentageCompletedText);
            EditorGUILayout.PropertyField(PercentageRemainingText);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(NumberOfLoopsText);
            // EditorGUILayout.PropertyField(IsCancelledText);
            // EditorGUILayout.PropertyField(IsCompletedText);
            // EditorGUILayout.PropertyField(IsPausedText);
            // EditorGUILayout.PropertyField(IsDoneText);
            EditorGUILayout.PropertyField(statusText);
            EditorGUILayout.PropertyField(UpdateText);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
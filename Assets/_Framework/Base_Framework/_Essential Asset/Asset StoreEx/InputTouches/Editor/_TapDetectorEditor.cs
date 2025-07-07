using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _InputTouches
{
    [CustomEditor(typeof(_TapDetector))]
    public class _TapDetectorEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty enableMultiTapFilter;
        protected SerializedProperty maxTapDisplacementAllowance;
        protected SerializedProperty shortTapTime;
        protected SerializedProperty longTapTime;
        protected SerializedProperty multiTapInterval;
        protected SerializedProperty multiTapPosSpacing;
        protected SerializedProperty maxMultiTapCount;
        protected SerializedProperty chargeMode;
        protected SerializedProperty minChargeTime;
        protected SerializedProperty maxChargeTime;
        protected SerializedProperty maxFingerGroupDist;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            enableMultiTapFilter = serializedObject.FindProperty("enableMultiTapFilter");
            maxTapDisplacementAllowance = serializedObject.FindProperty("maxTapDisplacementAllowance");
            shortTapTime = serializedObject.FindProperty("shortTapTime");
            longTapTime = serializedObject.FindProperty("longTapTime");
            multiTapInterval = serializedObject.FindProperty("multiTapInterval");
            multiTapPosSpacing = serializedObject.FindProperty("multiTapPosSpacing");
            maxMultiTapCount = serializedObject.FindProperty("maxMultiTapCount");
            chargeMode = serializedObject.FindProperty("chargeMode");
            minChargeTime = serializedObject.FindProperty("minChargeTime");
            maxChargeTime = serializedObject.FindProperty("maxChargeTime");
            maxFingerGroupDist = serializedObject.FindProperty("maxFingerGroupDist");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(enableMultiTapFilter);
            EditorGUILayout.PropertyField(maxTapDisplacementAllowance);
            EditorGUILayout.PropertyField(shortTapTime);
            EditorGUILayout.PropertyField(longTapTime);
            EditorGUILayout.PropertyField(multiTapInterval);
            EditorGUILayout.PropertyField(multiTapPosSpacing);
            EditorGUILayout.PropertyField(maxMultiTapCount);
            EditorGUILayout.PropertyField(chargeMode);
            EditorGUILayout.PropertyField(minChargeTime);
            EditorGUILayout.PropertyField(maxChargeTime);
            EditorGUILayout.PropertyField(maxFingerGroupDist);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
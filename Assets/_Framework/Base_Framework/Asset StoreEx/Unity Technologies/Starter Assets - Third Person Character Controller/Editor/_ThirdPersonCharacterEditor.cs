using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    [CustomEditor(typeof(_ThirdPersonCharacter))]
    public class _ThirdPersonCharacterEditor : Editor
    {
        protected SerializedProperty m_Script;

		protected SerializedProperty controller;
		protected SerializedProperty rigidBodyPush;

        protected SerializedProperty moveSpeed;
		protected SerializedProperty sprintSpeed;
		protected SerializedProperty rotationSmoothTime;
		protected SerializedProperty speedChangeRate;

		protected SerializedProperty jumpHeight;
		protected SerializedProperty gravity;

		protected SerializedProperty jumpTimeout;
		protected SerializedProperty fallTimeout;

		protected SerializedProperty groundedOffset;
		protected SerializedProperty groundedRadius;
		protected SerializedProperty groundLayers;

		protected SerializedProperty cinemachineCameraFollow;
		protected SerializedProperty topClamp;
		protected SerializedProperty bottomClamp;

		protected SerializedProperty pushLayers;
		protected SerializedProperty canPush;
		protected SerializedProperty strength;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            controller = serializedObject.FindProperty("controller");
            rigidBodyPush = serializedObject.FindProperty("rigidBodyPush");

            moveSpeed = serializedObject.FindProperty("moveSpeed");
            sprintSpeed = serializedObject.FindProperty("sprintSpeed");
            rotationSmoothTime = serializedObject.FindProperty("rotationSmoothTime");
            speedChangeRate = serializedObject.FindProperty("speedChangeRate");

            jumpHeight = serializedObject.FindProperty("jumpHeight");
            gravity = serializedObject.FindProperty("gravity");

            jumpTimeout = serializedObject.FindProperty("jumpTimeout");
            fallTimeout = serializedObject.FindProperty("fallTimeout");

            groundedOffset = serializedObject.FindProperty("groundedOffset");
            groundedRadius = serializedObject.FindProperty("groundedRadius");
            groundLayers = serializedObject.FindProperty("groundLayers");

            cinemachineCameraFollow = serializedObject.FindProperty("cinemachineCameraFollow");
            topClamp = serializedObject.FindProperty("topClamp");
            bottomClamp = serializedObject.FindProperty("bottomClamp");

            pushLayers = serializedObject.FindProperty("pushLayers");
            canPush = serializedObject.FindProperty("canPush");
            strength = serializedObject.FindProperty("strength");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(controller, true);
            EditorGUILayout.PropertyField(rigidBodyPush, true);

            EditorGUILayout.PropertyField(moveSpeed, true);
            EditorGUILayout.PropertyField(sprintSpeed, true);
            EditorGUILayout.PropertyField(rotationSmoothTime, true);
            EditorGUILayout.PropertyField(speedChangeRate, true);

            EditorGUILayout.PropertyField(jumpHeight, true);
            EditorGUILayout.PropertyField(gravity, true);

            EditorGUILayout.PropertyField(jumpTimeout, true);
            EditorGUILayout.PropertyField(fallTimeout, true);

            EditorGUILayout.PropertyField(groundedOffset, true);
            EditorGUILayout.PropertyField(groundedRadius, true);
            EditorGUILayout.PropertyField(groundLayers, true);

            EditorGUILayout.PropertyField(cinemachineCameraFollow, true);
            EditorGUILayout.PropertyField(topClamp, true);
            EditorGUILayout.PropertyField(bottomClamp, true);

            EditorGUILayout.PropertyField(pushLayers, true);
            EditorGUILayout.PropertyField(canPush, true);
            EditorGUILayout.PropertyField(strength, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
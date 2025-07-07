using UnityEditor;
using UnityEngine;

namespace pointcloudviewer.binaryviewer
{
    [CustomEditor(typeof(_PointCloudViewerDX11))]
    public class _PointCloudViewerDX11Editor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty fileName;

        protected SerializedProperty loadAtStart;
        protected SerializedProperty cloudMaterial;
        // protected SerializedProperty instantiateMaterial;
        // protected SerializedProperty useThreading;
        // protected SerializedProperty showDebug;
        protected SerializedProperty applyTranslationMatrix;

        // protected SerializedProperty displayPoints;
        // protected SerializedProperty renderOnlyMainCam;

        // protected SerializedProperty randomizeArray;
        // protected SerializedProperty packColors;

        protected SerializedProperty reSaveBinFile;

        protected SerializedProperty readWholeCloud;
        protected SerializedProperty initialPointsToRead;

        protected SerializedProperty useURPCustomPass;
        protected SerializedProperty useHDRPCustomPass;

        protected SerializedProperty useCommandBuffer;
        protected SerializedProperty camDrawPass;
        protected SerializedProperty forceDepthBufferPass;
        protected SerializedProperty camDepthPass;
#if UNITY_EDITOR
        // protected SerializedProperty commandBufferToSceneCamera;
#endif

        protected SerializedProperty _reader;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            fileName = serializedObject.FindProperty("fileName");

            loadAtStart = serializedObject.FindProperty("loadAtStart");
            cloudMaterial = serializedObject.FindProperty("cloudMaterial");
            // instantiateMaterial = serializedObject.FindProperty("instantiateMaterial");
            // useThreading = serializedObject.FindProperty("useThreading");
            // showDebug = serializedObject.FindProperty("showDebug");
            applyTranslationMatrix = serializedObject.FindProperty("applyTranslationMatrix");

            // displayPoints = serializedObject.FindProperty("displayPoints");
            // renderOnlyMainCam = serializedObject.FindProperty("renderOnlyMainCam");

            // randomizeArray = serializedObject.FindProperty("randomizeArray");
            // packColors = serializedObject.FindProperty("packColors");

            reSaveBinFile = serializedObject.FindProperty("reSaveBinFile");

            readWholeCloud = serializedObject.FindProperty("readWholeCloud");
            initialPointsToRead = serializedObject.FindProperty("initialPointsToRead");

            useURPCustomPass = serializedObject.FindProperty("useURPCustomPass");
            useHDRPCustomPass = serializedObject.FindProperty("useHDRPCustomPass");

            useCommandBuffer = serializedObject.FindProperty("useCommandBuffer");
            camDrawPass = serializedObject.FindProperty("camDrawPass");
            forceDepthBufferPass = serializedObject.FindProperty("forceDepthBufferPass");
            camDepthPass = serializedObject.FindProperty("camDepthPass");

#if UNITY_EDITOR
            // commandBufferToSceneCamera = serializedObject.FindProperty("commandBufferToSceneCamera");
#endif

            _reader = serializedObject.FindProperty("_reader");

        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(fileName);
            EditorGUILayout.PropertyField(_reader);

            EditorGUILayout.PropertyField(loadAtStart);
            EditorGUILayout.PropertyField(cloudMaterial);
            // EditorGUILayout.PropertyField(instantiateMaterial);
            // EditorGUILayout.PropertyField(useThreading);
            // EditorGUILayout.PropertyField(showDebug);
            EditorGUILayout.PropertyField(applyTranslationMatrix);

            // EditorGUILayout.PropertyField(displayPoints);
            // EditorGUILayout.PropertyField(renderOnlyMainCam);

            // EditorGUILayout.PropertyField(randomizeArray);
            // EditorGUILayout.PropertyField(packColors);

            EditorGUILayout.PropertyField(reSaveBinFile);

            EditorGUILayout.PropertyField(readWholeCloud);
            EditorGUILayout.PropertyField(initialPointsToRead);

            EditorGUILayout.PropertyField(useURPCustomPass);
            EditorGUILayout.PropertyField(useHDRPCustomPass);

            EditorGUILayout.PropertyField(useCommandBuffer);
            EditorGUILayout.PropertyField(camDrawPass);
            EditorGUILayout.PropertyField(forceDepthBufferPass);
            EditorGUILayout.PropertyField(camDepthPass);

            // EditorGUILayout.PropertyField(commandBufferToSceneCamera);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
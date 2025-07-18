﻿using UnityEngine;
using UnityEditor;

namespace WorldSpaceTransitions
{
    [CustomEditor(typeof(_SectionSetup))]
    public class _SectionSetupEditor : SectionSetupEditor
    {
        protected SerializedProperty m_Script;

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector(); 
            _SectionSetup setupScript = (_SectionSetup)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_model);
            serializedObject.ApplyModifiedProperties();
            if (setupScript.model != null)
            {
                if (setupScript.GetComponent<ISizedSection>() != null)
                {
                    //setupScript.boundsMode = (BoundsOrientation)EditorGUILayout.EnumPopup("bounds mode:", setupScript.boundsMode);
                    EditorGUILayout.PropertyField(m_boundsMode);
                    if (GUI.changed) 
                        setupScript.RecalculateBounds();
                    //setupScript.accurateBounds = EditorGUILayout.Toggle("accurate bounds", setupScript.accurateBounds);
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Recalculate bounds of " + setupScript.model.name))
                    {
                        setupScript.RecalculateBounds();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(8);

                string sp = setupScript.NewMatsPath;
                GUILayout.Label("new crossSection materials path:", EditorStyles.label);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(sp))
                {
                    sp = EditorUtility.OpenFolderPanel("Select path for crossSection materials", "", "");
                    Debug.Log(Application.dataPath);
                }
                if (sp.Contains(Application.dataPath)) 
                    setupScript.NewMatsPath = sp.Substring(Application.dataPath.Length + 1);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(8);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check shaders on " + setupScript.model.name))
                {
                    shaderInfo = setupScript.CheckShaders();
                    if (setupScript.shaderSubstitutes.Count > 0) 
                        foldout = true;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorStyles.label.wordWrap = true;
                if (shaderInfo != "") 
                    GUILayout.Label(shaderInfo, EditorStyles.label);
            }

            if (setupScript.shaderSubstitutes.Count > 0)
            {
                foldout = EditorGUILayout.Foldout(foldout, "Shader Substitutes", EditorStyles.foldoutHeader);
                EditorGUI.indentLevel += 1;
                if (foldout)
                {
                    for (int i = 0; i < m_shaderSubstitutes.arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(m_shaderSubstitutes.GetArrayElementAtIndex(i).FindPropertyRelative("original"));
                        EditorGUILayout.PropertyField(m_shaderSubstitutes.GetArrayElementAtIndex(i).FindPropertyRelative("substitute"));
                    }
                }
                EditorGUI.indentLevel -= 1;
                serializedObject.ApplyModifiedProperties();
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Create and Assign Section Materials"))
                {
                    setupScript.CreateSectionMaterials();
                    shaderInfo = setupScript.CheckShaders();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
            GUILayout.Space(10);
        }
    }
}


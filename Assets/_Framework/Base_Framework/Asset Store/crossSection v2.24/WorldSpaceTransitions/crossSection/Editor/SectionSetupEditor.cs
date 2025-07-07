using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
namespace WorldSpaceTransitions
{
    [CustomEditor(typeof(SectionSetup))]
    public class SectionSetupEditor : Editor
    {
        protected string shaderInfo = "";
        protected SerializedProperty m_model;
        protected SerializedProperty m_boundsMode;
        protected SerializedProperty m_shaderSubstitutes;
        string m_newPath;
        protected bool foldout = false;
        protected virtual void OnEnable()
        {
            m_model = serializedObject.FindProperty("model");
            m_boundsMode = serializedObject.FindProperty("boundsMode");
            m_shaderSubstitutes = serializedObject.FindProperty("shaderSubstitutes");
        }
        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector(); 
            SectionSetup setupScript = (SectionSetup)target;
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_model);
            serializedObject.ApplyModifiedProperties();
            if (setupScript.model)
            {
                GUIStyle style = GUI.skin.button;
                style.richText = true;
                if (setupScript.GetComponent<ISizedSection>() != null)
                {
                    //setupScript.boundsMode = (BoundsOrientation)EditorGUILayout.EnumPopup("bounds mode:", setupScript.boundsMode);
                    EditorGUILayout.PropertyField(m_boundsMode);
                    if (GUI.changed) setupScript.RecalculateBounds();
                    //setupScript.accurateBounds = EditorGUILayout.Toggle("accurate bounds", setupScript.accurateBounds);
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Recalculate bounds of <i><color=green>" + setupScript.model.name + "</color></i>", style))
                    {
                        setupScript.RecalculateBounds();
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(8);

                string sp = setupScript.NewMatsPath;
                GUILayout.Label("New crossSection materials will get saved inside path:", EditorStyles.label);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(sp))
                {
                    sp = EditorUtility.OpenFolderPanel("Select path for crossSection materials", "", "");
                    Debug.Log(sp);
                    string projpath = Path.GetDirectoryName(Application.dataPath);
                    Debug.Log(projpath);
                    sp = sp.Substring(projpath.Length + 1);
                    Debug.Log(sp);
                    setupScript.NewMatsPath = sp;

                    GUIUtility.ExitGUI();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(8);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check shaders on <i><color=green>" + setupScript.model.name + "</color></i>", style))
                {
                    shaderInfo = setupScript.CheckShaders();
                    if (setupScript.shaderSubstitutes.Count > 0) foldout = true;
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorStyles.label.wordWrap = true;
                if (shaderInfo != "") GUILayout.Label(shaderInfo, EditorStyles.label);
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
#endif

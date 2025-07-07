using UnityEngine;
using UnityEditor;

namespace RTG
{
    [CustomEditor(typeof(_RTScene))]
    public class _RTSceneEditor : RTSceneInspector
    {
        protected SerializedProperty m_Script;

        // protected RTScene _scene;
        protected _RTScene _rtScene
        {
            get 
            { 
                return _scene as _RTScene;
            }
        }

        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _scene = target as _RTScene;

            _rtScene.Settings.FoldoutLabel = "Settings";
            _rtScene.Settings.UsesFoldout = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _rtScene.Settings.RenderEditorGUI(_rtScene);
        }
    }
}

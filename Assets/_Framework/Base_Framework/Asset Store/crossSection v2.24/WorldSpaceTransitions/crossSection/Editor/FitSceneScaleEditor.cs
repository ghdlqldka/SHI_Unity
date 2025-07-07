using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace WorldSpaceTransitions
{
    [CustomEditor(typeof(FitSceneScale))]
    public class FitSceneScaleEditor : Editor
    {
        // Start is called before the first frame update
        void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            FitSceneScale fitSceneScale = (FitSceneScale)target;
            serializedObject.Update();
            SectionSetup ss = fitSceneScale.GetComponent<SectionSetup>();
            GUIStyle style = GUI.skin.button;
            style.richText = true;
            if (ss.model)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Fit scene scale to <i><color=green>" + ss.model.name + "</color></i>", style))
                {
                    fitSceneScale.FitScale();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

        }
    }
}

#endif

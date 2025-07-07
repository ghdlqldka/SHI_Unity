using UnityEngine;
using UnityEditor;

namespace RTG
{
    [CustomEditor(typeof(RTSceneGrid))]
    public class RTSceneGridInspector : Editor
    {
        protected RTSceneGrid _sceneGrid;

        public override void OnInspectorGUI()
        {
            _sceneGrid.Settings.UsesFoldout = true;
            _sceneGrid.Settings.FoldoutLabel = "Settings";
            _sceneGrid.Settings.RenderEditorGUI(_sceneGrid);

            _sceneGrid.LookAndFeel.UsesFoldout = true;
            _sceneGrid.LookAndFeel.FoldoutLabel = "Look & feel";
            _sceneGrid.LookAndFeel.RenderEditorGUI(_sceneGrid);

            _sceneGrid.Hotkeys.UsesFoldout = true;
            _sceneGrid.Hotkeys.FoldoutLabel = "Hotkeys";
            _sceneGrid.Hotkeys.RenderEditorGUI(_sceneGrid);
        }

        protected virtual void OnEnable()
        {
            _sceneGrid = target as RTSceneGrid;
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace RTG
{
    [CustomEditor(typeof(_RTGizmosEngine))]
    public class _RTGizmoEngineEditor : RTGizmoEngineInspector
    {
        // protected RTGizmosEngine _gizmoEngine;
        protected _RTGizmosEngine gizmoEngine
        {
            get
            {
                return _gizmoEngine as _RTGizmosEngine;
            }
        }

        protected SerializedProperty m_Script;
#if DEBUG
        protected SerializedProperty DEBUG_gizmoList;
#endif

        protected override void OnEnable()
        {
            _gizmoEngine = target as _RTGizmosEngine;

            m_Script = serializedObject.FindProperty("m_Script");
#if DEBUG
            DEBUG_gizmoList = serializedObject.FindProperty("DEBUG_gizmoList");
#endif

            gizmoEngine.MainToolbar.GetTabByIndex(_generalTab).AddTargetSettings(gizmoEngine.Settings);
            gizmoEngine.MainToolbar.GetTabByIndex(_sceneGizmo).AddTargetSettings(gizmoEngine.SceneGizmoLookAndFeel);

            gizmoEngine.MoveGizmoSettings2D.FoldoutLabel = "2D Mode settings";
            gizmoEngine.MoveGizmoSettings2D.UsesFoldout = true;
            gizmoEngine.MoveGizmoSettings3D.FoldoutLabel = "3D Mode settings";
            gizmoEngine.MoveGizmoSettings3D.UsesFoldout = true;
            gizmoEngine.MoveGizmoLookAndFeel2D.FoldoutLabel = "2D Mode look & feel";
            gizmoEngine.MoveGizmoLookAndFeel2D.UsesFoldout = true;
            gizmoEngine.MoveGizmoLookAndFeel3D.FoldoutLabel = "3D Mode look & feel";
            gizmoEngine.MoveGizmoLookAndFeel3D.UsesFoldout = true;
            gizmoEngine.MoveGizmoHotkeys.FoldoutLabel = "Hotkeys";
            gizmoEngine.MoveGizmoHotkeys.UsesFoldout = true;
            gizmoEngine.ObjectMoveGizmoSettings.FoldoutLabel = "Object settings";
            gizmoEngine.ObjectMoveGizmoSettings.UsesFoldout = true;

            gizmoEngine.RotationGizmoSettings3D.FoldoutLabel = "Settings";
            gizmoEngine.RotationGizmoSettings3D.UsesFoldout = true;
            gizmoEngine.RotationGizmoLookAndFeel3D.FoldoutLabel = "Look & feel";
            gizmoEngine.RotationGizmoLookAndFeel3D.UsesFoldout = true;
            gizmoEngine.RotationGizmoHotkeys.FoldoutLabel = "Hotkeys";
            gizmoEngine.RotationGizmoHotkeys.UsesFoldout = true;
            gizmoEngine.ObjectRotationGizmoSettings.FoldoutLabel = "Object settings";
            gizmoEngine.ObjectRotationGizmoSettings.UsesFoldout = true;

            gizmoEngine.ScaleGizmoSettings3D.FoldoutLabel = "Settings";
            gizmoEngine.ScaleGizmoSettings3D.UsesFoldout = true;
            gizmoEngine.ScaleGizmoLookAndFeel3D.FoldoutLabel = "Look & feel";
            gizmoEngine.ScaleGizmoLookAndFeel3D.UsesFoldout = true;
            gizmoEngine.ScaleGizmoHotkeys.FoldoutLabel = "Hotkeys";
            gizmoEngine.ScaleGizmoHotkeys.UsesFoldout = true;
            gizmoEngine.ObjectScaleGizmoSettings.FoldoutLabel = "Object settings";
            gizmoEngine.ObjectScaleGizmoSettings.UsesFoldout = true;

            gizmoEngine.UniversalGizmoSettings2D.FoldoutLabel = "2D Mode settings";
            gizmoEngine.UniversalGizmoSettings2D.UsesFoldout = true;
            gizmoEngine.UniversalGizmoSettings3D.FoldoutLabel = "3D Mode settings";
            gizmoEngine.UniversalGizmoSettings3D.UsesFoldout = true;
            gizmoEngine.UniversalGizmoLookAndFeel2D.FoldoutLabel = "2D Mode look & feel";
            gizmoEngine.UniversalGizmoLookAndFeel2D.UsesFoldout = true;
            gizmoEngine.UniversalGizmoLookAndFeel3D.FoldoutLabel = "3D Mode look & feel";
            gizmoEngine.UniversalGizmoLookAndFeel3D.UsesFoldout = true;
            gizmoEngine.UniversalGizmoHotkeys.FoldoutLabel = "Hotkeys";
            gizmoEngine.UniversalGizmoHotkeys.UsesFoldout = true;
            gizmoEngine.ObjectUniversalGizmoSettings.FoldoutLabel = "Object settings";
            gizmoEngine.ObjectUniversalGizmoSettings.UsesFoldout = true;

            EditorToolbarTab tab = gizmoEngine.MainToolbar.GetTabByIndex(_moveGizmoTab);
            tab.AddTargetSettings(gizmoEngine.ObjectMoveGizmoSettings);
            tab.AddTargetSettings(gizmoEngine.MoveGizmoSettings3D);
            tab.AddTargetSettings(gizmoEngine.MoveGizmoSettings2D);
            tab.AddTargetSettings(gizmoEngine.MoveGizmoLookAndFeel3D);
            tab.AddTargetSettings(gizmoEngine.MoveGizmoLookAndFeel2D);
            tab.AddTargetSettings(gizmoEngine.MoveGizmoHotkeys);

            tab = gizmoEngine.MainToolbar.GetTabByIndex(_rotationGizmoTab);
            tab.AddTargetSettings(gizmoEngine.ObjectRotationGizmoSettings);
            tab.AddTargetSettings(gizmoEngine.RotationGizmoSettings3D);
            tab.AddTargetSettings(gizmoEngine.RotationGizmoLookAndFeel3D);
            tab.AddTargetSettings(gizmoEngine.RotationGizmoHotkeys);

            tab = gizmoEngine.MainToolbar.GetTabByIndex(_scaleGizmoTab);
            tab.AddTargetSettings(gizmoEngine.ObjectScaleGizmoSettings);
            tab.AddTargetSettings(gizmoEngine.ScaleGizmoSettings3D);
            tab.AddTargetSettings(gizmoEngine.ScaleGizmoLookAndFeel3D);
            tab.AddTargetSettings(gizmoEngine.ScaleGizmoHotkeys);

            tab = gizmoEngine.MainToolbar.GetTabByIndex(_universalGizmoTab);
            tab.AddTargetSettings(gizmoEngine.UniversalGizmoConfig);
            tab.AddTargetSettings(gizmoEngine.ObjectUniversalGizmoSettings);
            tab.AddTargetSettings(gizmoEngine.UniversalGizmoSettings2D);
            tab.AddTargetSettings(gizmoEngine.UniversalGizmoSettings3D);
            tab.AddTargetSettings(gizmoEngine.UniversalGizmoLookAndFeel2D);
            tab.AddTargetSettings(gizmoEngine.UniversalGizmoLookAndFeel3D);
            tab.AddTargetSettings(gizmoEngine.UniversalGizmoHotkeys);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();
#if DEBUG
            EditorGUILayout.PropertyField(DEBUG_gizmoList);
#endif

            EditorGUILayout.Separator();
            gizmoEngine.MainToolbar.RenderEditorGUI(gizmoEngine);
        }
    }
}

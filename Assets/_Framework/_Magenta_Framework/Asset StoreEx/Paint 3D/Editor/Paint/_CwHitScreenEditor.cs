using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

#if UNITY_EDITOR
namespace PaintIn3D
{
    using UnityEditor;
    // using TARGET = _CwHitScreen;

    [CanEditMultipleObjects]
    [CustomEditor(typeof(_CwHitScreen))]
    public class _CwHitScreenEditor : CwHitScreen_Editor
    {
        protected SerializedProperty m_Script;

        // 
        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            _CwHitScreen tgt;
            _CwHitScreen[] tgts;
            GetTargets(out tgt, out tgts);

            // base.OnInspector();

            Separator();

            DrawBasic();

            Separator();

            DrawAdvancedFoldout();

            Separator();

            var point = tgt.Emit == CwHitScreenBase.EmitType.PointsIn3D;
            var line = tgt.Emit == CwHitScreenBase.EmitType.PointsIn3D && tgt.Connector.ConnectHits == true;
            var triangle = tgt.Emit == CwHitScreenBase.EmitType.TrianglesIn3D;
            var coord = tgt.Emit == CwHitScreenBase.EmitType.PointsOnUV;

            tgt.Connector.HitCache.Inspector(tgt.gameObject, point: point, line: line, triangle: triangle, coord: coord);
        }

        protected override void DrawBasic()
        {
            _CwHitScreen tgt; 
            _CwHitScreen[] tgts; 
            GetTargets(out tgt, out tgts);

            // base.DrawBasic();
            // TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

            if (Any(tgts, t => t.ShouldUpgradePointers() == true))
            {
                if (HelpButton("This component is using legacy control settings and won't be able to paint anything.", MessageType.Warning, "Upgrade", 80) == true)
                {
                    Each(tgts, t => t.TryUpgradePointers(), true, true);
                }
            }

            Draw("emit", "This allows you to control the hit data this component sends out.\n\nPointsIn3D = Point drawing in 3D.\n\nPointsOnUV = Point drawing on UV (requires non-convex MeshCollider).\n\nTrianglesIn3D = Triangle drawing in 3D (requires non-convex MeshCollider).");
            BeginError(Any(tgts, t => t.Layers == 0));
            Draw("layers", "The layers you want the raycast to hit.");
            EndError();
            Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");
            BeginError(Any(tgts, t => CwHelper.GetCamera(t.Camera) == null));
            Draw("_camera", "Orient to a specific camera?\n\nNone = MainCamera.");
            EndError();

            Separator();

            Draw("rotateTo", "This allows you to control how the paint is rotated.\n\nNormal = The rotation will be based on a normal direction, and rolled relative to an up axis.\n\nWorld = The rotation will be aligned to the world, or given no rotation.\n\nThisRotation = The current Transform.rotation will be used.\n\nThisLocalRotation = The current Transform.localRotation will be used.\n\nCustomRotation = The specified Transform.rotation will be used.\n\nCustomLocalRotation = The specified Transform.localRotation will be used.");
            if (Any(tgts, t => t.RotateTo == CwHitScreenBase.RotationType.Normal))
            {
                BeginIndent();
                Draw("normalDirection", "Which direction should the hit point rotation be based on?", "Direction");
                Draw("normalRelativeTo", "Based on the normal direction, what should the rotation be rolled relative to?\n\nWorldUp = It will be rolled so the up vector is world up.\n\nCameraUp = It will be rolled so the up vector is camera up.\n\nDrawAngle = It will be rolled according to the mouse/finger movement on screen.", "Relative To");
                EndIndent();
            }
            if (Any(tgts, t => t.RotateTo == CwHitScreenBase.RotationType.CustomRotation || t.RotateTo == CwHitScreenBase.RotationType.CustomLocalRotation))
            {
                BeginIndent();
                Draw("customTransform", "This allows you to specify the Transform when using RotateTo = CustomRotation/CustomLocalRotation.");
                EndIndent();
            }

            Draw("frequency", "This allows you to control how often the screen is painted.\n\nPixelInterval = Once every Interval pixels.\n\nScaledPixelInterval = Once every Interval scaled pixels.\n\nTimeInterval = Once every Interval seconds.\n\nOnceOnRelease = When the finger/mouse goes down a preview will be shown, and when it goes up the paint will apply.\n\nOnceOnPress = When the finger/mouse goes down the paint will apply.\n\nOnceEveryFrame = Every frame the paint will apply.");
            if (Any(tgts, t => t.Frequency == CwHitScreen.FrequencyType.PixelInterval || t.Frequency == CwHitScreen.FrequencyType.ScaledPixelInterval || t.Frequency == CwHitScreen.FrequencyType.TimeInterval))
            {
                BeginIndent();
                BeginError(Any(tgts, t => t.Interval <= 0.0f));
                Draw("interval", "This allows you to set the pixels/seconds between each hit point based on the current Frequency setting.");
                EndError();
                EndIndent();
            }
        }

        protected override void DrawAdvanced()
        {
            base.DrawAdvanced();

            Separator();

            CwPointConnector_Editor.Draw();
        }
    }
}
#endif
using UnityEngine;
using System.Collections.Generic;
using CW.Common;

#if UNITY_EDITOR
namespace _Magenta_WebGL
{
	using UnityEditor;
	using TARGET = MWGL_CwDragPitchYaw;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(MWGL_CwDragPitchYaw))]
	public class MWGL_CwDragPitchYawEditor : PaintIn3D._CwDragPitchYawEditor
    {
        protected override void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
        }

        protected override void OnInspector()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(m_Script);
            GUI.enabled = true;
            EditorGUILayout.Space();

            // base.OnInspector();
            MWGL_CwDragPitchYaw tgt;
            MWGL_CwDragPitchYaw[] tgts; 
			GetTargets(out tgt, out tgts);

			Draw("tools", "Rotation will be active if all of these tools are deactivated.");
			Draw("key", "The key that must be held for this component to activate on desktop platforms.\n\nNone = Any mouse button.");
			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");

			Separator();

			Draw("pitch", "The target pitch angle in degrees.");
			Draw("pitchSensitivity", "The speed the camera rotates relative to the mouse/finger drag distance.");
			Draw("pitchMin", "The minimum value of the pitch value.");
			Draw("pitchMax", "The maximum value of the pitch value.");

			Separator();

			Draw("yaw", "The target yaw angle in degrees.");
			Draw("yawSensitivity", "The speed the yaw changed relative to the mouse/finger drag distance.");

			Separator();

			Draw("dampening", "How quickly the rotation transitions from the current to the target value (-1 = instant).");
		}
	}
}
#endif
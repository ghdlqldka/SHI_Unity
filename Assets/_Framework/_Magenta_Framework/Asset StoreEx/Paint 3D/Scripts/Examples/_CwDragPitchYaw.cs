using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace PaintIn3D
{
	/// <summary>This component adds basic Pitch/Yaw controls to the current GameObject (e.g. camera) using mouse or touch controls.</summary>
	[ExecuteInEditMode]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwDragPitchYaw")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Drag Pitch Yaw")]
	public class _CwDragPitchYaw : CwDragPitchYaw
    {
        protected override void OnEnable()
        {
            _CwInputManager.EnsureThisComponentExists();

            _CwInputManager.OnFingerDown += HandleFingerDown;
            _CwInputManager.OnFingerUp += HandleFingerUp;
        }
    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwDragPitchYaw;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwDragPitchYaw_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

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
#endif
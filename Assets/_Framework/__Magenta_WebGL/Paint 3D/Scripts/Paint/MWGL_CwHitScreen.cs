using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;

namespace _Magenta_WebGL
{
	/// <summary>This component will perform a raycast under the mouse or finger as it moves across the screen. It will then send hit events to components like <b>CwPaintDecal</b>, allowing you to paint the scene.</summary>
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwHitScreen")]
	// [AddComponentMenu(PaintCore.CwCommon.ComponentHitMenuPrefix + "Hit Screen")]
	public class MWGL_CwHitScreen : PaintIn3D._CwHitScreen
    {
        // private static string LOG_FORMAT = "<color=#FFF4D6><b>[_CwHitScreen]</b></color> {0}";

    }
}

#if false //
#if UNITY_EDITOR
namespace PaintIn3D
{
	using UnityEditor;
	using TARGET = CwHitScreen;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwHitScreen_Editor : CwHitScreenBase_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			DrawBasic();

			Separator();

			DrawAdvancedFoldout();

			Separator();

			var point    = tgt.Emit == CwHitScreenBase.EmitType.PointsIn3D;
			var line     = tgt.Emit == CwHitScreenBase.EmitType.PointsIn3D && tgt.Connector.ConnectHits == true;
			var triangle = tgt.Emit == CwHitScreenBase.EmitType.TrianglesIn3D;
			var coord    = tgt.Emit == CwHitScreenBase.EmitType.PointsOnUV;

			tgt.Connector.HitCache.Inspector(tgt.gameObject, point: point, line: line, triangle: triangle, coord: coord);
		}

		protected override void DrawBasic()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.DrawBasic();

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
#endif
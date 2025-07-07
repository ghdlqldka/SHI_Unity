using UnityEngine;

namespace _Magenta_WebGL
{
	/// <summary>This makes the current <b>Transform</b> follow the <b>Target</b> Transform as if it were a child.</summary>
	[ExecuteInEditMode]
	// [HelpURL(CwShared.HelpUrlPrefix + "CwFollow")]
	// [AddComponentMenu(CwShared.ComponentMenuPrefix + "Follow")]
	public class MWGL_CwFollow : CW.Common._CwFollow
    {
		//
	}
}

#if false //
#if UNITY_EDITOR
namespace CW.Common
{
	using UnityEditor;
	using TARGET = CwFollow;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwFollow_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("follow", "What should this component follow?");
			if (Any(tgts, t => t.Follow == CwFollow.FollowType.TargetTransform))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.Target == null));
						Draw("target", "The transform that will be followed.");
					EndError();
				EndIndent();
			}
			Draw("damping", "How quickly this Transform follows the target.\n\n-1 = instant.");
			Draw("rotate", "Follow the target's rotation too?");
			Draw("ignoreZ", "Ignore Z axis for 2D?");
			Draw("followIn", "Where in the game loop should this component update?");

			Separator();

			Draw("localPosition", "This allows you to specify a positional offset relative to the Target transform.");
			Draw("localRotation", "This allows you to specify a rotational offset relative to the Target transform.");
		}
	}
}
#endif
#endif
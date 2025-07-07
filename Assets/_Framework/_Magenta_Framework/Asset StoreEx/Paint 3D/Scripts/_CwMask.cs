using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace PaintCore
{
	/// <summary>This component allows you to block paint from being applied at the current position using the specified shape.</summary>
	[ExecuteInEditMode]
	[HelpURL(CwCommon.HelpUrlPrefix + "CwMask")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Mask")]
	public class _CwMask : CwMask
    {
		//
	}
}

#if false //
#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwMask;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwMask_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Texture == null));
				Draw("texture", "The mask will use this texture shape.");
			EndError();
			Draw("channel", "The mask will use pixels from this texture channel.");
			Draw("invert", "By default, opaque/white parts of the mask are areas you can paint, and transparent/black parts are parts that are masked. Invert this?");
			Draw("stretch", "If you want the sides of the mask to extend farther out, then this allows you to set the scale of the boundary.\n\n1 = Default.\n\n2 = Double size.");
		}
	}
}
#endif
#endif
using CW.Common;
using UnityEngine;

namespace PaintCore
{
	/// <summary>This component sends pointer information to any <b>CwHitScreen</b> component, allowing you to paint with a touchscreen.</summary>
	// [RequireComponent(typeof(CwHitPointers))]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPointerTouch")]
	// [AddComponentMenu(CwCommon.ComponentHitMenuPrefix + "Pointer Touch")]
	public class _CwPointerTouch : CwPointerTouch
    {
		//
	}
}

#if false //
#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwPointerTouch;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPointerTouch_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("offset");
		}
	}
}
#endif
#endif
using CW.Common;
using PaintIn3D;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
	/// <summary>This component sends pointer information to any <b>CwHitScreen</b> component, allowing you to paint with the mouse.</summary>
	// [RequireComponent(typeof(CwHitPointers))]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPointerMouse")]
	// [AddComponentMenu(CwCommon.ComponentHitMenuPrefix + "Pointer Mouse")]
	public class MWGL_CwPointerMouse : PaintCore._CwPointerMouse
    {
        // private static string LOG_FORMAT = "<color=#00E1FF><b>[_CwPointerMouse]</b></color> {0}";

    }
}

#if false //
#if UNITY_EDITOR
namespace PaintCore
{
	using UnityEditor;
	using TARGET = CwPointerMouse;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class CwPointerMouse_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("preview", "If you enable this, then a paint preview will be shown under the mouse as long as the <b>RequiredKey</b> is not pressed.");
			Draw("keys", "This component will paint while any of the specified mouse buttons or keyboard keys are held.");
		}
	}
}
#endif
#endif
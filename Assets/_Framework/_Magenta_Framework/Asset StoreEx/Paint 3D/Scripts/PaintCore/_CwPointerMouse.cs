using CW.Common;
using PaintIn3D;
using System.Collections.Generic;
using UnityEngine;

namespace PaintCore
{
	/// <summary>This component sends pointer information to any <b>CwHitScreen</b> component, allowing you to paint with the mouse.</summary>
	// [RequireComponent(typeof(CwHitPointers))]
	// [HelpURL(CwCommon.HelpUrlPrefix + "CwPointerMouse")]
	// [AddComponentMenu(CwCommon.ComponentHitMenuPrefix + "Pointer Mouse")]
	public class _CwPointerMouse : CwPointerMouse
    {
        // private static string LOG_FORMAT = "<color=#00E1FF><b>[_CwPointerMouse]</b></color> {0}";

        protected override void OnEnable()
        {
            // base.OnEnable();
            cachedHitPointers = GetComponent<CwHitPointers>();
        }

        protected override void Update()
        {
            var newHeld = false;
            var enablePreview = false;
            var enablePaint = false;

            if (CwInput.GetMouseExists() == true)
            {
                CwInputManager.Finger finger;

                newHeld = GetKeyHeld();
                enablePaint = newHeld == true || oldHeld == true;
                enablePreview = Preview == true && enablePaint == false;

                if (enablePreview == true)
                {
                    GetFinger(PREVIEW_FINGER_INDEX, CwInput.GetMousePosition(), 1.0f, true, out finger);

                    if (cachedHitPointers is _CwHitScreen)
                    {
                        ((_CwHitScreen)cachedHitPointers).HandleFingerUpdate(finger, false, false);
                    }
                    else
                    {
                        cachedHitPointers.HandleFingerUpdate(finger, false, false);
                    }
                }

                if (enablePaint == true)
                {
                    var down = GetFinger(PAINT_FINGER_INDEX, CwInput.GetMousePosition(), 1.0f, true, out finger);

                    if (cachedHitPointers is _CwHitScreen)
                    {
                        ((_CwHitScreen)cachedHitPointers).HandleFingerUpdate(finger, down, newHeld == false);
                    }
                    else
                    {
                        cachedHitPointers.HandleFingerUpdate(finger, down, newHeld == false);
                    }
                }
            }

            if (enablePreview == false)
            {
                TryNullFinger(PREVIEW_FINGER_INDEX);
            }

            if (enablePaint == false)
            {
                TryNullFinger(PAINT_FINGER_INDEX);
            }

            oldHeld = newHeld;
        }
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
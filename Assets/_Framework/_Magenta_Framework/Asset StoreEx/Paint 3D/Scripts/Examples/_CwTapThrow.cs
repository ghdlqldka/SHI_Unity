using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using PaintCore;
using UnityEngine.InputSystem;

namespace PaintIn3D
{
	/// <summary>This component will spawn and throw Rigidbody prefabs from the camera when you tap the mouse or a finger.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwTapThrow")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Tap Throw")]
	public class _CwTapThrow : CwTapThrow
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[_CwTapThrow]</b></color> {0}";

        protected virtual void Awake()
		{
            Debug.Assert(Prefab != null);
		}

        protected override void OnEnable()
        {
            _CwInputManager.EnsureThisComponentExists();

            _CwInputManager.OnFingerDown += HandleFingerDown;
            _CwInputManager.OnFingerUp += HandleFingerUp;
        }

        protected override void HandleFingerDown(CwInputManager.Finger finger)
        {
            // base.HandleFingerDown(finger);

            if (finger.Index == CwInputManager.HOVER_FINGER_INDEX)
                return;

            if (CwInputManager.PointOverGui(finger.ScreenPosition, GuiLayers) == true)
                return;

            if (Key != KeyCode.None && CwInput.GetKeyIsHeld(Key) == false)
                return;

            fingers.Add(finger);
        }

        protected override void HandleFingerUp(CwInputManager.Finger finger)
        {
            // base.HandleFingerUp(finger);

            if (fingers.Remove(finger) == true)
            {
                //var delta = Vector2.Distance(finger.StartScreenPosition, finger.ScreenPosition) * CwInputManager.ScaleFactor;

                if (finger.Age < 0.5f)// && delta < 20.0f)
                {
                    DoThrow(finger.ScreenPosition);
                }
            }
        }

        protected override void DoThrow(Vector2 screenPosition)
        {
            Debug.LogFormat(LOG_FORMAT, "DoThrow()");
            // base.DoThrow(screenPosition);

            Debug.Assert(Prefab != null);
            // if (prefab != null)
            {
                // Camera camera = CwHelper.GetCamera(null);
                Camera camera = _CwPaintableManager.Instance._Camera;
                Debug.Assert(camera != null);
                // if (camera != null)
                {
                    if (storeStates == true)
                    {
                        CwStateManager.PotentiallyStoreAllStates();
                    }

                    // Find the ray for this screen position
                    var ray = camera.ScreenPointToRay(screenPosition);
                    var rotation = Quaternion.LookRotation(ray.direction);

                    // Loop through all prefabs and spawn them
                    var clone = Instantiate(Prefab, ray.origin, rotation);

                    clone.SetActive(true);

                    // Throw with velocity?
                    var cloneRigidbody = clone.GetComponent<Rigidbody>();

                    if (cloneRigidbody != null)
                    {
#if UNITY_6000_0_OR_NEWER
                        cloneRigidbody.linearVelocity = clone.transform.forward * Speed;
#else
							cloneRigidbody.velocity = clone.transform.forward * Speed;
#endif
                    }
                }
            }
        }
    }
}
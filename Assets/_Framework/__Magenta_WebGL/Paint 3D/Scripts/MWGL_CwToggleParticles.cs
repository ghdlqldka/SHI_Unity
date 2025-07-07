using UnityEngine;
using CW.Common;
using PaintCore;
using UnityEngine.InputSystem;

namespace _Magenta_WebGL
{
	/// <summary>This component enables or disables the specified ParticleSystem based on mouse or finger presses.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwToggleParticles")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Toggle Particles")]
	public class MWGL_CwToggleParticles : PaintIn3D._CwToggleParticles
    {
        
        protected override void Awake()
        {
            Target = null;
            Debug.Assert(_Targets.Length > 0);
        }

        protected override void LateUpdate()
        {
            if (Key >= KeyCode.Mouse0 || Key <= KeyCode.Mouse6)
            {
            }

            if (CwInput.GetKeyIsHeld(Key) == true)
            {
                if (storeStates == true && _Targets[0].isPlaying == false)
                {
                    // CwStateManager.PotentiallyStoreAllStates();
                    MWGL_CwPaintableManager.GetOrCreateInstance().PotentiallyStoreAllStates();
                }

                // target.Play();
                for (int i = 0; i < _Targets.Length; i++)
                {
                    _Targets[i].Play();
                }
            }
            else
            {
                // target.Stop();
                for (int i = 0; i < _Targets.Length; i++)
                {
                    _Targets[i].Stop();
                }
            }

        }
    }
}

using UnityEngine;
using CW.Common;
using PaintCore;
using UnityEngine.InputSystem;

namespace PaintIn3D
{
	/// <summary>This component enables or disables the specified ParticleSystem based on mouse or finger presses.</summary>
	[HelpURL(CwCommon.HelpUrlPrefix + "CwToggleParticles")]
	// [AddComponentMenu(CwCommon.ComponentMenuPrefix + "Toggle Particles")]
	public class _CwToggleParticles : CwToggleParticles
    {
        [SerializeField]
        protected ParticleSystem[] _targets;
        public ParticleSystem[] _Targets
        {
            get
            {
                return _targets;
            }
        }

        protected virtual void Awake()
        {
            Target = null;
            Debug.Assert(_Targets.Length > 0);
        }

        protected override void LateUpdate()
        {
#if false //
            if (target != null)
            {
                if (key >= KeyCode.Mouse0 || key <= KeyCode.Mouse6)
                {
                }

                if (CwInput.GetKeyIsHeld(key) == true)
                {
                    if (storeStates == true && target.isPlaying == false)
                    {
                        // CwStateManager.PotentiallyStoreAllStates();
                        _CwPaintableManager.GetOrCreateInstance().PotentiallyStoreAllStates();
                    }

                    target.Play();
                }
                else
                {
                    target.Stop();
                }
            }
#else
            // if (target != null)
            {
                if (Key >= KeyCode.Mouse0 || Key <= KeyCode.Mouse6)
                {
                }

                if (CwInput.GetKeyIsHeld(Key) == true)
                {
                    if (storeStates == true && _Targets[0].isPlaying == false)
                    {
                        // CwStateManager.PotentiallyStoreAllStates();
                        _CwPaintableManager.Instance.PotentiallyStoreAllStates();
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
#endif

        }
    }
}

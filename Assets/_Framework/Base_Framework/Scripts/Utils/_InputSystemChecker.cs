using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace _Base_Framework
{
    public class _InputSystemChecker : Michsky.MUIP.InputSystemChecker
    {
        private static string LOG_FORMAT = "<color=#FF7F00><b>[_InputSystemChecker]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b><color=yellow>" + this.gameObject.name + "</color></b>");

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER

            InputSystemUIInputModule tempModule;
            if (this.gameObject.TryGetComponent<InputSystemUIInputModule>(out tempModule) == false)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "this.gameObject.AddComponent<<b><color=yellow>InputSystemUIInputModule</color></b>>()");

                this.gameObject.AddComponent<InputSystemUIInputModule>();
                StandaloneInputModule oldModule;
                if (this.gameObject.TryGetComponent<StandaloneInputModule>(out oldModule)) 
                {
                    Destroy(oldModule);
                    Debug.LogWarningFormat(LOG_FORMAT, "Destroy(<b><color=red>StandaloneInputModule</color></b>)");
                }
            }
#endif
        }
    }
}
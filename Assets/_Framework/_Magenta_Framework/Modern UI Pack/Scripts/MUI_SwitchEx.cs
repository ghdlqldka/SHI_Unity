using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Magenta_Framework
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Button))]
    public class MUI_SwitchEx : Michsky.MUIP._MUI_Switch
    {
#if false //
        protected override void Awake()
        {
            switchTag = "_AR_Framework_Switch";
            saveValue = false; // Not support Save value !!!!!

            if (enableSwitchSounds == true)
            {
                if (useClickSound == true)
                {
                    Debug.AssertFormat(soundSource != null && clickSound != null, "<b>" + this.gameObject.name + "</b> has NOT SET soundSource or clickSound");
                }
                if (useHoverSound == true)
                {
                    Debug.AssertFormat(soundSource != null && hoverSound != null, "<b>" + this.gameObject.name + "</b> has NOT SET soundSource or hoverSound");
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }
#endif
    }
}
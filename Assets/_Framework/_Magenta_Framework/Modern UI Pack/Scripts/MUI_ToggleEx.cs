using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(Animator))]
    public class MUI_ToggleEx : Michsky.MUIP._MUI_Toggle
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_ToggleEx]</b></color> {0}";

        public override void OnToggled(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnToggled(), this.gameObject : " + this.gameObject.name + ", isOn : " + isOn);

            if (onValueChanged != null)
            {
                onValueChanged.Invoke(isOn);
            }
        }
    }
}
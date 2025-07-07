using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_Framework
{
    public class MUI_ProgressBarEx : Michsky.MUIP._MUI_ProgressBar
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_ProgressBarEx]</b></color> {0}";

        protected override void Awake()
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
#endif
        }
    }
}
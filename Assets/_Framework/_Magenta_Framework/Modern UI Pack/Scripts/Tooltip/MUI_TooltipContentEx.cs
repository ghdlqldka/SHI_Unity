using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace _Magenta_Framework
{
    // [AddComponentMenu("Modern UI Pack/Tooltip/Tooltip Content")]
    public class MUI_TooltipContentEx : Michsky.MUIP._MUI_TooltipContent
    {
        // private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_TooltipContentEx]</b></color> {0}";

#if false //
        protected override void Awake()
        {
            tooltipRect = null; // forcelly set!!!! Not used
            descriptionText = null; // forcelly set!!!! Not used

            tpManager = null; // forcelly set!!!! Not used
            tooltipAnimator = null; // forcelly set!!!! Not used
        }

        protected override void Start()
        {
            //
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            MUI_TooltipHandlerEx.Instance.ShowTooltip(true, delay, description);
        }

        // public void OnPointerExit(PointerEventData eventData)
        public override void OnPointerExit(PointerEventData eventData)
        {
            MUI_TooltipHandlerEx.Instance.ShowTooltip(false, delay);
        }

        protected override IEnumerator ShowTooltip()
        {
            throw new System.NotSupportedException("");
        }
#endif
    }
}
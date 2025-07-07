using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace _Magenta_WebGL
{
    // [AddComponentMenu("Modern UI Pack/Tooltip/Tooltip Content")]
    public class MWGL_MUI_TooltipContent : _Magenta_Framework.MUI_TooltipContentEx
    {
        // private static string LOG_FORMAT = "<color=#EBC6FB><b>[MWGL_MUI_TooltipContent]</b></color> {0}";

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
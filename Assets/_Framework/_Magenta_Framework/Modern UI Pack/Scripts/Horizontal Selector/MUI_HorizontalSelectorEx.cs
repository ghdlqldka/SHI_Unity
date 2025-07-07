using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace _Magenta_Framework
{
    public class MUI_HorizontalSelectorEx : Michsky.MUIP._MUI_HorizontalSelector
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_HorizontalSelectorEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>" +
                ", invokeAtStart : <b><color=yellow>" + invokeAtStart + "</color></b>");
            // base.Awake();

            if (selectorAnimator == null)
            {
                selectorAnimator = this.gameObject.GetComponent<Animator>();
            }
            if (label == null || labelHelper == null)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Cannot initalize the object due to missing resources.", this);
                return;
            }

            SetupSelector();
            UpdateContentLayout();

            if (invokeAtStart == true)
            {
                Debug.LogFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@");
                Invoke_OnItemSelect();
            }
        }
    }
}
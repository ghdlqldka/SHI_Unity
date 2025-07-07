using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    public class UI_ToastMessageHandlerEx : _Base_Framework._UI_ToastMessageHandler
    {
        private static string LOG_FORMAT = "<color=white><b>[UI_ToastMessageHandlerEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(dimImage.enabled == false);
            backgroundImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);

            // messageText = this.transform.Find("Text").GetComponent<Text>();
        }
    }
}
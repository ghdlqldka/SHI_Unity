using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_WebGL
{
    public class MWGL_UI_ToastMessageHandler : _Magenta_Framework.UI_ToastMessageHandlerEx
    {
        private static string LOG_FORMAT = "<color=white><b>[MWGL_UI_ToastMessageHandler]</b></color> {0}";

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
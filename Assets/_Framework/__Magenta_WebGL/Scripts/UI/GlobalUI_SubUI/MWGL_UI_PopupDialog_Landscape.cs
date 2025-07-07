using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Magenta_WebGL
{
    public class MWGL_UI_PopupDialog_Landscape : Magenta_Framework.UI_PopupDialog_LandscapeEx
    {
        private static string LOG_FORMAT = "<color=white><b>[MWGL_UI_PopupDialog_</b></color><color=red><b>Landscape]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(this.gameObject.activeSelf == false);
#if DEBUG
            Image dimImage = this.GetComponent<Image>();
            Debug.Assert(dimImage != null);
            Debug.Assert(dimImage.enabled == true); // Always, dimImage must be ENABLED!!!!!
#endif
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_MUI_TabControl : _Magenta_Framework.MUI_TabControlEx
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MWGL_MUI_TabControl]</b></color> {0}";

        public override void OnClickTabButton(string newPanel)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTabButton(), newPanel : " + newPanel);

            OpenWindow(newPanel);
        }
    }
}
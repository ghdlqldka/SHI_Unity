using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class MUI_TabControlEx : Michsky.MUIP._MUI_TabControl
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_TabControlEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name +
                "</b>, windows.Count : <b>" + windows.Count + "</b>");

            Debug.Assert(windows.Count > 0);

            InitializeWindows();
        }

        public override void OnClickTabButton(string newPanel)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickTabButton(), newPanel : <b>" + newPanel + "</b>");

            OpenWindow(newPanel);
        }
    }
}
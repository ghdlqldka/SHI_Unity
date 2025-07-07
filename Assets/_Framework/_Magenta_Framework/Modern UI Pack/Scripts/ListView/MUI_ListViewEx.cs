using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class MUI_ListViewEx : Michsky.MUIP._MUI_ListView
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_ListViewEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            if (itemParent == null) 
            { 
                Debug.LogErrorFormat(LOG_FORMAT, "<b>[List View]</b> 'Item Parent' is missing."); 
                return;
            }
            if (initializeOnAwake == true) 
            { 
                InitializeItems();
            }
        }
    }
}
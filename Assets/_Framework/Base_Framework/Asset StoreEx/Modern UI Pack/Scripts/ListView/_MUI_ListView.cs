using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.MUIP
{
    public class _MUI_ListView : ListView
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_ListView]</b></color> {0}";

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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_InputFieldManagerEx : Michsky.MUIP._MUI_InputFieldManager
    {
        // private static string LOG_FORMAT = "<color=#CD9A9A><b>[MUI_InputFieldManagerEx]</b></color> {0}";

#if false //
        protected override void Awake()
        {
            if (Application.isPlaying == true && webglMode == true)
            {
                return;
            }

            try
            {
                if (UIManagerAsset == null)
                {
                    UIManagerAsset = Resources.Load<MUI_ManagerEx>("AR_Framework/MUIP Manager Ex");
                }
                Debug.Assert(UIManagerAsset != null);

                this.enabled = true;

                if (UIManagerAsset.enableDynamicUpdate == false)
                {
                    UpdateInputField();
                    this.enabled = false;
                }
            }
            catch 
            {
                Debug.LogErrorFormat(LOG_FORMAT, "No UI Manager found, assign it manually.", this); 
            }
        }
#endif
    }
}
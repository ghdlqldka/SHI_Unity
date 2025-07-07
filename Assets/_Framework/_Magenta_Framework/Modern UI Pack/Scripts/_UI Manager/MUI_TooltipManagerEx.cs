using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_TooltipManagerEx : Michsky.MUIP._MUI_TooltipManager
    {
#if false //
        protected override void Awake()
        {
            if (Application.isPlaying && webglMode == true)
                return;

            try
            {
                Debug.Assert(UIManagerAsset != null);
                /*
                if (UIManagerAsset == null)
                    UIManagerAsset = Resources.Load<UIManager>("_MUIP Manager");
                */

                this.enabled = true;

                if (UIManagerAsset.enableDynamicUpdate == false)
                {
                    UpdateTooltip();
                    this.enabled = false;
                }
            }

            catch { Debug.Log("<b>[Modern UI Pack]</b> No UI Manager found, assign it manually.", this); }
        }
#endif
    }
}
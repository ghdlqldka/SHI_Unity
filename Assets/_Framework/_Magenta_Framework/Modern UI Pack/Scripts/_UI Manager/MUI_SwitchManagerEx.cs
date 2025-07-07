using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_SwitchManagerEx : Michsky.MUIP._MUI_SwitchManager
    {
        // private static string LOG_FORMAT = "<color=#CD9A9A><b>[UIManagerSwitchEx]</b></color> {0}";

#if false //
        protected override void Awake()
        {
            if (Application.isPlaying && webglMode == true)
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
                    UpdateSwitch();
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
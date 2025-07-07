using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_WebGL
{
    [ExecuteInEditMode]
    public class MWGL_MUI_ButtonManager : _Magenta_Framework.MUI_ButtonManagerEx
    {
        // private static string LOG_FORMAT = "<color=#B1C5EF><b>[MWGL_MUI_ButtonManager]</b></color> {0}";

#if false //
        protected override void Awake()
        {
            if (Application.isPlaying == true && webglMode == true)
                return;

            try
            {
                if (UIManagerAsset == null)
                {
                    UIManagerAsset = Resources.Load<MUI_ManagerEx>("AR_Framework/MUIP Manager Ex");
                }
                Debug.Assert(UIManagerAsset != null);

                /*
                this.enabled = true;

                if (UIManagerAsset.enableDynamicUpdate == false)
                {
                    UpdateButton();
                    this.enabled = false;
                }
                */
            }
            catch 
            { 
                Debug.LogErrorFormat(LOG_FORMAT, "No UI Manager found, assign it manually.", this);
            }
        }
#endif
    }
}
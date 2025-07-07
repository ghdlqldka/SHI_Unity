using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_ButtonManagerEx : Michsky.MUIP._MUI_ButtonManager
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[MUI_ButtonManagerEx]</b></color> {0}";

        public new MUI_ButtonEx buttonHandler
        {
            get
            {
                return buttonManager as MUI_ButtonEx;
            }
            set
            {
                buttonManager = value;
            }
        }

        protected override void Awake()
        {
            // base.Awake();

            try
            {
                Debug.Assert(_ManagerAsset != null);

                if (buttonHandler == null)
                {
                    buttonHandler = GetComponent<MUI_ButtonEx>();
                }
                Debug.AssertFormat(_ManagerAsset != null, "this.gameObject.name : " + this.gameObject.name);

                // this.enabled = true;
                if (_ManagerAsset.enableDynamicUpdate == false && this.enabled == true)
                {
                    UpdateButton();
                    this.enabled = false;
                }
            }
            catch
            {
                Debug.LogFormat(LOG_FORMAT, "No UI Manager found, assign it manually.", this);
            }
        }
    }
}
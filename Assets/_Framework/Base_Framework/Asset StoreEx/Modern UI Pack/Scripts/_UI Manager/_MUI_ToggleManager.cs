using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class _MUI_ToggleManager : UIManagerToggle
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_ToggleManager]</b></color> {0}";

        // protected UIManager UIManagerAsset;
        protected _MUI_Manager _ManagerAsset
        {
            get
            {
                return UIManagerAsset as _MUI_Manager;
            }
            set
            {
                UIManagerAsset = value;
            }
        }

        protected override void Awake()
        {
            // base.Awake();

            try
            {
                /*
                if (UIManagerAsset == null)
                {
                    UIManagerAsset = Resources.Load<MUI_Manager>("_MUIP Manager");
                }
                */
                Debug.Assert(_ManagerAsset != null);

                // this.enabled = true;
                if (_ManagerAsset.enableDynamicUpdate == false)
                {
                    UpdateToggle();
                    this.enabled = false;
                }
            }
            catch
            { 
                Debug.LogErrorFormat(LOG_FORMAT, "No UI Manager found, assign it manually.", this);
            }
        }

        protected override void Update()
        {
            if (_ManagerAsset == null || this.enabled == false)
            { 
                return;
            }
            if (_ManagerAsset.enableDynamicUpdate) { UpdateToggle(); }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_ScrollbarExManager : Michsky.MUIP._MUI_ScrollbarManager
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_ScrollbarExManager]</b></color> {0}";

        // protected UIManager UIManagerAsset;
        protected new MUI_ManagerEx _ManagerAsset
        {
            get
            {
                return UIManagerAsset as MUI_ManagerEx;
            }
            set
            {
                UIManagerAsset = value;
            }
        }

        protected override void Awake()
        {
            try
            {
                Debug.Assert(_ManagerAsset != null);

                if (_ManagerAsset.enableDynamicUpdate == false && this.enabled == true)
                {
                    UpdateScrollbar();
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
            if (_ManagerAsset.enableDynamicUpdate == true)
            { 
                UpdateScrollbar();
            }
        }
    }
}
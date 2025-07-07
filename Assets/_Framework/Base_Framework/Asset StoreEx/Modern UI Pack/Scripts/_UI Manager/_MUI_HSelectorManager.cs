using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class _MUI_HSelectorManager : UIManagerHSelector
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_HSelectorManager]</b></color> {0}";

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

                if (_ManagerAsset.enableDynamicUpdate == false && this.enabled == true)
                {
                    UpdateSelector();
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
            if (_ManagerAsset.enableDynamicUpdate == true) { UpdateSelector(); }
        }
    }
}
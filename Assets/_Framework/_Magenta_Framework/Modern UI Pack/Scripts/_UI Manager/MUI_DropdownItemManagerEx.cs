using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_DropdownItemExManager : Michsky.MUIP._MUI_DropdownItemManager
    {
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
            // if (_ManagerAsset == null) { _ManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }
            Debug.Assert(_ManagerAsset != null);

            // this.enabled = true;

            if (_ManagerAsset.enableDynamicUpdate == false && this.enabled == true)
            {
                UpdateDropdown();
                this.enabled = false;
            }
        }

        protected override void Update()
        {
            Debug.Assert(_ManagerAsset != null);
            if (/*_ManagerAsset == null || */this.enabled == false) 
            { 
                return; 
            }

            if (_ManagerAsset.enableDynamicUpdate == true)
            {
                UpdateDropdown();
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Magenta_Framework
{
    [ExecuteInEditMode]
    public class MUI_DropdownListExManager : Michsky.MUIP._MUI_DropdownListManager
    {
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
            // if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }
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
            if (_ManagerAsset == null || this.enabled == false)
            {
                return; 
            }
            if (_ManagerAsset.enableDynamicUpdate == true) { UpdateDropdown(); }
        }
    }
}
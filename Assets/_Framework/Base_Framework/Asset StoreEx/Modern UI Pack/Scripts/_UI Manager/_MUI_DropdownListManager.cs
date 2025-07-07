using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class _MUI_DropdownListManager : UIManagerDropdown
    {
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
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class _MUI_ProgressBarLoopManager : UIManagerProgressBarLoop
    {
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
                UpdateProgressBar();
                this.enabled = false;
            }
        }

        protected override void Update()
        {
            if (_ManagerAsset == null || this.enabled == false) 
            { 
                return; 
            }
            if (_ManagerAsset.enableDynamicUpdate == true) { UpdateProgressBar(); }
        }
    }
}
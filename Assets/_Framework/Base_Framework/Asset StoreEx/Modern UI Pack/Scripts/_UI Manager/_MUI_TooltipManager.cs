using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class _MUI_TooltipManager : UIManagerTooltip
    {
        protected _MUI_Manager _ManagerAsset
        {
            get
            {
                return UIManagerAsset as _MUI_Manager;
            }
        }

        protected override void Awake()
        {
            try
            {
                /*
                if (UIManagerAsset == null)
                    UIManagerAsset = Resources.Load<UIManager>("_MUIP Manager");
                */
                Debug.Assert(_ManagerAsset != null);

                this.enabled = true;

                if (_ManagerAsset.enableDynamicUpdate == false)
                {
                    UpdateTooltip();
                    this.enabled = false;
                }
            }

            catch { Debug.Log("<b>[Modern UI Pack]</b> No UI Manager found, assign it manually.", this); }
        }

    }
}
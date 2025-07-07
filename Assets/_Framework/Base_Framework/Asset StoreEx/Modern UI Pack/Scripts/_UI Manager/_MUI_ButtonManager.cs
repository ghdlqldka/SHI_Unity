using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.MUIP
{
    [ExecuteInEditMode]
    public class _MUI_ButtonManager : UIManagerButton
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_ButtonManager]</b></color> {0}";

        // public ButtonManager buttonManager;
        public _MUI_Button buttonHandler
        {
            get
            {
                return buttonManager as _MUI_Button;
            }
            set
            {
                buttonManager = value;
            }
        }

        // protected UIManager UIManagerAsset;
        protected _MUI_Manager _ManagerAsset
        {
            get
            {
                return UIManagerAsset as _MUI_Manager;
            }
            /*
            set
            {
                UIManagerAsset = value;
            }
            */
        }

        protected override void Awake()
        {
            // base.Awake();

            try
            {
                // if (UIManagerAsset == null) { UIManagerAsset = Resources.Load<UIManager>("MUIP Manager"); }
                Debug.AssertFormat(_ManagerAsset != null, "this.gameObject.name : " + this.gameObject.name);
                // Debug.LogFormat(LOG_FORMAT, "_ManagerAsset.enableDynamicUpdate : " + _ManagerAsset.enableDynamicUpdate);
                /*
                if (buttonManager == null)
                {
                    buttonManager = GetComponent<_MUI_Button>(); 
                }
                */
                if (buttonHandler == null)
                {
                    buttonHandler = GetComponent<_MUI_Button>();
                }

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

        protected virtual void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable(), this.gameObject.name : <b>" + this.gameObject.name + "</b>");
        }

        protected override void Update()
        {
            Debug.Assert(_ManagerAsset != null);
            if (/*_ManagerAsset == null ||*/ this.enabled == false || buttonHandler == null) 
            { 
                return;
            }
            if (_ManagerAsset.enableDynamicUpdate == true)
            { 
                UpdateButton();
            }
        }

        protected override void UpdateButton()
        {
            // Debug.LogFormat(LOG_FORMAT, "UpdateButton(), this.gameObject : " + this.gameObject.name + ", this.enabled : " + this.enabled);

            if (overrideColors == false)
            {
                if (disabledBackground != null)
                { 
                    disabledBackground.color = highlightedBackground.color = new Color(_ManagerAsset.buttonAccentColor.r, _ManagerAsset.buttonAccentColor.g, _ManagerAsset.buttonAccentColor.b, _ManagerAsset.buttonDisabledAlpha); 
                }
                if (normalBackground != null)
                { 
                    normalBackground.color = _ManagerAsset.buttonAccentColor;
                }
                if (highlightedBackground != null)
                { 
                    highlightedBackground.color = _ManagerAsset.buttonAccentColor;
                }
            }

            if (buttonHandler.enableIcon == true && overrideColors == false)
            {
                if (outlineMode == false)
                {
                    if (disabledIcon != null)
                    { 
                        disabledIcon.color = _ManagerAsset.buttonNormalColor;
                    }
                    if (normalIcon != null)
                    { 
                        normalIcon.color = _ManagerAsset.buttonNormalColor;
                    }
                    if (highlightedIcon != null)
                    { 
                        highlightedIcon.color = _ManagerAsset.buttonNormalColor;
                    }
                }

                else
                {
                    if (disabledIcon != null)
                    { 
                        disabledIcon.color = new Color(_ManagerAsset.buttonAccentColor.r, _ManagerAsset.buttonAccentColor.g, _ManagerAsset.buttonAccentColor.b, _ManagerAsset.buttonDisabledAlpha); 
                    }
                    if (normalIcon != null)
                    { 
                        normalIcon.color = _ManagerAsset.buttonAccentColor; 
                    }
                    if (highlightedIcon != null)
                    { 
                        highlightedIcon.color = _ManagerAsset.buttonNormalColor;
                    }
                }
            }

            if (buttonHandler.enableText == true)
            {
                if (overrideColors == false)
                {
                    if (outlineMode == false)
                    {
                        if (disabledText != null)
                        { 
                            disabledText.color = _ManagerAsset.buttonNormalColor;
                        }
                        if (normalText != null)
                        { 
                            normalText.color = _ManagerAsset.buttonNormalColor;
                        }
                        if (highlightedText != null)
                        { 
                            highlightedText.color = _ManagerAsset.buttonNormalColor;
                        }
                    }

                    else
                    {
                        if (disabledText != null)
                        { 
                            disabledText.color = new Color(_ManagerAsset.buttonAccentColor.r, _ManagerAsset.buttonAccentColor.g, _ManagerAsset.buttonAccentColor.b, _ManagerAsset.buttonDisabledAlpha);
                        }
                        if (normalText != null)
                        { 
                            normalText.color = _ManagerAsset.buttonAccentColor;
                        }
                        if (highlightedText != null)
                        { 
                            highlightedText.color = _ManagerAsset.buttonNormalColor;
                        }
                    }
                }

                if (overrideFonts == false)
                {
                    if (disabledText != null)
                    { 
                        disabledText.font = _ManagerAsset.buttonFont;
                    }
                    if (normalText != null)
                    { 
                        normalText.font = _ManagerAsset.buttonFont;
                    }
                    if (highlightedText != null)
                    { 
                        highlightedText.font = _ManagerAsset.buttonFont;
                    }
                }
            }
        }
    }
}
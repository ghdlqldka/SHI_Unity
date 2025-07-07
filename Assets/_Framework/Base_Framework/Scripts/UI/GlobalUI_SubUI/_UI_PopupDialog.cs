using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_PopupDialog : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_PopupDialog]</b></color> {0}";

        public enum _Type
        {
            ONE_BUTTON, //Ok
            TWO_BUTTON, // Yes, No
            THREE_BUTTON, // Cancel, Yes, No
        }

        [Header("Icon")]
        [ReadOnly]
        [SerializeField]
        protected Image iconImage;

        [ReadOnly]
        [SerializeField]
        protected Sprite defaultIconSprite;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected TextMeshProUGUI titleText;
        [ReadOnly]
        [SerializeField]
        protected TextMeshProUGUI messageText;

        [Header("Buttons")]
        [ReadOnly]
        [SerializeField]
        protected Button oneButton;
        [ReadOnly]
        [SerializeField]
        protected Button twoButton;
        [ReadOnly]
        [SerializeField]
        protected Button threeButton;

        public delegate void DialogResultCallback(Button button);
        // protected event DialogResultCallback OnDialogResult;
        protected DialogResultCallback _DialogResult;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(this.gameObject.activeSelf == false);
#if DEBUG
            Image dimImage = this.GetComponent<Image>();
            Debug.Assert(dimImage != null);
            Debug.Assert(dimImage.enabled == true); // Always, dimImage must be ENABLED!!!!!
#endif
        }

        protected virtual void OnEnable()
        {
            //
        }

        protected virtual void OnDisable()
        {
            //
        }

        public virtual void Show(string title, string message, string oneBtnText = "Yes", string twoBtnText = "No", DialogResultCallback result = null)
        {
            Debug.Assert(string.IsNullOrEmpty(oneBtnText) == false && string.IsNullOrEmpty(twoBtnText) == false);

            Show(defaultIconSprite, title, message, oneBtnText, twoBtnText, "", result);
        }

        public virtual void Show(Sprite iconSprite, string title, string message, string oneBtnText = "Yes", string twoBtnText = "No", DialogResultCallback result = null)
        {
            Debug.Assert(string.IsNullOrEmpty(oneBtnText) == false && string.IsNullOrEmpty(twoBtnText) == false);

            Show(iconSprite, title, message, oneBtnText, twoBtnText, "", result);
        }

        public virtual void Show(string title, string message, string oneBtnText, string twoBtnText, string threeBtnText, DialogResultCallback result = null)
        {
            Debug.Assert(string.IsNullOrEmpty(oneBtnText) == false && string.IsNullOrEmpty(twoBtnText) == false);

            Show(defaultIconSprite, title, message, oneBtnText, twoBtnText, threeBtnText, result);
        }

        public virtual void Show(Sprite iconSprite, string title, string message, string oneBtnText, string twoBtnText, string threeBtnText, DialogResultCallback result = null)
        {
            Debug.LogFormat(LOG_FORMAT, "iconSprite : " + iconSprite + ", oneBtnText : " + oneBtnText + ", twoBtnText : " + twoBtnText + ", threeBtnText : " + threeBtnText);

            // Icon
            if (iconSprite != null)
            {
                this.iconImage.sprite = iconSprite;
            }
            this.iconImage.gameObject.SetActive(iconSprite != null);

            // Title
            if (string.IsNullOrEmpty(title) == true)
            {
                this.titleText.gameObject.SetActive(false);
            }
            else
            {
                this.titleText.text = title;
                this.titleText.gameObject.SetActive(true);
            }
            // Message
            this.messageText.text = message;

            // One Button
            Debug.Assert(string.IsNullOrEmpty(oneBtnText) == false);
            oneButton.GetComponent<_MUI_Button>().buttonText = oneBtnText;

            // Two Button
            if (string.IsNullOrEmpty(twoBtnText) == true)
            {
                twoButton.gameObject.SetActive(false);
            }
            else
            {
                twoButton.GetComponent<_MUI_Button>().buttonText = twoBtnText;
                twoButton.gameObject.SetActive(true);
            }

            // Three Button
            if (string.IsNullOrEmpty(threeBtnText) == true)
            {
                threeButton.gameObject.SetActive(false);
            }
            else
            {
                threeButton.GetComponent<_MUI_Button>().buttonText = threeBtnText;
                threeButton.gameObject.SetActive(true);
            }

            _DialogResult = result;

            this.gameObject.SetActive(true);
        }

        public virtual void OnClickOneButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>One</b>Button()");

            if (_DialogResult != null)
            {
                _DialogResult(this.oneButton);
                _DialogResult = null;
            }

            this.gameObject.SetActive(false);
        }

        public virtual void OnClickTwoButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Two</b>Button()");

            if (_DialogResult != null)
            {
                _DialogResult(this.twoButton);
                _DialogResult = null;
            }

            this.gameObject.SetActive(false);
        }

        public virtual void OnClickThreeButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Three</b>Button()");

            if (_DialogResult != null)
            {
                _DialogResult(this.threeButton);
                _DialogResult = null;
            }

            this.gameObject.SetActive(false);
        }
    }
}
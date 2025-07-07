// using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_PopupDialog_Portrait : _UI_PopupDialog
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_PopupDialog_</b></color><color=red><b>Portrait]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(this.gameObject.activeSelf == false);
#if DEBUG
            Image dimImage = this.GetComponent<Image>();
            Debug.Assert(dimImage != null);
            Debug.Assert(dimImage.enabled == true); // Always, dimImage must be ENABLED!!!!!
#endif
        }

        public override void Show(Sprite iconSprite, string title, string message, string oneBtnText, string twoBtnText, string threeBtnText, DialogResultCallback result = null)
        {
            Debug.LogFormat(LOG_FORMAT, "Show(), iconSprite : " + iconSprite + ", oneBtnText : " + oneBtnText + ", twoBtnText : " + twoBtnText + ", threeBtnText : " + threeBtnText);

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
            oneButton.GetComponent<Michsky.MUIP._MUI_Button>().buttonText = oneBtnText;

            // Two Button
            if (string.IsNullOrEmpty(twoBtnText) == true)
            {
                twoButton.gameObject.SetActive(false);
            }
            else
            {
                twoButton.GetComponent<Michsky.MUIP._MUI_Button>().buttonText = twoBtnText;
                twoButton.gameObject.SetActive(true);
            }

            // Three Button
            if (string.IsNullOrEmpty(threeBtnText) == true)
            {
                threeButton.gameObject.SetActive(false);
            }
            else
            {
                threeButton.GetComponent<Michsky.MUIP._MUI_Button>().buttonText = threeBtnText;
                threeButton.gameObject.SetActive(true);
            }

            _DialogResult = result;

            this.gameObject.SetActive(true);
        }
    }
}
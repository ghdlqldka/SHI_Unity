using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Magenta_Framework
{
    public class UI_SubtitleWindowEx : _Base_Framework._UI_SubtitleWindow
    {
        private static string LOG_FORMAT = "<color=white><b>[UI_SubtitleWindowEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(dimImage.enabled == false);
            backgroundImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            // messageText = this.transform.Find("Text").GetComponent<Text>();
        }

        public override void ShowMessage(string message, float duration, FinishCallback finishCallback, bool showNextButton, Sprite iconSprite = null)
        {
            Debug.LogFormat(LOG_FORMAT, "ShowMessage(), message : <b>" + message + "</b>, duration : <b>" + duration + 
                "</b>, showNextButton : <b>" + showNextButton + "</b>");

#if DEBUG
            Debug.Assert(duration >= 0);

            if (string.IsNullOrEmpty(message) == true) // Hide
            {
                Debug.Assert(duration == 0);
            }
            else
            {
                //
            }
#endif

            StopAllCoroutines();

            // _buttonClickCallback = buttonClickCallback;

            if (string.IsNullOrEmpty(message) == false) // Show message
            {
                messageText.text = message;

                FinishCallback _callback = _finishCallback;
                _finishCallback = null;
                if (_callback != null)
                {
                    _callback();
                }
                _finishCallback = finishCallback; // new finish Callback

                if (iconSprite != null)
                {
                    this.iconImage.sprite = iconSprite;
                }
                backgroundImage.gameObject.SetActive(true);
                this.iconImage.gameObject.SetActive(iconSprite != null);
                messageText.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(showNextButton);
                // dimImage.enabled = true;

                StartCoroutine(PostShowMessage(duration));
            }
            else // forcelly hide
            {
                Debug.LogFormat(LOG_FORMAT, "<color=red><b>HIDE</b></color> SubtitleWindow!!!!!");
                Debug.Assert(duration == 0);
                Debug.Assert(finishCallback == null);
                Debug.Assert(showNextButton == false);
                Debug.Assert(iconSprite == null);

                HideMessage();
            }
        }
    }
}
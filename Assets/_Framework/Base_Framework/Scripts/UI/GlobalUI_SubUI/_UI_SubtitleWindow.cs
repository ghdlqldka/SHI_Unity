using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_SubtitleWindow : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_SubtitleWindow]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected Image backgroundImage;

        [ReadOnly]
        [SerializeField]
        protected Image iconImage;

        [ReadOnly]
        [SerializeField]
        protected TextMeshProUGUI messageText;

        [ReadOnly]
        [SerializeField]
        protected Button nextButton;

        public delegate void FinishCallback();
        protected FinishCallback _finishCallback = null;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(dimImage.enabled == false);
            backgroundImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            // messageText = this.transform.Find("Text").GetComponent<Text>();
        }

        protected virtual void OnEnable()
        {
            //
        }

        // duration is 0, means in-finite
        public virtual void ShowMessage(string message, float duration, FinishCallback finishCallback = null, Sprite iconSprite = null)
        {
            // Debug.LogFormat(LOG_FORMAT, "ShowMessage(), duration : " + duration + ", message : " + message);
            Debug.Assert(duration >= 0);
            ShowMessage(message, duration, finishCallback, false, iconSprite);
        }

        public virtual void ShowMessage(string message, float duration, FinishCallback finishCallback, bool showNextButton, Sprite iconSprite = null)
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

        protected virtual IEnumerator PostShowMessage(float duration)
        {
            Debug.Assert(0 <= duration);

            if (duration == 0)
            {
                //
            }
            else
            {
                yield return new WaitForSeconds(duration);

                // dimImage.enabled = false;
                HideMessage();
#if DEBUG
                yield return null; // for Debug.Log()
                Debug.LogFormat(LOG_FORMAT, "=============> <b><color=red>Hide</color></b> SubtitleWindow <=============");
#endif
            }
        }

        protected virtual void HideMessage()
        {
            backgroundImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            // _buttonClickCallback = null;

            FinishCallback _callback = _finishCallback;
            _finishCallback = null;
            if (_callback != null)
            {
                _callback();
            }
        }

        public virtual void OnClickNextButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickNextButton()");

            StopAllCoroutines();

            HideMessage();
        }
    }
}
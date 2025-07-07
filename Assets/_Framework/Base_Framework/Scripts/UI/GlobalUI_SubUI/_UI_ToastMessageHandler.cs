using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_ToastMessageHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[_UI_ToastMessageHandler]</b></color> {0}";

        public enum Duration
        {
            LENGTH_SHORT,
            LENGTH_LONG,
        }

        /*
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Image dimImage;
        */

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Image backgroundImage;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Image iconImage;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected TextMeshProUGUI messageText;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // Debug.Assert(dimImage.enabled == false);
            backgroundImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);

            // messageText = this.transform.Find("Text").GetComponent<Text>();
        }

        protected virtual void OnEnable()
        {
            //
        }

        public virtual void ShowMessage(string message, Duration duration = Duration.LENGTH_SHORT, Sprite iconSprite = null)
        {
            if (duration == Duration.LENGTH_LONG)
            {
                ShowMessage(message, 5, iconSprite);
            }
            else
            {
                ShowMessage(message, 3, iconSprite);
            }
        }

        public virtual void ShowMessage(string message, float duration, Sprite iconSprite = null)
        {
            Debug.LogFormat(LOG_FORMAT, "ShowMessage(), duration : " + duration + ", message : " + message);

            StopAllCoroutines();

            if (string.IsNullOrEmpty(message) == false)
            {
                messageText.text = message;
                if (iconSprite != null)
                {
                    this.iconImage.sprite = iconSprite;
                }
                backgroundImage.gameObject.SetActive(true);
                this.iconImage.gameObject.SetActive(iconSprite != null);
                messageText.gameObject.SetActive(true);
                // dimImage.enabled = true;

                StartCoroutine(PostShow(duration));
            }
            else
            {
                // dimImage.enabled = false;
                backgroundImage.gameObject.SetActive(false);
                this.iconImage.gameObject.SetActive(false);
                messageText.gameObject.SetActive(false);
            }
        }

        protected virtual IEnumerator PostShow(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            // dimImage.enabled = false;
            backgroundImage.gameObject.SetActive(false);
            iconImage.gameObject.SetActive(false);
            messageText.gameObject.SetActive(false);

            Debug.LogFormat(LOG_FORMAT, "=============> <b><color=red>Hide</color></b> Toast Message <=============");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _FadeManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#8DA278><b>[_FadeManager]</b></color> {0}";

        protected static _FadeManager _instance;
        public static _FadeManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Image fadeImage;

        protected Coroutine fadeCoroutine = null;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "Awake()");

                Instance = this;

                // fadeImage = GetComponent<Image>();
                Debug.Assert(fadeImage != null);
                Debug.LogFormat(LOG_FORMAT, "Initial fadeImage alpha : <color=red><b>" + fadeImage.color.a + "</b></color>");
                fadeImage.enabled = false; // <= defaultly disable
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void OnEnable()
        {
            //
        }

        public virtual void Fade(float alpha)
        {
            Debug.Assert(fadeCoroutine == null);

            Color tempColor = fadeImage.color;
            tempColor.a = alpha;
            fadeImage.color = tempColor;

            if (alpha > 0f)
            {
                fadeImage.enabled = true; // enable
            }
            else
            {
                fadeImage.enabled = false;
            }
        }

        public virtual void FadeIn(float fadeInTime = 2, System.Action finishEvent = null)
        {
            Debug.LogFormat(LOG_FORMAT, "FadeIn(), fadeInTime : <b>" + fadeInTime + "</b>");

            if (fadeCoroutine  != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(DoFadeIn(fadeInTime, finishEvent));
        }

        public virtual void FadeOut(float fadeOutTime = 2, System.Action finishEvent = null)
        {
            Debug.LogFormat(LOG_FORMAT, "FadeOut(), fadeOutTime : <b>" + fadeOutTime + "</b>");

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            StartCoroutine(DoFadeOut(fadeOutTime, finishEvent));
        }

        protected virtual IEnumerator DoFadeIn(float fadeInTime, System.Action finishEvent = null)
        {
            fadeImage.enabled = true; // enable

            Color tempColor = fadeImage.color;
            while (tempColor.a < 1f)
            {
                tempColor.a += Time.deltaTime / fadeInTime;
                fadeImage.color = tempColor;

                if (tempColor.a >= 1f)
                {
                    tempColor.a = 1f;
                }

                yield return null;
            }

            fadeImage.color = tempColor;

            if (finishEvent != null)
            {
                finishEvent();
            }
        }

        protected virtual IEnumerator DoFadeOut(float fadeOutTime, System.Action finishEvent = null)
        {
            Color tempColor = fadeImage.color;
            while (tempColor.a > 0f)
            {
                tempColor.a -= Time.deltaTime / fadeOutTime;
                fadeImage.color = tempColor;

                if (tempColor.a <= 0f)
                {
                    tempColor.a = 0f;
                }

                yield return null;
            }

            fadeImage.color = tempColor;
            fadeImage.enabled = false; // <= disable

            fadeCoroutine = null;

            if (finishEvent != null)
            {
                finishEvent();
            }
        }
    }
}
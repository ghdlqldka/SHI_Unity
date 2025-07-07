using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _ScreenCaptureManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#997AC1><b>[_ScreenCaptureManager]</b></color> {0}";

        protected static _ScreenCaptureManager _instance;
        public static _ScreenCaptureManager Instance
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

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "Awake()");

                Instance = this;
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

        public delegate void _ScreenCapturing(bool capturing);
        public event _ScreenCapturing OnScreenCapturing;

        public delegate void ScreenCaptureResultCallback(Texture2D screenshot);
        // public event ScreenCaptureResultCallback OnScreenCaptureResult;

        public virtual IEnumerator DoScreenCapture(ScreenCaptureResultCallback result = null)
        {
            Debug.Assert(_GlobalObjectUtilities.Instance.EnableScreenCapture == true);

            if (OnScreenCapturing != null) // Hide UI
            {
                Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Hide necessary UI");
                OnScreenCapturing(true);
            }

            yield return null; // timing

            Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Capture~~~~");
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            yield return new WaitForEndOfFrame();
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            if (OnScreenCapturing != null) // Recover UI
            {
                Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Recover UI");
                OnScreenCapturing(false);
            }

            if (result != null)
            {
                result(screenshot);
            }
        }

        public virtual IEnumerator DoSelfieCapture(ScreenCaptureResultCallback result = null)
        {
            // Debug.Assert(GlobalObjectUtilities.Instance.enableScreenCapture == true);

            /*
            if (OnScreenCapturing != null) // Hide UI
            {
                Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Hide necessary UI");
                OnScreenCapturing(true);
            }
            */

            yield return null; // timing

            Debug.LogFormat(LOG_FORMAT, "Do<b>SelfieCapture</b>() - Step 2 => Capture~~~~");
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false); //
            yield return new WaitForEndOfFrame();
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();

            /*
            if (OnScreenCapturing != null) // Recover UI
            {
                Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Recover UI");
                OnScreenCapturing(false);
            }
            */

            if (result != null)
            {
                result(screenshot);
            }
        }
    }
}
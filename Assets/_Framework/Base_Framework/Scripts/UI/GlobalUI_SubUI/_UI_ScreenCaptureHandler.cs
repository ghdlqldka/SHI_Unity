using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#elif PLATFORM_IOS
using UnityEngine.iOS;
#endif

namespace _Base_Framework
{
    public class _UI_ScreenCaptureHandler : MonoBehaviour
    {
        // private static string LOG_FORMAT = "<color=#CD9A9A><b>[UI_ScreenCaptureHandler]</b></color> {0}";
        private static string LOG_FORMAT = "<color=white><b>[_UI_ScreenCaptureHandler]</b></color> {0}";

        protected Texture2D capturedTexture = null;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected RawImage rawImage;

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Button captureButton;

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected GameObject screenshotPanel;

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _UI_CameraFlash flash;

        protected virtual bool IsValidConfig
        {
            get
            {
                bool valid = true;

                if (rawImage == null || captureButton == null || screenshotPanel == null || flash == null)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "Check UnityEditor Inspector!!!!, Is all variable is set Properly?");
                    valid = false;
                }

                return valid;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (IsValidConfig == false)
            {
                Debug.Assert(false);
            }

#if UNITY_ANDROID
            // Write
            if (_PermissionManager.HasUserAuthorizedPermission(_PermissionManager.ExternalStorageWrite) == false)
            {
                _PermissionManager.RequestUserPermission(_PermissionManager.ExternalStorageWrite);
            }

            // Read
            if (_PermissionManager.HasUserAuthorizedPermission(_PermissionManager.ExternalStorageRead) == false)
            {
                _PermissionManager.RequestUserPermission(_PermissionManager.ExternalStorageRead);
            }
#endif
            // GlobalObjectUtilities.Instance.enableScreenCapture
        }

        protected virtual void OnEnable()
        {
            // captureButton.gameObject.SetActive(true);
            screenshotPanel.SetActive(false);

            capturedTexture = null;

            StartCoroutine(PostOnEnable());
        }

        protected virtual IEnumerator PostOnEnable()
        {
            while (_GlobalObjectUtilities.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            if (_GlobalObjectUtilities.Instance.EnableScreenCapture == false)
            {
                captureButton.gameObject.SetActive(false);
            }
        }

        public virtual void OnClickCaptureButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Capture</b>Button() - Step 1");

            Debug.Assert(capturedTexture == null);

            // Hide all UI
            captureButton.gameObject.SetActive(false);
            screenshotPanel.SetActive(false);

            _UI_GlobalHandler.Instance.Toast.ShowMessage(""); // Hide Toast message

            StartCoroutine(StartCapture());
        }

        protected IEnumerator StartCapture()
        {
            Debug.Assert(_GlobalObjectUtilities.Instance.EnableScreenCapture == true);

            flash.Flash();

            while (_UI_CameraFlash.isRunning) //wait until flash finish
            {
                yield return new WaitForSeconds(0);
            }

            StartCoroutine(_ScreenCaptureManager.Instance.DoScreenCapture(DoneCapture));
        }

        protected void DoneCapture(Texture2D capturedTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "<color=red><b>Done</b></color>Capture() - Step 3");

            this.capturedTexture = capturedTexture;
            if (this.capturedTexture != null)
            {
                rawImage.texture = this.capturedTexture;
                screenshotPanel.SetActive(true);

                captureButton.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "<color=red><b>Fail</b></color> to Capture!!!!! - Step 4");
            }
        }

        public void ShowCaptureButton(bool show)
        {
            Debug.LogFormat(LOG_FORMAT, "Show<b>CaptureButton</b>(), show : <b>" + show + "</b>, EnableScreenCapture : <b>" + _GlobalObjectUtilities.Instance.EnableScreenCapture + "</b>");

            if (_GlobalObjectUtilities.Instance.EnableScreenCapture == true)
            {
                captureButton.gameObject.SetActive(show);
            }
            else
            {
                Debug.Assert(false); // Is it possible?
            }
        }

        public virtual void OnClickShareButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Share</b>Button()");

            rawImage.texture = null;
            screenshotPanel.SetActive(false);
            captureButton.gameObject.SetActive(true);

            capturedTexture = null;
        }

        public virtual void OnClickSaveButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Save</b>Button()");

            rawImage.texture = null;
            screenshotPanel.SetActive(false);
            captureButton.gameObject.SetActive(true);

            capturedTexture = null;
        }

        public virtual void OnClickDiscardButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Discard</b>Button()");

            rawImage.texture = null;
            screenshotPanel.SetActive(false);
            captureButton.gameObject.SetActive(true);

            capturedTexture = null;
        }
    }
}
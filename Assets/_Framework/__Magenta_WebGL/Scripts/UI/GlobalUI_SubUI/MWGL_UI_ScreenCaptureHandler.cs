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

namespace _Magenta_WebGL
{
    public class MWGL_UI_ScreenCaptureHandler : _Magenta_Framework.UI_ScreenCaptureHandlerEx
    {
        private static string LOG_FORMAT = "<color=white><b>[MWGL_UI_ScreenCaptureHandler]</b></color> {0}";

        protected override void Awake()
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
    }
}
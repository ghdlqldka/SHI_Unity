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

namespace _Magenta_Framework
{
    public class UI_ScreenCaptureHandlerEx : _Base_Framework._UI_ScreenCaptureHandler
    {
        private static string LOG_FORMAT = "<color=white><b>[UI_ScreenCaptureHandlerEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (IsValidConfig == false)
            {
                Debug.Assert(false);
            }

#if UNITY_ANDROID
            // Write
            if (PermissionManagerEx.HasUserAuthorizedPermission(PermissionManagerEx.ExternalStorageWrite) == false)
            {
                PermissionManagerEx.RequestUserPermission(PermissionManagerEx.ExternalStorageWrite);
            }

            // Read
            if (PermissionManagerEx.HasUserAuthorizedPermission(PermissionManagerEx.ExternalStorageRead) == false)
            {
                PermissionManagerEx.RequestUserPermission(PermissionManagerEx.ExternalStorageRead);
            }
#endif
            // GlobalObjectUtilitiesEx.Instance.enableScreenCapture
        }
    }
}
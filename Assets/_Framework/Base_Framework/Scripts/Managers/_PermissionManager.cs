using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#elif PLATFORM_IOS
using UnityEngine.iOS;
#endif

namespace _Base_Framework
{
    public class _PermissionManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#4C64FF><b>[_PermissionManager]</b></color> {0}";

        protected static _PermissionManager _instance;
        public static _PermissionManager Instance
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

#if PLATFORM_ANDROID
        //
        // Summary:
        //     Used when requesting permission or checking if permission has been granted to
        //     use the camera.
        public const string Camera = Permission.Camera;
        //
        // Summary:
        //     Used when requesting permission or checking if permission has been granted to
        //     use the microphone.
        public const string Microphone = Permission.Microphone;
        //
        // Summary:
        //     Used when requesting permission or checking if permission has been granted to
        //     use the users location with high precision.
        public const string FineLocation = Permission.FineLocation;
        //
        // Summary:
        //     Used when requesting permission or checking if permission has been granted to
        //     use the users location with coarse granularity.
        public const string CoarseLocation = Permission.CoarseLocation;
        //
        // Summary:
        //     Used when requesting permission or checking if permission has been granted to
        //     read from external storage such as a SD card.
        public const string ExternalStorageRead = Permission.ExternalStorageRead;
        //
        // Summary:
        //     Used when requesting permission or checking if permission has been granted to
        //     write to external storage such as a SD card.
        public const string ExternalStorageWrite = Permission.ExternalStorageWrite;

        // protected PermissionCallbacks _callbacks = new PermissionCallbacks();
#endif

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

#if PLATFORM_ANDROID
                //
#elif PLATFORM_IOS
                //
#else
                Destroy(this.gameObject); // <=========================
#endif
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

        protected virtual void Start()
        {
#if PLATFORM_ANDROID
            PermissionCallbacks _callbacks = new PermissionCallbacks();
            _callbacks.PermissionGranted += OnPermissionGranted;
            _callbacks.PermissionDenied += OnPermissionDenied;
            _callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedAndDontAskAgain;

            // Write
            if (HasUserAuthorizedPermission(ExternalStorageWrite) == false)
            {
                RequestUserPermission(ExternalStorageWrite, _callbacks);
            }

            // Read
            if (HasUserAuthorizedPermission(ExternalStorageRead) == false)
            {
                RequestUserPermission(ExternalStorageRead, _callbacks);
            }
#endif
        }

        protected virtual void OnPermissionGranted(string permissionName)
        {
            // throw new System.NotImplementedException();
            Debug.LogFormat(LOG_FORMAT, "OnPermissionGranted(), permissionName : " + permissionName);
        }

        protected virtual void OnPermissionDenied(string permissionName)
        {
            // throw new System.NotImplementedException();
            Debug.LogFormat(LOG_FORMAT, "OnPermissionDenied(), permissionName : " + permissionName);
        }

        protected virtual void OnPermissionDeniedAndDontAskAgain(string permissionName)
        {
            // throw new System.NotImplementedException();
            Debug.LogFormat(LOG_FORMAT, "OnPermissionDeniedAndDontAskAgain(), permissionName : " + permissionName);
        }

        public static bool HasUserAuthorizedPermission(string permission)
        {
#if PLATFORM_ANDROID
            return Permission.HasUserAuthorizedPermission(permission);
#else
            return false;
#endif
        }

        public static void RequestUserPermission(string permission)
        {
#if PLATFORM_ANDROID
            if (HasUserAuthorizedPermission(permission) == false)
            {
                Permission.RequestUserPermission(permission);
            }
#endif
        }

#if PLATFORM_ANDROID
        public static void RequestUserPermission(string permission, PermissionCallbacks callbacks)
        {
            if (HasUserAuthorizedPermission(permission) == false)
            {
                Permission.RequestUserPermission(permission, callbacks);
            }
        }

        public static void RequestUserPermissions(string[] permissions, PermissionCallbacks callbacks)
        {
            // if (HasUserAuthorizedPermission(permission) == false)
            {
                Permission.RequestUserPermissions(permissions, callbacks);
            }
        }
#endif
    }
}
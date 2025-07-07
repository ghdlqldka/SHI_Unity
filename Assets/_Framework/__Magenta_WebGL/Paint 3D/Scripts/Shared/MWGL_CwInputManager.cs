using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace _Magenta_WebGL
{
	/// <summary>This component combines finger and mouse and keyboard inputs into a single interface.</summary>
	// [HelpURL(CwShared.HelpUrlPrefix + "CwInputManager")]
	// [AddComponentMenu(CwShared.ComponentMenuPrefix + "Input Manager")]
	public class MWGL_CwInputManager : CW.Common._CwInputManager
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[MWGL_CwInputManager]</b></color> {0}";

        public static new void EnsureThisComponentExists()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "EnsureThisComponentExists()");

            if (Application.isPlaying == true && CW.Common.CwHelper.FindAnyObjectByType<MWGL_CwInputManager>() == null)
            {
                new GameObject(typeof(MWGL_CwInputManager).Name).AddComponent<MWGL_CwInputManager>();
            }
        }

        public static new MWGL_CwInputManager Instance
        {
            get
            {
                return _instance as MWGL_CwInputManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}
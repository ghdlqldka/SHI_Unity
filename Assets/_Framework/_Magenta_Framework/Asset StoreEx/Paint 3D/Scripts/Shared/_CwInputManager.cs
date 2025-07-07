using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace CW.Common
{
	/// <summary>This component combines finger and mouse and keyboard inputs into a single interface.</summary>
	// [HelpURL(CwShared.HelpUrlPrefix + "CwInputManager")]
	// [AddComponentMenu(CwShared.ComponentMenuPrefix + "Input Manager")]
	public class _CwInputManager : CwInputManager
    {
        private static string LOG_FORMAT = "<color=#F3940F><b>[_CwInputManager]</b></color> {0}";

        public static new void EnsureThisComponentExists()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "EnsureThisComponentExists()");

            if (Application.isPlaying == true && CwHelper.FindAnyObjectByType<_CwInputManager>() == null)
            {
                new GameObject(typeof(_CwInputManager).Name).AddComponent<_CwInputManager>();
            }
        }

        protected static _CwInputManager _instance;
        public static _CwInputManager Instance
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

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}
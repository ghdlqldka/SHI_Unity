using System.Collections;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.MUIP
{
    public class _MUI_TooltipHandler : TooltipManager
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_TooltipHandler]</b></color> {0}";

        protected static _MUI_TooltipHandler _instance;
        public static _MUI_TooltipHandler Instance
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

        [SerializeField]
        protected Camera _camera;

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                Debug.Assert(_camera != null);

                base.Awake();
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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
using Screen = UnityEngine.Device.Screen; // To support Device Simulator on Unity 2021.1+
#endif

namespace IngameDebugConsole
{
	public class _UI_DebugLogWindow : MonoBehaviour
	{
        private static string LOG_FORMAT = "<color=white><b>[_UI_DebugLogWindow]</b></color> {0}";

        [SerializeField]
        protected _UI_DebugLogRecycledListView _ui_RecycledListView;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        public void Show()
        {
            //
        }

        // Hide the popup
        public void Hide()
        {
            //
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
#if UNITY_EDITOR && UNITY_2021_1_OR_NEWER
using Screen = UnityEngine.Device.Screen; // To support Device Simulator on Unity 2021.1+
#endif

// Manager class for the debug popup
namespace IngameDebugConsole
{
	public class _UI_DebugLogPopup : DebugLogPopup
	{
        private static string LOG_FORMAT = "<color=white><b>[_UI_DebugLogPopup]</b></color> {0}";

        // protected DebugLogManager debugManager;
        protected _DebugLogManager _debugManager
        {
            get 
            { 
                return debugManager as _DebugLogManager;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            popupTransform = (RectTransform)transform;
            backgroundImage = GetComponent<Image>();
            canvasGroup = GetComponent<CanvasGroup>();

            normalColor = backgroundImage.color;

            halfSize = popupTransform.sizeDelta * 0.5f;

            Vector2 pos = popupTransform.anchoredPosition;
            if (pos.x != 0f || pos.y != 0f)
                normalizedPosition = pos.normalized; // Respect the initial popup position set in the prefab
            else
                normalizedPosition = new Vector2(0.5f, 0f); // Right edge by default
        }

        public override void OnPointerClick(PointerEventData data)
        {
            Debug.LogFormat(LOG_FORMAT, "OnPointerClick()");

            // Hide the popup and show the log window
            if (isPopupBeingDragged == false)
            {
                _debugManager.ShowLogWindow();
            }
        }
    }
}
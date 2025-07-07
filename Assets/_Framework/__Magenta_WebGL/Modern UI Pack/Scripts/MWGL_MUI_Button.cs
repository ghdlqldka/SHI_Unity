using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _Magenta_WebGL
{
    [RequireComponent(typeof(Button))]
    public class MWGL_MUI_Button : _Magenta_Framework.MUI_ButtonEx
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MWGL_MUI_Button]</b></color> {0}";

        public override void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogFormat(LOG_FORMAT, "<color=yellow>" + this.gameObject.name + "</color>-OnPointerClick()");

            if (!isInteractable || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            if (enableButtonSounds && useClickSound == true && soundSource != null)
            {
                soundSource.PlayOneShot(clickSound);
            }

            // Invoke click actions
            onClick.Invoke();

            // Check for double click
            if (checkForDoubleClick == false || !gameObject.activeInHierarchy)
            {
                return;
            }
            if (waitingForDoubleClickInput == true)
            {
                onDoubleClick.Invoke();
                waitingForDoubleClickInput = false;
                return;
            }

            waitingForDoubleClickInput = true;

            StopCoroutine("CheckForDoubleClick");
            StartCoroutine("CheckForDoubleClick");
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.MUIP
{
    public class _MUI_DropdownList : CustomDropdown
    {
        private static string LOG_FORMAT = "<color=#A6CAEF><b>[_MUI_DropdownList]</b></color> {0}";

        public override void OnPointerClick(PointerEventData eventData)
        {
            Debug.LogFormat(LOG_FORMAT, "OnPointerClick()");
            // base.OnPointerClick(eventData);

            if (isInteractable == false)
            { 
                return; 
            }

            if (enableDropdownSounds && useClickSound)
            { 
                soundSource.PlayOneShot(clickSound); 
            }

            Animate();
        }

        public virtual void OnItemChanged(int i)
        {
            Debug.LogFormat(LOG_FORMAT, "OnItemChanged(), i : <b>" + i + "</b>");
        }
    }
}
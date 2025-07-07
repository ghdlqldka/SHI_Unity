using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using Michsky.MUIP;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _Magenta_Framework
{
    // [AddComponentMenu("Modern UI Pack/Context Menu/Context Menu Content")]
    public class MUI_ContextMenuAreaEx : Michsky.MUIP._MUI_ContextMenuArea
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_ContextMenuAreaEx]</b></color> {0}";

        protected override void Awake()
        {
            if (contextMenu == null)
            {
                try
                {
#if UNITY_2023_2_OR_NEWER
                    contextMenu = FindObjectsByType<_MUI_ContextMenu>(FindObjectsSortMode.None)[0];
                    Debug.LogFormat(LOG_FORMAT, "contextMenu.gameObject : <b>" + contextMenu.gameObject.name + "</b>");
#else
                    contextMenu = (ContextMenuManager)FindObjectsOfType(typeof(ContextMenuManager))[0];
#endif
                    Debug.Assert(contextMenu.ButtonPrefab);
                    Debug.Assert(contextMenu.SeparatorPrefab);
                    Debug.Assert(contextMenu.SubMenuPrefab);
                    Debug.Assert(contextMenu.ItemParent);

                    // itemParent = contextMenu.transform.Find("Content/Item List").transform;
                    itemParent = contextMenu.ItemParent;
                }

                catch
                {
                    Debug.LogError("<b>[Context Menu]</b> Context Manager is missing.", this);
                    return;
                }
            }

            foreach (Transform child in itemParent)
            {
                Destroy(child.gameObject);
            }

            selectedItem = null; // Not used!!!!!
        }
    }
}
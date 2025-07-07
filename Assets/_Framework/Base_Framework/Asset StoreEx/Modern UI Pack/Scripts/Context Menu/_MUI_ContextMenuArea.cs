using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.MUIP
{
    // [AddComponentMenu("Modern UI Pack/Context Menu/Context Menu Content")]
    public class _MUI_ContextMenuArea : ContextMenuContent
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_ContextMenuArea]</b></color> {0}";

        // public ContextMenuManager contextManager;
        public _MUI_ContextMenu contextMenu
        {
            get
            {
                return contextManager as _MUI_ContextMenu;
            }
            protected set
            {
                contextManager = value;
            }
        }

        protected override void Awake()
        {
            // base.Awake();

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

        public override void OnPointerClick(PointerEventData eventData)
        {
            // base.OnPointerClick(eventData);

            if (contextMenu.IsOn == true)
            {
                contextMenu.Close();
            }
            else if (eventData.button == PointerEventData.InputButton.Right && contextMenu.IsOn == false)
            { 
                ProcessContent();
            }
        }

        public override void ProcessContent()
        {
            // base.ProcessContent();

            foreach (Transform child in itemParent)
            { 
                Destroy(child.gameObject); 
            }

            GameObject itemPrefab = null;
            for (int i = 0; i < contexItems.Count; ++i)
            {
                // bool nulLVariable = false;

                if (contexItems[i].contextItemType == ContextItemType.Button/* && contextMenu.ButtonPrefab != null*/)
                    itemPrefab = contextMenu.ButtonPrefab;
                else if (contexItems[i].contextItemType == ContextItemType.Separator/* && contextMenu.SeparatorPrefab != null*/)
                    itemPrefab = contextMenu.SeparatorPrefab;
                else
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "<b>[Context Menu]</b> At least one of the item presets is missing. " +
                        "You can assign a new variable in Resources (Context Menu) tab. All default presets can be found in " +
                        "<b>Modern UI Pack > Prefabs > Context Menu</b> folder.", this);
                    // nulLVariable = true;
                    itemPrefab = null;
                }

                // if (nulLVariable == false)
                {
                    if (contexItems[i].subMenuItems.Count == 0)
                    {
                        GameObject go = Instantiate(itemPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                        go.transform.SetParent(itemParent, false);

                        if (contexItems[i].contextItemType == ContextItemType.Button)
                        {
                            setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                            textHelper = contexItems[i].itemText;
                            setItemText.text = textHelper;

                            Transform goImage = go.gameObject.transform.Find("Icon");
                            setItemImage = goImage.GetComponent<Image>();
                            imageHelper = contexItems[i].itemIcon;
                            setItemImage.sprite = imageHelper;

                            if (imageHelper == null)
                                setItemImage.color = new Color(0, 0, 0, 0);

                            Button itemButton = go.GetComponent<Button>();
                            // itemButton.onClick.AddListener(contexItems[i].onClick.Invoke);
                            ContextItem item = contexItems[i];
                            itemButton.onClick.AddListener(() => OnClickItem(item));
                            // () => OnClickItem(item)
                            itemButton.onClick.AddListener(contextMenu.Close);
                        }
                    }

                    else if (contextMenu.SubMenuPrefab != null && contexItems[i].subMenuItems.Count != 0)
                    {
                        GameObject go = Instantiate(contextMenu.SubMenuPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                        go.transform.SetParent(itemParent, false);

                        _MUI_ContextMenuSubMenu subMenu = go.GetComponent<_MUI_ContextMenuSubMenu>();
                        // subMenu.cmManager = contextMenu;
                        subMenu.contextMenu = contextMenu;
                        // subMenu.cmContent = this;
                        subMenu.contextMenuArea = this;
                        subMenu.subMenuIndex = i;

                        setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                        textHelper = contexItems[i].itemText;
                        setItemText.text = textHelper;

                        Transform goImage;
                        goImage = go.gameObject.transform.Find("Icon");
                        setItemImage = goImage.GetComponent<Image>();
                        imageHelper = contexItems[i].itemIcon;
                        setItemImage.sprite = imageHelper;
                    }

                    StopCoroutine("ExecuteAfterTime");
                    StartCoroutine("ExecuteAfterTime", 0.01f);
                }
            }

            contextMenu.SetContextMenuPosition();
            contextMenu.Open();
        }

        public override void AddNewItem()
        {
            // base.AddNewItem();

            ContextItem item = new ContextItem();
            contexItems.Add(item);
        }

        public virtual void OnClickItem(ContextItem item)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "1. OnClickItem(), item.itemText : <color=yellow><b>" + item.itemText + "</b></color>");
        }

        public virtual void OnClickItem(SubMenuItem item)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "2. OnClickItem(), item.itemText : <color=cyan><b>" + item.itemText + "</b></color>");
        }
    }
}
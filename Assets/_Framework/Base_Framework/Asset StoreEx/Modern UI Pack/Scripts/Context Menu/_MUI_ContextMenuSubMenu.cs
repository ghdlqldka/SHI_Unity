using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using static Michsky.MUIP.ContextMenuContent;

namespace Michsky.MUIP
{
    public class _MUI_ContextMenuSubMenu : ContextMenuSubMenu
    {
        // public ContextMenuManager cmManager;
        public _MUI_ContextMenu contextMenu
        {
            get
            {
                return cmManager as _MUI_ContextMenu;
            }
            set
            {
                cmManager = value;
            }
        }
        public _MUI_ContextMenuArea contextMenuArea
        {
            get
            {
                return cmContent as _MUI_ContextMenuArea;
            }
            set
            {
                cmContent = value;
            }
        }
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (contextMenu.subMenuBehaviour == ContextMenuManager.SubMenuBehaviour.Click)
            {
                if (subMenuAnimator.GetCurrentAnimatorStateInfo(0).IsName("Menu In"))
                {
                    subMenuAnimator.Play("Menu Out");
                    if (trigger != null)
                    {
                        trigger.SetActive(false);
                    }
                }

                else
                {
                    subMenuAnimator.Play("Menu In");
                    if (trigger != null) 
                    { 
                        trigger.SetActive(true);
                    }
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            foreach (Transform child in itemParent)
                Destroy(child.gameObject);

            for (int i = 0; i < contextMenuArea.contexItems[subMenuIndex].subMenuItems.Count; ++i)
            {
                bool nulLVariable = false;

                if (contextMenuArea.contexItems[subMenuIndex].subMenuItems[i].contextItemType == ContextMenuContent.ContextItemType.Button/* && contextMenu.contextButton != null*/)
                    selectedItem = contextMenu.contextButton;
                else if (contextMenuArea.contexItems[subMenuIndex].subMenuItems[i].contextItemType == ContextMenuContent.ContextItemType.Separator/* && contextMenu.contextSeparator != null*/)
                    selectedItem = contextMenu.contextSeparator;
                else
                {
                    Debug.LogError("<b>[Context Menu]</b> At least one of the item presets is missing. " +
                        "You can assign a new variable in Resources (Context Menu) tab. All default presets can be found in " +
                        "<b>Modern UI Pack > Prefabs > Context Menu</b> folder.", this);
                    nulLVariable = true;
                }

                if (nulLVariable == false)
                {
                    GameObject go = Instantiate(selectedItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    go.transform.SetParent(itemParent, false);

                    if (contextMenuArea.contexItems[subMenuIndex].subMenuItems[i].contextItemType == ContextMenuContent.ContextItemType.Button)
                    {
                        setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                        textHelper = contextMenuArea.contexItems[subMenuIndex].subMenuItems[i].itemText;
                        setItemText.text = textHelper;

                        Transform goImage = go.gameObject.transform.Find("Icon");
                        setItemImage = goImage.GetComponent<Image>();
                        imageHelper = contextMenuArea.contexItems[subMenuIndex].subMenuItems[i].itemIcon;
                        setItemImage.sprite = imageHelper;

                        if (imageHelper == null)
                            setItemImage.color = new Color(0, 0, 0, 0);

                        Button itemButton = go.GetComponent<Button>();
                        // itemButton.onClick.AddListener(contextMenuArea.contexItems[subMenuIndex].subMenuItems[i].onClick.Invoke);
                        SubMenuItem item = contextMenuArea.contexItems[subMenuIndex].subMenuItems[i];
                        itemButton.onClick.AddListener(() => contextMenuArea.OnClickItem(item));

                        itemButton.onClick.AddListener(CloseOnClick);
                        StartCoroutine(ExecuteAfterTime(0.01f));
                    }
                }
            }

            Debug.Assert(contextMenu.autoSubMenuPosition == true);
            // if (contextMenu.autoSubMenuPosition == true)
            {
                if (contextMenu.horizontalBound == ContextMenuManager.CursorBoundHorizontal.Left) 
                { 
                    listParent.pivot = new Vector2(0f, listParent.pivot.y);
                }
                else if (contextMenu.horizontalBound == ContextMenuManager.CursorBoundHorizontal.Right)
                { 
                    listParent.pivot = new Vector2(1f, listParent.pivot.y);
                }

                if (contextMenu.verticalBound == ContextMenuManager.CursorBoundVertical.Top)
                { 
                    listParent.pivot = new Vector2(listParent.pivot.x, 0f);
                }
                else if (contextMenu.verticalBound == ContextMenuManager.CursorBoundVertical.Bottom) 
                { 
                    listParent.pivot = new Vector2(listParent.pivot.x, 1f);
                }
            }

            if (contextMenu.subMenuBehaviour == ContextMenuManager.SubMenuBehaviour.Hover)
                subMenuAnimator.Play("Menu In");
        }

        public override void CloseOnClick()
        {
            contextMenu._animator.Play("Menu Out");
            contextMenu.IsOn = false;
            trigger.SetActive(false);
        }
    }
}
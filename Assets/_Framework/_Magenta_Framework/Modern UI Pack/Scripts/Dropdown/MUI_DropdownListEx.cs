using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace _Magenta_Framework
{
    public class MUI_DropdownListEx : Michsky.MUIP._MUI_DropdownList
    {
        // private static string LOG_FORMAT = "<color=#A6CAEF><b>[CustomDropdownEx]</b></color> {0}";

#if false //
        protected override void Awake()
        {
            enableTrigger = true; // forcelly set!!!!!
            Debug.Assert(triggerObject != null);

            isListItem = true; // forcelly set!!!!!

            enableDropdownSounds = true; // forcelly set
            useHoverSound = false; // forcelly set
            hoverSound = null; // forcelly set
            useClickSound = true; // forcelly set

            saveSelected = false; // forcelly set
        }

        protected override void Start()
        {
            try
            {
                dropdownAnimator = gameObject.GetComponent<Animator>();
                itemList = itemParent.GetComponent<VerticalLayoutGroup>();

                if (dropdownItems.Count != 0)
                    SetupDropdown();

                currentListParent = transform.parent;

                if (enableTrigger == true && triggerObject != null)
                {
                    triggerButton = gameObject.GetComponent<Button>();
                    triggerEvent = triggerObject.AddComponent<EventTrigger>();
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerClick;
                    entry.callback.AddListener((eventData) => { OnClickDropdownList(); });
                    triggerEvent.GetComponent<EventTrigger>().triggers.Add(entry);
                }
            }
            catch
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Cannot initalize the object due to missing resources.", this);
            }

            if (enableScrollbar == true)
                itemList.padding.right = 25;
            else
                itemList.padding.right = 8;

            if (setHighPriorty == true)
                transform.SetAsLastSibling();

            Debug.Assert(saveSelected == false);
        }

        public override void SetupDropdown()
        {
            foreach (Transform child in itemParent)
            {
                GameObject.Destroy(child.gameObject);
            }

            index = 0;

            for (int i = 0; i < dropdownItems.Count; ++i)
            {
                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = dropdownItems[i].itemName;
                setItemText.text = textHelper;

                Transform goImage;
                goImage = go.gameObject.transform.Find("Icon");
                setItemImage = goImage.GetComponent<Image>();
                imageHelper = dropdownItems[i].itemIcon;
                setItemImage.sprite = imageHelper;

                Button itemButton;
                itemButton = go.GetComponent<Button>();

                // itemButton.onClick.AddListener(Animate);
                itemButton.onClick.AddListener(OnClickDropdownList);
                itemButton.onClick.AddListener(delegate
                {
                    ChangeDropdownInfo(index = go.transform.GetSiblingIndex());
                    dropdownEvent.Invoke(index = go.transform.GetSiblingIndex());

                    Debug.Assert(saveSelected == false);
                });

                if (dropdownItems[i].OnItemSelection != null)
                    itemButton.onClick.AddListener(dropdownItems[i].OnItemSelection.Invoke);

                if (invokeAtStart == true)
                    dropdownItems[i].OnItemSelection.Invoke();
            }

            try
            {
                selectedText.text = dropdownItems[selectedItemIndex].itemName;
                selectedImage.sprite = dropdownItems[selectedItemIndex].itemIcon;
                currentListParent = transform.parent;
            }
            catch
            {
                selectedText.text = dropdownTag;
                currentListParent = transform.parent;
                Debug.Log("Dropdown - There is no dropdown items in the list.", this);
            }
        }

        public override void OnClickDropdownList()
        {
            Debug.Assert(isListItem == true);
            Debug.Assert(enableTrigger == true);
            if (isOn == false && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading In");
                isOn = true;

                siblingIndex = transform.GetSiblingIndex();
                gameObject.transform.SetParent(listParent, true);
            }

            else if (isOn == true && animationType == AnimationType.FADING)
            {
                dropdownAnimator.Play("Fading Out");
                isOn = false;

                gameObject.transform.SetParent(currentListParent, true);
                gameObject.transform.SetSiblingIndex(siblingIndex);
            }

            else if (isOn == false && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding In");
                isOn = true;

                siblingIndex = transform.GetSiblingIndex();
                gameObject.transform.SetParent(listParent, true);
            }

            else if (isOn == true && animationType == AnimationType.SLIDING)
            {
                dropdownAnimator.Play("Sliding Out");
                isOn = false;

                gameObject.transform.SetParent(currentListParent, true);
                gameObject.transform.SetSiblingIndex(siblingIndex);
            }

            else if (isOn == false && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish In");
                isOn = true;

                siblingIndex = transform.GetSiblingIndex();
                gameObject.transform.SetParent(listParent, true);
            }

            else if (isOn == true && animationType == AnimationType.STYLISH)
            {
                dropdownAnimator.Play("Stylish Out");
                isOn = false;

                gameObject.transform.SetParent(currentListParent, true);
                gameObject.transform.SetSiblingIndex(siblingIndex);
            }

            if (setHighPriorty == true)
                transform.SetAsLastSibling();

            Debug.Assert(enableTrigger == true);
            if (isOn == false)
            {
                triggerObject.SetActive(false);
                triggerButton.interactable = true;
            }

            else if (isOn == true)
            {
                triggerObject.SetActive(true);
                triggerButton.interactable = false;
            }

            if (outOnPointerExit == true)
            {
                triggerObject.SetActive(false);
                triggerButton.interactable = true;
            }
        }
#endif
    }
}
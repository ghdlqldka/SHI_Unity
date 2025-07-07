using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

namespace Michsky.MUIP
{
    public class _MUI_HorizontalSelector : HorizontalSelector
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_HorizontalSelector]</b></color> {0}";

        // public List<Item> items = new List<Item>();
        public List<Item> itemList
        {
            get
            {
                return items;
            }
        }

        // public int index = 0;
        public int Index
        {
            get
            {
                return index;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>" +
                ", invokeAtStart : <b><color=yellow>" + invokeAtStart + "</color></b>");
            // base.Awake();

            if (selectorAnimator == null) 
            { 
                selectorAnimator = this.gameObject.GetComponent<Animator>();
            }
            if (label == null || labelHelper == null)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Cannot initalize the object due to missing resources.", this);
                return;
            }

            SetupSelector();
            UpdateContentLayout();

            if (invokeAtStart == true)
            {
                Debug.LogFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@");
                Invoke_OnItemSelect();
            }
        }

        public void Invoke_OnItemSelect()
        {
            Debug.LogFormat(LOG_FORMAT, "Invoke_OnItemSelect(), Index : <color=yellow>" + Index + "</color>");

            items[Index].onItemSelect.Invoke();
            onValueChanged.Invoke(Index);
        }

        protected override void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable(), this.gameObject : <b>" + this.gameObject.name + "</b>");
            // base.OnEnable();

            if (this.gameObject.activeInHierarchy == true)
            { 
                StartCoroutine(DisableAnimator());
            }
        }

        protected virtual void OnDisable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnDisable()");
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");
        }

        public override void SetupSelector()
        {
            Debug.LogFormat(LOG_FORMAT, "SetupSelector(), saveSelected : <b>" + saveSelected + "</b>");
            // base.SetupSelector();

            if (items.Count == 0)
            {
                return;
            }

            if (saveSelected == true)
            {
                if (PlayerPrefs.HasKey("HorizontalSelector_" + saveKey))
                { 
                    defaultIndex = PlayerPrefs.GetInt("HorizontalSelector_" + saveKey);
                }
                else 
                { 
                    PlayerPrefs.SetInt("HorizontalSelector_" + saveKey, defaultIndex);
                }
            }

            label.text = items[defaultIndex].itemTitle;
            labelHelper.text = label.text;
            onItemTextChanged?.Invoke(label);

            if (labelIcon != null && enableIcon)
            {
                labelIcon.sprite = items[defaultIndex].itemIcon;
                labelIconHelper.sprite = labelIcon.sprite;
            }

            else if (enableIcon == false)
            {
                if (labelIcon != null)
                {
                    labelIcon.gameObject.SetActive(false);
                }
                if (labelIconHelper != null)
                { 
                    labelIconHelper.gameObject.SetActive(false);
                }
            }

            Debug.LogWarningFormat(LOG_FORMAT, "defaultIndex : <b><color=yellow>" + defaultIndex + "</color></b>");
            index = defaultIndex;

            if (enableIndicators) 
            { 
                UpdateIndicators();
            }
            else if (indicatorParent != null)
            {
                Destroy(indicatorParent.gameObject);
            }
        }

        public override void UpdateContentLayout()
        {
            // base.UpdateContentLayout();

            if (contentLayout != null) 
            { 
                contentLayout.spacing = contentSpacing;
            }
            if (contentLayoutHelper != null) 
            { 
                contentLayoutHelper.spacing = contentSpacing;
            }
            if (labelIcon != null)
            {
                labelIcon.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
                labelIconHelper.transform.localScale = new Vector3(iconScale, iconScale, iconScale);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(label.transform.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(label.transform.parent.GetComponent<RectTransform>());
        }
    }
}
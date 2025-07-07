using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Base_Framework
{
    // [RequireComponent(typeof(Image))]
    public class _MUI_PanelPageSelector : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#CD9A9A><b>[_MUI_PanelPageSelector]</b></color> {0}";

        [Header("Page Selector")]
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _MUI_HorizontalSelector horizontalSelector;
        public _MUI_HorizontalSelector HorizontalSelector
        {
            get
            {
                return horizontalSelector;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected _MUI_HSelectorManager hSelector;
        [Space(5)]
        public bool invokeAtStart;
        public bool loopSelection;
        public int defaultIndex = 0;
        public int Index
        {
            get
            {
                return horizontalSelector.Index;
            }
        }

        [Header("Items")]
        // Items
        public List<Item> _itemList = new List<Item>();
        public UnityEvent<int> onPageChanged;//onValueChanged;
        [System.Serializable]
        public class Item
        {
            public string itemTitle = "Item Title";
            public GameObject pageObj;
        }

        protected virtual void Awake()
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            // Debug.Assert(horizontalSelector.invokeAtStart == false);
#endif
            horizontalSelector.enabled = false;
            
            if (hSelector != null)
            {
                hSelector.enabled = false;
                Destroy(hSelector);
                hSelector = null;
            }
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "<color=magenta>Start() - Init</color> <b>HorizontalSelector</b>!!!!!");

            Debug.Assert(horizontalSelector.gameObject.activeSelf == false);

            horizontalSelector.itemList.Clear();
            for (int i = 0; i < _itemList.Count; i++)
            {
                HorizontalSelector.Item item = new HorizontalSelector.Item();
                item.itemTitle = _itemList[i].itemTitle;
                item.onItemSelect.AddListener(OnItemSelect);
                horizontalSelector.itemList.Add(item);
            }

            horizontalSelector.invokeAtStart = invokeAtStart;
            horizontalSelector.loopSelection = loopSelection;
            horizontalSelector.defaultIndex = defaultIndex;

            horizontalSelector.gameObject.SetActive(true);
        }

        public virtual void SetupPageSelector(List<Item> itemList)
        {
            Debug.Assert(itemList != null);
            for (int i = 0; i < _itemList.Count; i++)
            {
                _itemList[i].pageObj.SetActive(false);
            }

            _itemList = itemList;
            horizontalSelector.itemList.Clear();
            for (int i = 0; i < _itemList.Count; i++)
            {
                HorizontalSelector.Item item = new HorizontalSelector.Item();
                item.itemTitle = _itemList[i].itemTitle;
                item.onItemSelect.AddListener(OnItemSelect);
                horizontalSelector.itemList.Add(item);
            }

            horizontalSelector.SetupSelector();
        }

#if DEBUG
        [Header("For Test")]
        [SerializeField]
        protected GameObject testPage00;
        [SerializeField]
        protected GameObject testPage01;
        [SerializeField]
        protected GameObject testPage02;
#endif
        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                List<Item> itemList = new List<Item>();
                Item item = new Item();
                item.itemTitle = "1 / 3";
                item.pageObj = testPage00;
                itemList.Add(item);
                item = new Item();
                item.itemTitle = "2 / 3";
                item.pageObj = testPage01;
                itemList.Add(item);
                item = new Item();
                item.itemTitle = "3 / 3";
                item.pageObj = testPage02;
                itemList.Add(item);

                SetupPageSelector(itemList);
                OnItemSelect();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                List<Item> itemList = new List<Item>();
                Item item = new Item();
                item.itemTitle = "1 / 2";
                item.pageObj = testPage00;
                itemList.Add(item);
                item = new Item();
                item.itemTitle = "2 / 2";
                item.pageObj = testPage01;
                itemList.Add(item);

                SetupPageSelector(itemList);
                OnItemSelect();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                List<Item> itemList = new List<Item>();
                Item item = new Item();
                item.itemTitle = "1 / 1";
                item.pageObj = testPage00;
                itemList.Add(item);

                SetupPageSelector(itemList);
                OnItemSelect();
            }
#endif
        }

        protected virtual void OnItemSelect()
        {
            Debug.Assert(horizontalSelector.loopSelection == loopSelection);
            Debug.LogFormat(LOG_FORMAT, "OnItemSelect(), _itemList.Count : <b>" + _itemList.Count + 
                "</b>, horizontalSelector.Index : <b>" + horizontalSelector.Index + "</b>");

            for (int i = 0; i < _itemList.Count; i++)
            {
                _itemList[i].pageObj.SetActive(false);
            }
            _itemList[horizontalSelector.Index].pageObj.SetActive(true);

            if (onPageChanged != null)
            {
                onPageChanged.Invoke(horizontalSelector.Index);
            }
        }

        public virtual void OnPageChanged(int index)
        {
            Debug.LogFormat(LOG_FORMAT, "OnPageChanged(), index : " + index);

        }
    }
}
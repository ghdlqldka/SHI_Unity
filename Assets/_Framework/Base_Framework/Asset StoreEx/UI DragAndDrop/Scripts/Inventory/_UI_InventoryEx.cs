using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace _Base_Framework
{
    public class _UI_InventoryEx : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=magenta><b>[_UI_InventoryEx]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected bool isDropZone = false;

        [SerializeField]
        protected _UI_DragAndDropHandlerEx ui_dragAndDropHandler;

        [Header("Prefabs")]
        [SerializeField]
        protected _UI_SlotEx slotPrefab;
        [SerializeField]
        protected _UI_DraggableItemEx itemPrefab;

        // [SerializeField]
        // protected _DraggableDataEx[] itemDatas;

        protected List<_UI_SlotEx> slotList = new List<_UI_SlotEx>();

        protected bool isReady = false;
        public bool IsReady
        {
            get
            {
                return isReady;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            isReady = false;

            Debug.Assert(ui_dragAndDropHandler);
            this.GetComponent<GridLayoutGroup>().constraintCount = ui_dragAndDropHandler.ItemDatas.Length;
        }

        protected virtual void OnDestroy()
        {
            //
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start(), this.gameObject : <b>" + this.gameObject.name + "</b>");

            isReady = true;
            _Reset();
        }

        public virtual void _Reset()
        {
            if (IsReady == false) // Not executed "Awake()"
            {
                return;
            }

            Debug.LogFormat(LOG_FORMAT, "_Reset()");

            StopAllCoroutines();
            // _index = 0;

            // CreateSlots(items);
            slotList.Clear();

            for (int i = this.transform.childCount - 1; i >= 0; i--)
            {
                // Destroy(this.transform.GetChild(i).gameObject);
                DestroyImmediate(this.transform.GetChild(i).gameObject); // for timing issue
            }

            // create a Slot for each object in the list, or use a premade one
            for (int i = 0; i < ui_dragAndDropHandler.ItemDatas.Length; i++)
            {
                _DraggableDataEx data = ui_dragAndDropHandler.ItemDatas[i];

                GameObject obj = Instantiate(slotPrefab.gameObject, this.transform);
                _UI_SlotEx ui_slot = obj.GetComponent<_UI_SlotEx>();
                ui_slot.Id = data.id;
                ui_slot.IsDropZone = isDropZone;

                if (isDropZone == false)
                {
                    // make an item object inside it as a child
                    obj = Instantiate(itemPrefab.gameObject, ui_slot.transform);
                    _UI_DraggableItemEx item = obj.GetComponent<_UI_DraggableItemEx>();

                    item.ui_dragAndDropHandler = ui_dragAndDropHandler;
                    item.ui_slot = ui_slot;
                    item.Data = data;

                    ui_dragAndDropHandler.ui_draggableItemList.Add(item);
                }
                else // silhouette
                {
                    // obj.GetComponent<Image>().sprite = data.icon;
                }

                slotList.Add(ui_slot);
            }

        }
    }
}

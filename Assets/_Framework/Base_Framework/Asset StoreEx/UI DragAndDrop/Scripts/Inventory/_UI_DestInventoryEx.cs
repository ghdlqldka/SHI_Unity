using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace _Base_Framework
{
    public class _UI_DestInventoryEx : _UI_InventoryEx
    {
        private static string LOG_FORMAT = "<color=#F1A1EE><b>[_UI_DestInventoryEx]</b></color> {0}";

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int _index = 0;

        protected override void Awake()
        {
            Debug.Assert(ui_dragAndDropHandler != null);

            isReady = false;

            if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Static)
            {
                //
            }
            else
            {
                this.GetComponent<GridLayoutGroup>().constraintCount = ui_dragAndDropHandler.ItemDatas.Length;
            }

            isDropZone = true;
        }

        protected override void OnDestroy()
        {
            //
        }

#if DEBUG
        protected List<int> DEBUG_Slot_ID = new List<int>(); // in _UI_DragAndDropHandlerEx._Type.Static
#endif
        public override void _Reset()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "_Reset()");

            if (IsReady == false) // Not executed "Awake()"
            {
                return;
            }

            Debug.LogFormat(LOG_FORMAT, "_Reset()");

            StopAllCoroutines();
            _index = 0;

            // CreateSlots(items);
            slotList.Clear();

            if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Static)
            {
                _UI_SlotEx[] slots = this.gameObject.GetComponentsInChildren<_UI_SlotEx>(true); // include inactivate
                Debug.LogFormat(LOG_FORMAT, "slots.Length : " + slots.Length);

                for (int i = 0; i < slots.Length; i++)
                {
                    // Debug.Assert(slots[i].gameObject.activeSelf == false);
                    slots[i].gameObject.name = "_UI_Dest_SlotEx_" + slots[i].Id;

#if DEBUG
                    if (DEBUG_Slot_ID.Contains(slots[i].Id) == true)
                    {
                        Debug.LogErrorFormat(LOG_FORMAT, "<b>DUPLICATION</b> SLOT ID : " + slots[i].Id);
                    }
                    DEBUG_Slot_ID.Add(slots[i].Id);
#endif

                    slots[i].DragAndDropHandler = ui_dragAndDropHandler;
                    slots[i].IsDropZone = isDropZone;
                    // dragAndDropHandler.DraggableItemList.Add(slots[i].transform.GetChild(0).GetComponent<_DraggableItemEx>());
                    slotList.Add(slots[i]);

                    // slots[i].gameObject.SetActive(true);
                }
            }
            else
            {
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
                    _UI_SlotEx slot = obj.GetComponent<_UI_SlotEx>();
                    slot.DragAndDropHandler = ui_dragAndDropHandler;
                    slot.Id = data.id;
                    slot.Silhouette = data.silhouetteIcon;
                    RectTransform rectTransform = obj.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = ui_dragAndDropHandler.CellSize;

                    Debug.Assert(isDropZone == true); // Dropzone is Destination Inventory
                    slot.IsDropZone = isDropZone;

                    // obj.GetComponent<Image>().sprite = data.icon; // silhouette

                    slotList.Add(slot);
                }

                if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Type_1)
                {
                    Debug.LogFormat(LOG_FORMAT, "this.transform.childCount : " + this.transform.childCount + ", _index : " + _index);
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        this.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    this.transform.GetChild(_index).gameObject.SetActive(true);
                }
            }
        }

        public virtual void _OnBeginDrag(_UI_DraggableItemEx item)
        {
            for (int i = 0; i < slotList.Count; i++)
            {
                if (slotList[i].Id == item.Id)
                {
                    //
                }
                else
                {
                    slotList[i].transform.Find("Image_Slot").GetComponent<Image>().raycastTarget = false;
                }
            }
        }

        public virtual void _OnEndDrag(_UI_DraggableItemEx item)
        {
            for (int i = 0; i < slotList.Count; i++)
            {
                slotList[i].transform.Find("Image_Slot").GetComponent<Image>().raycastTarget = true;
            }
        }

        public virtual void _OnDragAndDropUpdated(_UI_DraggableItemEx item)
        {
            // throw new System.NotImplementedException();

            if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Static)
            {
                ui_dragAndDropHandler.Invoke_OnDragAndDropUpdated(item);
            }
            else if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Type_0)
            {
                ui_dragAndDropHandler.Invoke_OnDragAndDropUpdated(item);
            }
            else if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Type_1)
            {
                StartCoroutine(PostOnDragAndDropUpdated(item));
            }
            else
            {
                Debug.Assert(false);
            }
        }

        protected virtual IEnumerator PostOnDragAndDropUpdated(_UI_DraggableItemEx item)
        {
            yield return new WaitForSeconds(1.0f);

            if (_index < (this.transform.childCount - 1))
            {
                this.transform.GetChild(_index).gameObject.SetActive(false);
                _index++;
                this.transform.GetChild(_index).gameObject.SetActive(true);
                Debug.LogWarningFormat(LOG_FORMAT, "NEXT!!!!!");
            }
            else if (_index == (this.transform.childCount - 1))
            {
                this.transform.GetChild(_index).gameObject.SetActive(false);
                Debug.LogWarningFormat(LOG_FORMAT, "COMPLETE!!!!!");
            }

            ui_dragAndDropHandler.Invoke_OnDragAndDropUpdated(item);
        }
    }
}

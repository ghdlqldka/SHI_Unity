using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace _Base_Framework
{
    public class _UI_StartInventoryEx : _UI_InventoryEx
    {
        private static string LOG_FORMAT = "<color=#F69C5F><b>[_UI_StartInventoryEx]</b></color> {0}";

        [Space(10)]
        public bool bRandomize = false;

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

            isDropZone = false;
        }

#if DEBUG
        protected List<int> DEBUG_Slot_ID = new List<int>(); // in _UI_DragAndDropHandlerEx._Type.Static
#endif
        public override void _Reset()
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

            if (ui_dragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Static)
            {
                _UI_SlotEx[] _ui_slots = this.gameObject.GetComponentsInChildren<_UI_SlotEx>(true);
                Debug.LogFormat(LOG_FORMAT, "_ui_slots.Length : " + _ui_slots.Length);

                for (int i = 0; i < _ui_slots.Length; i++)
                {
                    _ui_slots[i].gameObject.name = "_UI_Start_SlotEx_" + _ui_slots[i].Id;
#if DEBUG
                    if (DEBUG_Slot_ID.Contains(_ui_slots[i].Id) == true)
                    {
                        Debug.LogErrorFormat(LOG_FORMAT, "<b>DUPLICATION</b> SLOT ID : " + _ui_slots[i].Id);
                    }
                    DEBUG_Slot_ID.Add(_ui_slots[i].Id);
#endif

                    _ui_slots[i].DragAndDropHandler = ui_dragAndDropHandler;
                    _ui_slots[i].IsDropZone = isDropZone;
                    _UI_DraggableItemEx item = _ui_slots[i].transform.GetComponentInChildren<_UI_DraggableItemEx>();
                    item.Id = _ui_slots[i].Id;
                    item.ui_dragAndDropHandler = ui_dragAndDropHandler;
                    item.ui_slot = _ui_slots[i];
                    // item.Data = data;

                    ui_dragAndDropHandler.ui_draggableItemList.Add(item);
                    slotList.Add(_ui_slots[i]);
                }
            }
            else // dynamically add
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
                    _UI_SlotEx ui_slot = obj.GetComponent<_UI_SlotEx>();
                    ui_slot.DragAndDropHandler = ui_dragAndDropHandler;
                    ui_slot.Id = data.id;
                    ui_slot.IsDropZone = isDropZone;

                    // make an item object inside it as a child
                    obj = Instantiate(itemPrefab.gameObject, ui_slot.transform);
                    _UI_DraggableItemEx item = obj.GetComponent<_UI_DraggableItemEx>();
                    item.GetComponent<RectTransform>().sizeDelta = ui_dragAndDropHandler.CellSize;

                    item.ui_dragAndDropHandler = ui_dragAndDropHandler;
                    item.ui_slot = ui_slot;
                    item.Data = data;

                    ui_dragAndDropHandler.ui_draggableItemList.Add(item);

                    slotList.Add(ui_slot);
                }

                if (bRandomize == true) // Random Slots
                {
                    int count = this.transform.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        int index = Random.Range(0, count - 1);
                        Transform _t = this.transform.GetChild(index);
                        _t.SetParent(null);
                        _t.SetParent(this.transform); // add to last
                    }
                }
            }
        }
    }
}

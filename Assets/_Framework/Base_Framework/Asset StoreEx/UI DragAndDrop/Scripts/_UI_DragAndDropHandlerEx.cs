using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _UI_DragAndDropHandlerEx : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#0AFF00><b>[_UI_DragAndDropHandlerEx]</b></color> {0}";

        public enum _Type
        {
            Static,

            Type_0,
            Type_1
        }

        [SerializeField]
        protected _Type type = _Type.Type_0;
        public _Type Type
        {
            get
            {
                return type;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected bool _isDraggable = true;
        public bool _IsDraggable
        {
            get
            {
                return _isDraggable;
            }
            set
            {
                if (_isDraggable != value)
                {
                    _isDraggable = value;
                }
            }
        }

        [SerializeField]
        protected Vector2 cellSize;
        public Vector2 CellSize
        {
            get
            {
                return cellSize;
            }
        }

        [Header("Inventory")]
        [SerializeField]
        protected _UI_StartInventoryEx ui_startInventory;
        [SerializeField]
        protected _UI_DestInventoryEx ui_destInventory;

        [Space(10)]
        [SerializeField]
        protected _DraggableDataEx[] itemDatas;
        public _DraggableDataEx[] ItemDatas
        {
            get
            {
                return itemDatas;
            }
        }

        [Space(10)]
        [SerializeField]
        protected List<_UI_DraggableItemEx> draggableItemList;
        public List<_UI_DraggableItemEx> ui_draggableItemList
        {
            get
            {
                return draggableItemList;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected AudioClip clip;

        // this avoids memory allocation each time we move while dragging
        protected static List<RaycastResult> hits = new List<RaycastResult>();

        public delegate void DragAndDropUpdatedCallback(_UI_DraggableItemEx item);
        public event DragAndDropUpdatedCallback OnDragAndDropUpdated;

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), Type : <b><color=red>" + Type + "</color></b>");

            if (_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework)
            {
                if (_GlobalObjectUtilities.Instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>(_GlobalObjectUtilities.prefabPath);
                    Debug.Assert(prefab != null);
                    GameObject obj = Instantiate(prefab);
                    Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilities</b>\"</color>");
                    obj.name = prefab.name;
                }
            }
            else
            {
                Debug.Assert(false);
            }

            Debug.Assert(ui_startInventory != null && ui_destInventory != null);
            if (Type == _Type.Static)
            {
                Destroy(ui_startInventory.GetComponent<GridLayoutGroup>());
                Destroy(ui_destInventory.GetComponent<GridLayoutGroup>());

                cellSize = Vector2.zero;
                itemDatas = null;
            }
            else
            {
                Debug.Assert(ItemDatas.Length > 0);
                ui_startInventory.GetComponent<GridLayoutGroup>().cellSize = cellSize;
                ui_destInventory.GetComponent<GridLayoutGroup>().cellSize = cellSize;
            }

            draggableItemList = new List<_UI_DraggableItemEx>();

            if (Type == _Type.Static)
            {
                //
            }
            else if (Type == _Type.Type_0)
            {
                //
            }
            else if (Type == _Type.Type_1)
            {
                ui_destInventory.gameObject.GetComponent<GridLayoutGroup>().enabled = false;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        protected virtual void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.R))
            {
                _Reset();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _IsDraggable = !_IsDraggable;
            }
#endif
        }

        public virtual void _Reset()
        {
            Debug.LogFormat(LOG_FORMAT, "<color=red><b>_Reset()</b></color>");

            if (Type == _Type.Static)
            {
                for (int i = 0; i < ui_draggableItemList.Count; i++)
                {
                    if (ui_draggableItemList[i].IsDragging)
                    {
                        ui_draggableItemList[i].OnEndDrag(null);
                    }
                    ui_draggableItemList[i].Draggable = true;
                    ui_draggableItemList[i].transform.SetParent(ui_draggableItemList[i].ui_slot.transform, false);
                }
            }
            else
            {
                if (ui_startInventory.IsReady == true && ui_destInventory.IsReady == true)
                {
                    ui_draggableItemList.Clear();

                    ui_startInventory._Reset();
                    ui_destInventory._Reset();
                }
                else
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "Reset failed!!!");
                    // do-nothings
                    Debug.Assert(ui_draggableItemList.Count == 0);
                }
            }
        }

        public virtual void _OnBeginDrag(_UI_DraggableItemEx item)
        {
            ui_destInventory._OnBeginDrag(item);
        }

        public virtual void _OnEndDragItem(_UI_DraggableItemEx item)
        {
            // raycast to find what we're dropping over
            _UI_SlotEx targetSlot = GetSlotUnderMouse();

            ui_destInventory._OnEndDrag(item);

            if ((targetSlot != null && targetSlot.IsDropZone == true && targetSlot.Id == item.Id) && _IsDraggable == true)
            {
                item.transform.SetParent(targetSlot.transform);
                // item.Slot = targetSlot;
                item.Draggable = false;

                Debug.LogFormat(LOG_FORMAT, "<color=yellow>Place Draggable(</color><b><color=red>" + item.Id + "</color></b><color=yellow>) at <b>DropZone</b></color>");

                _FxManager.Instance.PlayOneShot(clip);

                CheckAllItemsUndraggable();

                ui_destInventory._OnDragAndDropUpdated(item);
                // Invoke_OnDragAndDropUpdated(item);
            }
            else
            {
#if DEBUG
                if (targetSlot == null)
                {
                    Debug.LogFormat(LOG_FORMAT, "targetSlot : <b><color=red>NULL</color></b>");
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "targetSlot : <b><color=red>" + targetSlot.gameObject.name + "</color></b>");
                }
#endif
                item.transform.SetParent(item.ui_slot.transform);
                Debug.LogFormat(LOG_FORMAT, "<color=cyan>Replace Draggable at <b>Original Slot</b></color>");
            }
        }

        public void Invoke_OnDragAndDropUpdated(_UI_DraggableItemEx item)
        {
            Debug.LogFormat(LOG_FORMAT, "Invoke_OnDragAndDropUpdated(), Id : <b><color=red>" + item.Id + "</color></b>");
            if (OnDragAndDropUpdated != null)
            {
                OnDragAndDropUpdated(item);
            }
        }

        // finds the firstSlot component currently under the mouse
        protected virtual _UI_SlotEx GetSlotUnderMouse()
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointer, hits);
            foreach (RaycastResult hit in hits)
            {
                _UI_SlotEx ui_slot = hit.gameObject.GetComponent<_UI_SlotEx>();
                if (ui_slot != null)
                {
                    return ui_slot;
                }
                else if (string.Compare("Image_Slot", hit.gameObject.name) == 0)
                {
                    Debug.Assert(hit.gameObject.transform.parent != null);
                    ui_slot = hit.gameObject.transform.parent.GetComponent<_UI_SlotEx>();
                    if (ui_slot != null)
                    {
                        return ui_slot;
                    }
                }
                else
                {
                    //
                }
            }

            return null;
        }

        public bool CheckAllItemsUndraggable()
        {
            for (int i = 0; i < ui_draggableItemList.Count; i++)
            {
                if (ui_draggableItemList[i].Draggable == true)
                {
                    return false;
                }
            }

            Debug.LogFormat(LOG_FORMAT, "<color=magenta> ===> All Items are <b>UNDRAGGABLE</b>!!!!!</color>");

            // _Reset(); // test code
            return true;
        }
    }
}
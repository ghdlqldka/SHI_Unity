using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace _Base_Framework
{
    [System.Serializable]
    public class _UI_DraggableItemEx : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static string LOG_FORMAT = "<color=#A6F6CD><b>[_UI_DraggableItemEx]</b></color> {0}";

        [HideInInspector]
        protected _DraggableDataEx data;
        public virtual _DraggableDataEx Data
        {
            get
            {
                return data;
            }
            set
            {
                Debug.Assert(value != null);
                data = value;

                (transform as RectTransform).anchoredPosition = Vector3.zero;

                _id = value.id;
                if (_text != null)
                {
                    _text.text = value.name;
                }

                if (iconImage != null)
                {
                    iconImage.sprite = value.icon;
                }
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected int _id;
        public virtual int Id
        {
            get
            {
                return _id;
            }
            set
            {
                Debug.Assert(Data == null);
                this.gameObject.name = "_DraggableItemEx_" + value;
                _id = value;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected bool draggable = true;
        public bool Draggable
        {
            get
            {
                return draggable;
            }
            set
            {
                draggable = value;
                Debug.LogWarningFormat(LOG_FORMAT, "Id(<b><color=red>" + Id + "</color></b>), <b>Draggable : <color=red>" + value + "</color></b>");
            }
        }

        [Space(10)]
        [SerializeField]
        protected Image iconImage;
        [SerializeField]
        protected Text _text;

        // [ReadOnly]
        // [SerializeField]
        protected _UI_DragAndDropHandlerEx _ui_dragAndDropHandler;
        public _UI_DragAndDropHandlerEx ui_dragAndDropHandler
        {
            protected get
            {
                return _ui_dragAndDropHandler;
            }
            set
            {
                _ui_dragAndDropHandler = value;
            }
        }

        // [HideInInspector]
        protected _UI_SlotEx _slot;
        public _UI_SlotEx ui_slot
        {
            get
            {
                return _slot;
            }
            set
            {
                _slot = value;
            }
        }

        protected bool dragging;
        public bool IsDragging
        {
            get // It only can be call in _Reset()!!!!!!!!!
            {
                return dragging;
            }
        }

        // default to left dragging
        [Space(10)]
        [Tooltip("0 = left mouse, 1 = right mouse to drag")]
        public int mouseButton = 0;

        protected Canvas canvas;
        protected int sortingOrder;

        protected virtual void Awake()
        {
            canvas = this.transform.GetComponentInParent<Canvas>();
            Debug.Assert(canvas != null);
            sortingOrder = canvas.sortingOrder;

            Debug.Assert(mouseButton == 0 || mouseButton == 1);

            Debug.Assert(iconImage != null);
            Debug.Assert(_text != null);
        }

        protected virtual void Start()
        {
            this.gameObject.name = "_DraggableItemEx_" + Id;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            // start dragging with the right mouse, and only if the container will allow it
            if (Input.GetMouseButton(mouseButton) && Draggable == true)
            {
                dragging = true;

                Debug.LogFormat(LOG_FORMAT, "+++++ On<b>Begin</b>Drag(), Id : <b><color=red>" + Id + "</color></b>");

                this.transform.SetParent(canvas.transform);
                // move this to the very front of the UI, so the dragged element draws over everything
                this.transform.SetAsLastSibling();

                // move that canvas forwards, so we're dragged on top of other items, and not behind them
                canvas.sortingOrder = sortingOrder + 1;

                ui_dragAndDropHandler._OnBeginDrag(this);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (dragging == true && ui_dragAndDropHandler._IsDraggable == true)
            {
                this.transform.position = eventData.position;
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (dragging == false)
            {
                return;
            }

            Debug.LogFormat(LOG_FORMAT, "----- On<b>End</b>Drag(), Id : <b><color=red>" + Id + "</color></b>");

            ui_dragAndDropHandler._OnEndDragItem(this);

            (this.transform as RectTransform).anchoredPosition3D = Vector3.zero;
            dragging = false;

            canvas.sortingOrder = sortingOrder;
        }

        
    }
}
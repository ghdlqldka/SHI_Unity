using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace _Base_Framework
{
    [RequireComponent(typeof(Image))]
    public class _UI_SlotEx : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#2ECB7B><b>[_UI_SlotEx]</b></color> {0}";

        [SerializeField]
        protected Image iconImage;

        // [ReadOnly]
        // [SerializeField]
        protected _UI_DragAndDropHandlerEx dragAndDropHandler;
        public _UI_DragAndDropHandlerEx DragAndDropHandler
        {
            protected get
            {
                return dragAndDropHandler;
            }
            set
            {
                dragAndDropHandler = value;
            }
        }

        // [ReadOnly]
        [SerializeField]
        protected int id;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool isDropZone;
        public bool IsDropZone
        {
            get
            {
                return isDropZone;
            }
            set
            {
                isDropZone = value;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected Sprite silhouetteSpirte;
        public Sprite Silhouette
        {
            get
            {
                return silhouetteSpirte;
            }
            set
            {
                silhouetteSpirte = value;
            }
        }

        protected virtual void Awake()
        {
            Debug.Assert(iconImage != null);
        }

        protected virtual void Start()
        {
            StartCoroutine(PostStart());
        }

        protected virtual IEnumerator PostStart()
        {
            while (DragAndDropHandler == null)
            {
                yield return null;
                Debug.LogFormat(LOG_FORMAT, "yield return null");
            }

            if (DragAndDropHandler.Type == _UI_DragAndDropHandlerEx._Type.Static)
            {
                //
            }
            else
            {
                if (IsDropZone == false)
                {
                    this.gameObject.name = "_UI_Slot_" + Id;
                    iconImage.enabled = false;
                }
                else
                {
                    this.gameObject.name = "_UI_Slot_" + Id + "_DropZone";
                    if (Silhouette != null)
                    {
                        iconImage.sprite = Silhouette;
                    }
                }
            }
        }

        public virtual void HighlightSlots(bool on)
        {
            Debug.LogFormat(LOG_FORMAT, "HighlightSlots(), on : " + on);

            if (iconImage != null && IsDropZone == true)
            {
                iconImage.color = on ? Color.blue : Color.white;
            }
        }
    }
}
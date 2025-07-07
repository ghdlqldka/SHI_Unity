using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RuntimeSceneGizmo
{
	public class _SceneGizmoRenderer : SceneGizmoRenderer
    {
        private static string LOG_FORMAT = "<color=#53F6EB><b>[_SceneGizmoRenderer]</b></color> {0}";

        public delegate void GizmoClicked(GizmoComponent component);
        public event GizmoClicked OnGizmoClicked;

        // [SerializeField]
        // protected SceneGizmoController controller;
        [SerializeField]
        protected _SceneGizmoController gizmoPrefab;

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            imageHolderTR = (RectTransform)imageHolder.transform;
            controller = (_SceneGizmoController)Instantiate(gizmoPrefab);

            m_onComponentClicked = null; // Not used!!!!!

            StartCoroutine(PostAwake());
        }

        protected virtual IEnumerator PostAwake()
        {
            while (controller.TargetTexture == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            imageHolder.texture = controller.TargetTexture;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                return;
            }

            GizmoComponent hitComponent = controller.Raycast(GetNormalizedPointerPosition(eventData));
            if (hitComponent != GizmoComponent.None)
            {
                // m_onComponentClicked.Invoke(hitComponent);
                Invoke_OnGizmoClicked(hitComponent);
            }
        }

        protected void Invoke_OnGizmoClicked(GizmoComponent component)
        {
            if (OnGizmoClicked != null)
            {
                OnGizmoClicked(component);
            }
        }
    }
}
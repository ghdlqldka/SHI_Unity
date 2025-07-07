using RuntimeSceneGizmo;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _SHI_BA
{
	public class SWC_GizmoRenderer : _Magenta_WebGL.MWGL_GizmoRenderer
    {
        private static string LOG_FORMAT = "<color=#53F6EB><b>[SWC_GizmoRenderer]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            imageHolderTR = (RectTransform)imageHolder.transform;
            controller = (SWC_GizmoController)Instantiate(gizmoPrefab);

            m_onComponentClicked = null; // Not used!!!!!

            StartCoroutine(PostAwake());
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
    }
}
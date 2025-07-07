using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _Magenta_Framework
{
    [RequireComponent(typeof(Animator))]
    public class MUI_ContextMenuEx : Michsky.MUIP._MUI_ContextMenu
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[MUI_ContextMenuEx]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(ButtonPrefab != null);
            Debug.Assert(SeparatorPrefab != null);
            Debug.Assert(SubMenuPrefab != null);

            autoSubMenuPosition = true; // forcelly set!!!!!
            debugMode = true; // forcelly set!!!!!

            if (mainCanvas == null)
            {
                mainCanvas = this.gameObject.GetComponentInParent<Canvas>();
            }
            contextContent = this.transform.Find("Content").gameObject;
            Debug.Assert(_contentObj != null);

            contextAnimator = this.gameObject.GetComponent<Animator>();
            Debug.Assert(_animator != null);
            cameraSource = CameraSource.Custom; // forcelly set!!!!!
            Debug.Assert(targetCamera != null);

            contextRect = this.gameObject.GetComponent<RectTransform>();
            contentRect = _contentObj.GetComponent<RectTransform>();
            contentPos = new Vector3(vBorderTop, hBorderLeft, 0);
            this.gameObject.transform.SetAsLastSibling();
#if UNITY_2022_1_OR_NEWER
            subMenuBehaviour = SubMenuBehaviour.Click;
#endif
        }
    }
}
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.MUIP
{
    [RequireComponent(typeof(Animator))]
    public class _MUI_ContextMenu : ContextMenuManager
    {
        private static string LOG_FORMAT = "<color=#EBC6FB><b>[_MUI_ContextMenu]</b></color> {0}";

        // public bool isOn;
        public bool IsOn
        {
            get
            {
                return isOn;
            }
            set
            {
                Debug.LogWarningFormat(LOG_FORMAT, "IsOn : <b><color=yellow>" + IsOn + "</color></b>");
                isOn = value;
            }
        }

        // [Header("Prefab")]
        // public GameObject contextButton;
        public GameObject ButtonPrefab
        {
            get
            {
                return contextButton;
            }
        }
        public GameObject SeparatorPrefab
        {
            get
            {
                return contextSeparator;
            }
        }
        public GameObject SubMenuPrefab
        {
            get
            {
                return contextSubMenu;
            }
        }

        // public GameObject contextContent;
        public GameObject _contentObj
        {
            get
            {
                return contextContent;
            }
        }

        // public Animator contextAnimator;
        public Animator _animator
        {
            get
            {
                return contextAnimator;
            }
        }

        [SerializeField]
        protected Transform itemParent;
        public Transform ItemParent
        {
            get
            {
                return itemParent;
            }
        }

        protected override void Awake()
        {
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
            /*
            if (contextAnimator == null)
            { 
                contextAnimator = this.gameObject.GetComponent<Animator>();
            }
            */
            contextAnimator = this.gameObject.GetComponent<Animator>();
            Debug.Assert(_animator != null);
            /*
            if (cameraSource == CameraSource.Main)
            { 
                targetCamera = Camera.main;
            }
            */
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

        public override void Open()
        {
            _animator.Play("Menu In");
            IsOn = true;
        }

        public override void Close()
        {
            // base.Close();

            _animator.Play("Menu Out");
            IsOn = false;
        }
    }
}
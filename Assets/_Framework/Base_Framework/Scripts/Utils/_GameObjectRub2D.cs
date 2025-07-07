using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Base_Framework
{
    [RequireComponent(typeof(Collider2D))]
    public class _GameObjectRub2D : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_GameObjectRub2D]</b></color> {0}";

        protected Collider2D _collider;

        [SerializeField]
        protected Camera _camera;
        public Camera _Camera
        {
            set
            {
                _camera = value;
            }
        }

        [Header("Cursor")]
        [SerializeField]
        protected Texture2D cursorTexture;
        [SerializeField]
        protected CursorMode cursorMode = CursorMode.ForceSoftware;
        [SerializeField]
        protected Vector2 hotSpot = Vector2.zero;

        [SerializeField]
        protected float distanceTrigger = 0.0f;
        public float DistanceTrigger
        {
            get
            {
                return distanceTrigger;
            }
            set
            {
                distanceTrigger = value;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected float mouseRubDistance = 0.0f;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected bool rubStarted = false;

        [SerializeField]
        protected AudioClip triggerAudioClip;
        protected bool isTriggered = false;

        [Space(10)]
        [SerializeField]
        protected bool onlyOnce = false;
        public UnityEvent<_GameObjectRub2D> onTriggerEvent = new UnityEvent<_GameObjectRub2D>();

        protected Vector3 lastMousePosition;

        protected virtual void Awake()
        {
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

            Debug.Assert(distanceTrigger > 0);
            _collider = this.GetComponent<Collider2D>();
            _collider.isTrigger = true;
            Debug.Assert(_collider != null);
            // Debug.Assert(triggerAudioClip != null);
        }

        protected virtual void OnEnable()
        {
            _collider.enabled = true;
        }

        protected virtual void OnDisable()
        {
            _collider.enabled = false;
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }

        protected virtual void OnMouseEnter()
        {
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }

        protected virtual void OnMouseDown()
        {
            // Debug.LogFormat(LOG_FORMAT, "OnMouseDown()");

            Debug.Assert(isTriggered == false);
            if (_EventSystem.IsPointerOverGameObject() == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                return;
            }

            RaycastHit2D hit2D = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(Input.mousePosition));
            if (hit2D.collider == null || hit2D.collider.gameObject != this.gameObject)
            {
                return;
            }

            lastMousePosition = Input.mousePosition;
            mouseRubDistance = 0;

            rubStarted = true; // <=======================
        }

        protected virtual void OnMouseDrag()
        {
            if (rubStarted == true && isTriggered == false && Input.mousePosition != lastMousePosition)
            {
                if (_EventSystem.IsPointerOverGameObject() == true)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                    rubStarted = false; // <=======================
                    return;
                }

                RaycastHit2D hit = Physics2D.GetRayIntersection(_camera.ScreenPointToRay(Input.mousePosition));
                if (hit.collider == null || hit.collider.gameObject != this.gameObject)
                {
                    rubStarted = false; // <=======================
                    return;
                }

                mouseRubDistance += (Input.mousePosition - lastMousePosition).magnitude;
                lastMousePosition = Input.mousePosition;
                // Debug.LogFormat(LOF_FORMAT, "" + mouseRubDistance);

                if (mouseRubDistance >= distanceTrigger)
                {
                    // Debug.LogWarningFormat(LOG_FORMAT, "<color=red><b>Trigger Fired!!!!!</b></color>()");
                    isTriggered = true;
                    _collider.enabled = false;
                    // mouseRubDistance = 0;
                    if (triggerAudioClip != null)
                    {
                        _FxManager.Instance.PlayOneShot(triggerAudioClip);
                    }
                    onTriggerEvent.Invoke(this);
                }
            }
        }

        protected virtual void OnMouseUp()
        {
            rubStarted = false; // <=======================

            if (isTriggered == true)
            {
                isTriggered = false;
                if (onlyOnce == true)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "<b>Only Once Operation : <color=red>True</color></b>. So, disable this Script!!!!!");
                    this.enabled = false;
                }
                else
                {
                    _collider.enabled = true;
                }
            }
        }

        protected virtual void OnMouseExit()
        {
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
        }

        public virtual void OnTriggerGameObjectRub2D(_GameObjectRub2D gameObjectRub)
        {
            // throw new System.NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>OnTrigger</color>GameObjectRub2D</b>(), gameObjectRub : <b>" + gameObjectRub.gameObject.name + "</b>");
        }
    }
}
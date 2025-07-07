using UnityEngine;
using UnityEngine.Events;

namespace _Base_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class _GameObjectRubCollider : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_GameObjectRubCollider]</b></color> {0}";

        protected Collider _collider;

        [SerializeField]
        protected Camera _camera;
        public Camera _Camera
        {
            set
            {
                _camera = value;
            }
        }


        [Header("Rub")]
#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected bool _rubStarted = false;
        public bool RubStarted
        {
            get
            {
                return _rubStarted;
            }
            set
            {
                if (_rubStarted != value)
                {
                    _rubStarted = value;
                    if (value == true)
                    {
                        // change the scale, give the user some visual feedback
                        this.transform.localScale *= 1.1f;
                    }
                    else
                    {
                        //change the scale of dragObj1, give the user some visual feedback
                        this.transform.localScale *= 10f / 11f;
                    }
                    Debug.LogFormat(LOG_FORMAT, "RubStarted : <b>" + value + "</b>");
                }
            }
        }
        [SerializeField]
        protected float rubSpeed = 1.0f;

        [Space(10)]
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

        [Header("Collision")]
        [ReadOnly]
        [SerializeField]
        protected float _collisionDistance = 0.0f;
        [ReadOnly]
        [SerializeField]
        protected bool _collisionStay = false;

        [Space(10)]
        [SerializeField]
        protected AudioClip triggerAudioClip;
        protected bool isTriggered = false;

        [Space(10)]
        [SerializeField]
        protected bool onlyOnce = false;
        public UnityEvent<_GameObjectRubCollider> onTriggerEvent = new UnityEvent<_GameObjectRubCollider>();

        protected Vector3 rubOffset;

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
            _collider = this.GetComponent<Collider>();
            _collider.isTrigger = true; // <================
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
        }

        protected virtual void OnMouseDown()
        {
            // Debug.LogFormat(LOG_FORMAT, "OnMouseDown()");

            if (_EventSystem.IsPointerOverGameObject() == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                return;
            }

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            //use raycast at the cursor position to detect the object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity) == true)
            {
                if (hit.collider.transform == this.transform) // itself
                {
                    Vector3 p = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
                    rubOffset = this.transform.position - (p * rubSpeed);

                    this.transform.position = (p * rubSpeed) + rubOffset;

                    lastMousePosition = Input.mousePosition;
                    RubStarted = true; // <=======================
                }
            }
        }

        protected virtual void OnMouseDrag()
        {
            if (RubStarted == true && Input.mousePosition != lastMousePosition)
            {
                Vector3 p = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
                this.transform.position = (p * rubSpeed) + rubOffset;

                if (_collisionStay == true && isTriggered == false)
                {
                    _collisionDistance += (Input.mousePosition - lastMousePosition).magnitude;
                    // Debug.LogWarningFormat(LOG_FORMAT, "<color=red><b>Trigger Fired!!!!!</b></color>()");
                    if (_collisionDistance >= distanceTrigger)
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

                lastMousePosition = Input.mousePosition;
            }
        }

        protected virtual void OnMouseUp()
        {
            if (RubStarted == true)
            {
                Vector3 p = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));

                this.transform.position = (p * rubSpeed) + rubOffset;

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

                RubStarted = false; // <=======================
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnTriggerEnter(), collision : " + other.gameObject.name);
            _collisionDistance = 0;
            _collisionStay = true;
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnTriggerStay(), collision : " + other.gameObject.name);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            // Debug.LogFormat(LOG_FORMAT, "OnTriggerExit(), collision : " + other.gameObject.name);
            _collisionDistance = 0;
            _collisionStay = false;
        }

        public virtual void OnTriggerGameObjectRubCollider(_GameObjectRubCollider gameObjectRubCollider)
        {
            // throw new System.NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>OnTrigger</color>GameObjectRubCollider</b>(), gameObjectRubCollider : " + gameObjectRubCollider.gameObject.name);
        }
    }
}
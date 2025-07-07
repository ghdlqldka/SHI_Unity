using _InputTouches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Base_Framework
{
    [RequireComponent(typeof(Collider2D))]
    public class _GameObjectButton2D : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_GameObjectButton2D]</b></color> {0}";

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

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected bool isPressed = false;
        [SerializeField]
        protected AudioClip clickAudioClip;

        [Space(10)]
        [SerializeField]
        protected bool onlyOnce = false;
        public UnityEvent<_GameObjectButton2D> onClickEvent = new UnityEvent<_GameObjectButton2D>();

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

            _collider = this.GetComponent<Collider2D>();
            _collider.isTrigger = true;
            Debug.Assert(_collider != null);
            // Debug.Assert(clickAudioClip != null);
        }

        protected virtual void OnEnable()
        {
            _collider.enabled = true;
            StartCoroutine(CheckButtonHit());
        }

        protected virtual IEnumerator CheckButtonHit()
        {
            while (true)
            {
                yield return null;

                if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Left_Button))
                {
                    /*
                    if (EventSystem.current.IsPointerOverGameObject() == true)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                        continue;
                    }
                    */
                    if (_EventSystem.IsPointerOverGameObject() == true)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                        continue;
                    }

                    Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
                    if (hit2D.collider != null && hit2D.collider.gameObject == this.gameObject)
                    {
                        // onClickEvent.Invoke(this.gameObject);
                        isPressed = true;
                    }
                    else
                    {
                        // Debug.LogFormat(LOG_FORMAT, "hit : " + hit.collider);
                    }
                }
                else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Left_Button))
                {
                    /*
                    if (EventSystem.current.IsPointerOverGameObject() == true)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                        continue;
                    }
                    */
                    if (_EventSystem.IsPointerOverGameObject() == true)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "IGNORE for Pointer Over GameObject!!!!");
                        continue;
                    }

                    if (isPressed == true)
                    {
                        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);
                        if (hit2D.collider != null && hit2D.collider.gameObject == this.gameObject)
                        {
                            if (clickAudioClip != null)
                            {
                                _FxManager.Instance.PlayOneShot(clickAudioClip);
                            }
                            onClickEvent.Invoke(this);
                            if (onlyOnce == true)
                            {
                                Debug.LogWarningFormat(LOG_FORMAT, "<b>Only Once Operation : <color=red>True</color></b>. So, disable this Script!!!!!");
                                this.enabled = false;
                            }
                        }
                    }
                    isPressed = false;
                }
            }
        }

        protected virtual void OnDisable()
        {
            _collider.enabled = false;
        }

        public virtual void OnClickGameObjectButton2D(_GameObjectButton2D gameObjectButton)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>OnClick</color>GameObjectButton2D</b>(), gameObjectButton : <b>" + gameObjectButton.gameObject.name + "</b>");
        }
    }
}
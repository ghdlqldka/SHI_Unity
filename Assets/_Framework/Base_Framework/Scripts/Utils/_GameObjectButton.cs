using _InputTouches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _Base_Framework
{
    [RequireComponent(typeof(Collider))]
    public class _GameObjectButton : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#B1C5EF><b>[_GameObjectButton]</b></color> {0}";

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

        [ReadOnly]
        [SerializeField]
        protected bool isPressed = false;

        [SerializeField]
        protected AudioClip clickAudioClip;

        [Space(10)]
        [SerializeField]
        protected bool onlyOnce = false;
        public UnityEvent<_GameObjectButton> onClickEvent = new UnityEvent<_GameObjectButton>();

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

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

            _collider = this.GetComponent<Collider>();
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
                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo) && hitInfo.collider.gameObject == this.gameObject)
                    {
                        // onClickEvent.Invoke(this.gameObject);
                        isPressed = true;
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
                        RaycastHit hitInfo;
                        if (Physics.Raycast(ray, out hitInfo) && hitInfo.collider.gameObject == this.gameObject)
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

        public virtual void OnClickGameObjectButton(_GameObjectButton gameObjectButton)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>OnClick</color>GameObjectButton</b>(), gameObjectButton : <b>" + gameObjectButton.gameObject.name + "</b>");
        }
    }
}
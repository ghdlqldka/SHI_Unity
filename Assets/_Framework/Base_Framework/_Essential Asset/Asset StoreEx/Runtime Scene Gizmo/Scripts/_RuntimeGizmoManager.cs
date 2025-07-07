using UnityEngine;

namespace RuntimeSceneGizmo
{
	public class _RuntimeGizmoManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#F346C2><b>[_RuntimeGizmoManager]</b></color> {0}";

        protected static _RuntimeGizmoManager _instance;
        public static _RuntimeGizmoManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected Transform _camParentTransform;
        public Transform CamParentTransform
        {
            get
            {
                return _camParentTransform;
            }
        }

        [SerializeField]
        protected Camera _camera;
        public Camera _Camera
        {
            get
            {
                Debug.AssertFormat(_camera != null, "");
                return _camera;
            }
        }

        public delegate void GizmoClicked(GizmoComponent component);
        public event GizmoClicked OnGizmoClicked;

        [Space(10)]
        [SerializeField]
        protected _GizmoListener _gizmoListener;
        public _GizmoListener gizmoListener
        {
            get
            {
                return _gizmoListener;
            }
        }

        [SerializeField]
        protected _SceneGizmoRenderer _gizmoRenderer;
        public _SceneGizmoRenderer gizmoRenderer
        {
            get
            {
                return _gizmoRenderer;
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
#if DEBUG
                Debug.LogFormat(LOG_FORMAT, "Awake(), Display.displays.Length : " + Display.displays.Length);
#endif

                Instance = this;

                Debug.Assert(this.transform.position == Vector3.zero);
                Debug.Assert(this.transform.rotation == Quaternion.identity);
                Debug.AssertFormat(_camParentTransform != null, "_camParentTransform MUST BE SET in UnityEditor!!!!!!");
                Debug.Assert(_camParentTransform.rotation == Quaternion.identity);
                Debug.Assert(gizmoListener != null);
                gizmoRenderer.OnGizmoClicked += _OnGizmoClicked;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            gizmoRenderer.OnGizmoClicked -= _OnGizmoClicked;

            Instance = null;
        }

        protected virtual void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable() - _Camera.transform.SetParent(CamParentTransform, true);");

            // CamParentTransform.rotation = _Camera.transform.rotation;
            _Camera.transform.SetParent(CamParentTransform, true);
        }

        protected virtual void OnDisable()
        {
            _Camera.transform.SetParent(null, true);
        }

        protected void _OnGizmoClicked(GizmoComponent component)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnGizmoClicked(), component : <color=yellow><b>" + component + "</b></color>");

            if (OnGizmoClicked != null)
            {
                OnGizmoClicked(component);
            }
        }
    }
}
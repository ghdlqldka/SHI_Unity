using RuntimeSceneGizmo;
using UnityEngine;

namespace _Magenta_WebGL
{
	public class MWGL_GizmoMaxManager : _Magenta_Framework.GizmoMaxManagerEx
    {
        private static string LOG_FORMAT = "<color=#F346C2><b>[MWGL_GizmoMaxManager]</b></color> {0}";

        public static new MWGL_GizmoMaxManager Instance
        {
            get
            {
                return _instance as MWGL_GizmoMaxManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        public new MWGL_GizmoMaxCamera MaxCamera
        {
            get
            {
                return _maxCamera as MWGL_GizmoMaxCamera;
            }
        }

        protected override void Awake()
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
                Debug.Assert(_maxCamera != null);
                Debug.Assert(_maxCamera.enabled == false);
                Debug.Assert(_Camera.transform.localPosition == Vector3.zero);
                Debug.Assert(_Camera.transform.localRotation == Quaternion.identity);

                _gizmoListener = MaxCamera.gameObject.GetComponent<_GizmoListener>();
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

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            gizmoRenderer.OnGizmoClicked -= _OnGizmoClicked;

            Instance = null;
        }

        protected override void OnEnable()
        {
            Debug.LogFormat(LOG_FORMAT, "OnEnable() - _Camera.transform.SetParent(CamParentTransform, true);");

            // CamParentTransform.rotation = _Camera.transform.rotation;
            // _Camera.transform.SetParent(CamParentTransform, true);
            _maxCamera.enabled = true;
        }

        protected override void OnDisable()
        {
            _maxCamera.enabled = false;
            // _Camera.transform.SetParent(null, true);
        }
    }
}
using System.Collections;
using UnityEngine;
using static _Utilities;

namespace RuntimeSceneGizmo
{
	public class _GizmoListener : CameraGizmoListener
    {
        private static string LOG_FORMAT = "<color=#49F370><b>[_GizmoListener]</b></color> {0}";

        // protected Camera mainCamera;
        protected Camera _camera
        {
            get
            {
                return mainCamera;
            }
            set
            {
                mainCamera = value;
            }
        }
        // protected Transform mainCamParent;
        protected Transform camParentTransform
        {
            get
            {
                return _RuntimeGizmoManager.Instance.CamParentTransform;
            }
        }

        [Space(10)]
        [SerializeField]
        protected Transform _target;
        public Transform Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
            }
        }

        // protected IEnumerator cameraRotateCoroutine, projectionChangeCoroutine;
        protected Coroutine _cameraRotateCoroutine;
        protected Coroutine _projectionChangeCoroutine;

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            // mainCamera = Camera.main;
            // mainCamParent = mainCamera.transform.parent;
            mainCamParent = null; // Not used!!!!!

            cameraRotateCoroutine = null; // Not used!!!!!
            projectionChangeCoroutine = null; // Not used!!!!!

            StartCoroutine(PostAwake());
        }

        protected virtual IEnumerator PostAwake()
        {
            while (_RuntimeGizmoManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            _camera = _RuntimeGizmoManager.Instance._Camera;
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(PostOnEnable());
        }

        protected virtual IEnumerator PostOnEnable()
        {
            while (_RuntimeGizmoManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            _RuntimeGizmoManager.Instance.OnGizmoClicked += OnGizmoComponentClicked;
        }

        protected override void OnDisable()
        {
            // base.OnDisable();
            cameraRotateCoroutine = projectionChangeCoroutine = null;
            StopAllCoroutines();
            _cameraRotateCoroutine = null;
            _projectionChangeCoroutine = null;

            _RuntimeGizmoManager.Instance.OnGizmoClicked -= OnGizmoComponentClicked;
        }

        public override void OnGizmoComponentClicked(GizmoComponent component)
        {
            Debug.LogFormat(LOG_FORMAT, "OnGizmoComponentClicked(), component : " + component);

            switch (component)
            {
                case GizmoComponent.Center:
                    if (_projectionChangeCoroutine != null)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "Skip it");
                        return;
                    }
                    _projectionChangeCoroutine = SwitchOrthographicMode(_camera, OnProjectionSwitched);
                    break;

                case GizmoComponent.XNegative:
                    RotateCameraInDirection(Target, Vector3.right);
                    break;
                case GizmoComponent.XPositive:
                    RotateCameraInDirection(Target, -Vector3.right);
                    break;
                case GizmoComponent.YNegative:
                    RotateCameraInDirection(Target, Vector3.up);
                    break;
                case GizmoComponent.YPositive:
                    RotateCameraInDirection(Target, -Vector3.up);
                    break;
                case GizmoComponent.ZNegative:
                    RotateCameraInDirection(Target, Vector3.forward);
                    break;
                case GizmoComponent.ZPositive:
                    RotateCameraInDirection(Target, -Vector3.forward);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        public override void SwitchOrthographicMode()
        {
            throw new System.NotSupportedException("");
        }

        public virtual Coroutine SwitchOrthographicMode(Camera camera, EndCallback endCallback = null)
        {
            return StartCoroutine(SwitchProjection(camera, endCallback));
        }

        protected virtual void OnProjectionSwitched()
        {
            Debug.LogFormat(LOG_FORMAT, "OnProjectionSwitched()");

            // projectionChangeCoroutine = null;
            _projectionChangeCoroutine = null;
        }

        // Credit: https://forum.unity.com/threads/smooth-transition-between-perspective-and-orthographic-modes.32765/#post-212814
        protected override IEnumerator SwitchProjection()
        {
            throw new System.NotSupportedException("");
        }

        protected static float _projectionTransitionSpeed = 2f;
        protected static IEnumerator SwitchProjection(Camera camera, EndCallback endCallback = null)
        {
            bool isOrthographic = camera.orthographic;

            Matrix4x4 dest, src = camera.projectionMatrix;
            if (isOrthographic == true)
            {
                dest = Matrix4x4.Perspective(camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
            }
            else
            {
                float orthographicSize = camera.orthographicSize;
                float aspect = camera.aspect;
                dest = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, camera.nearClipPlane, camera.farClipPlane);
            }

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * _projectionTransitionSpeed)
            {
                float lerpModifier = isOrthographic ? t * t : Mathf.Pow(t, 0.2f);
                Matrix4x4 matrix = new Matrix4x4();
                for (int i = 0; i < 16; i++)
                    matrix[i] = Mathf.LerpUnclamped(src[i], dest[i], lerpModifier);

                camera.projectionMatrix = matrix;
                yield return null;
            }

            camera.orthographic = !isOrthographic;
            camera.ResetProjectionMatrix();

            if (endCallback != null)
            {
                endCallback();
            }
        }

        public override void RotateCameraInDirection(Vector3 direction)
        {
            throw new System.NotSupportedException("");
        }

        public virtual void RotateCameraInDirection(Transform target, Vector3 direction)
        {
            if (_cameraRotateCoroutine != null)
            {
                return;
            }

            if (target != null)
            {
                direction = target.TransformDirection(direction);
            }
            _cameraRotateCoroutine = StartCoroutine(SetCameraRotation(direction));
        }

        protected override IEnumerator SetCameraRotation(Vector3 targetForward)
        {
            Quaternion initialRotation = camParentTransform.localRotation;
            Quaternion targetRotation;
            if (Mathf.Abs(targetForward.y) < 0.99f)
            {
                targetRotation = Quaternion.LookRotation(targetForward);
            }
            else
            {
                Vector3 cameraForward = camParentTransform.forward;
                if (cameraForward.x == 0f && cameraForward.z == 0f)
                {
                    cameraForward.y = 1f;
                }
                else if (Mathf.Abs(cameraForward.x) > Mathf.Abs(cameraForward.z))
                {
                    cameraForward.x = Mathf.Sign(cameraForward.x);
                    cameraForward.y = 0f;
                    cameraForward.z = 0f;
                }
                else
                {
                    cameraForward.x = 0f;
                    cameraForward.y = 0f;
                    cameraForward.z = Mathf.Sign(cameraForward.z);
                }

                if (targetForward.y > 0f)
                {
                    cameraForward = -cameraForward;
                }

                targetRotation = Quaternion.LookRotation(targetForward, cameraForward);
            }

            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime * cameraAdjustmentSpeed)
            {
                camParentTransform.localRotation = Quaternion.LerpUnclamped(initialRotation, targetRotation, t);
                yield return null;
            }

            camParentTransform.localRotation = targetRotation;
            // cameraRotateCoroutine = null;
            _cameraRotateCoroutine = null;
        }
    }
}
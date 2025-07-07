//
//Filename: MaxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Linq;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using RuntimeSceneGizmo;

namespace _Base_Framework
{
    // [AddComponentMenu("Camera-Control/3dsMax Camera Style")]
    // [RequireComponent(typeof(Camera))]
    public class _GizmoMaxCamera : _Base_Framework._MaxCamera
    {
        private static string LOG_FORMAT = "<color=#8145B5><b>[_GizmoMaxCamera]</b></color> {0}";

        protected Coroutine _projectionSwitchCoroutine;

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
            // base.Awake();

            // _cam = this.GetComponent<Camera>();

            layerMask = 0; // Not used!!!!

            rotation = Quaternion.identity; // Not used!!!!!
            position = Vector3.negativeInfinity; // Not used!!!!!

#if ENABLE_INPUT_SYSTEM
            rotationButton = Mouse.current.rightButton;
            panningButton = Mouse.current.leftButton;
#endif

            StartCoroutine(PostAwake());
        }

        public override void Init()
        {
            // base.Init();

            rotspeed = 0;

            GameObject obj = new GameObject("===> Cam Target <===");
            _camTargetTransform = obj.transform;
#if DEBUG
            if (show_DEBUG_Sphere == true)
            {
                GameObject sphereObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereObj.name = "DEBUG_Sphere";
                Destroy(sphereObj.GetComponent<SphereCollider>());

                Material material = Resources.Load<Material>("Red - Fade");
                Debug.Assert(material != null);
                Renderer sphereRenderer = sphereObj.GetComponent<Renderer>();
                sphereRenderer.material = material;
                sphereRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sphereRenderer.receiveShadows = false;
                sphereObj.transform.SetParent(CamTargetTransform, false);
            }
#endif

            // If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
            if (Target == null)
            {
                Target = CamTargetTransform;
            }
            else
            {
                CamTargetTransform.SetParent(Target, false);
            }

            orgCamTargetPosition = CamTargetTransform.position;
            CamTargetTransform.position += targetOffset;

            // CamTargetTransform.position = this.transform.position + (this.transform.forward * distance);
            distance = Vector3.Distance(this.transform.position, CamTargetTransform.position);
            Debug.LogWarningFormat(LOG_FORMAT, "<color=red>Init Distance : <b>" + distance + "</b></color>");
            this.transform.position = CamTargetTransform.position - (this.transform.forward * distance);

            Debug.LogFormat(LOG_FORMAT, "Init(), Target : <b><color=yellow>" + Target.gameObject.name + "</color></b>");

            distance = Vector3.Distance(this.transform.position, CamTargetTransform.position);
            currentDistance = distance;
            desiredDistance = distance;

            desiredRotation = this.transform.rotation;
            Vector3 cross = Vector3.Cross(Vector3.right, this.transform.right);
            _xDeg = Vector3.Angle(Vector3.right, this.transform.right);
            if (cross.y < 0)
            {
                _xDeg = 360 - _xDeg;
            }
            _yDeg = Vector3.Angle(Vector3.up, this.transform.up);

            geomPlane = new Plane(Vector3.up, Vector3.zero);
#if DEBUG
            if (show_DEBUG_Plane == true)
            {
                GameObject planeObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                planeObj.name = "=====> DEBUG plane <=====";
                planeObj.transform.position = Vector3.zero;
                planeObj.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                planeObj.transform.localScale = new Vector3(1000, 0.001f, 1000);

                Destroy(planeObj.GetComponent<Collider>());
                Renderer planeRender = planeObj.GetComponent<Renderer>();
                planeRender.material.color = Color.green;
                planeRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                planeRender.receiveShadows = false;
            }
#endif
        }

        protected virtual IEnumerator PostAwake()
        {
            while (_GizmoMaxManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "");
                yield return null;
            }

            _cam = _GizmoMaxManager.Instance._Camera;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            StartCoroutine(PostOnEnable());
        }

        protected virtual IEnumerator PostOnEnable()
        {
            while (_GizmoMaxManager.Instance == null)
            {
                yield return null;
            }

            _GizmoMaxManager.Instance.OnGizmoClicked += OnGizmoClicked;
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            _rotateCoroutine = null;

            _GizmoMaxManager.Instance.OnGizmoClicked -= OnGizmoClicked;
        }

        // Mapping Notes:
        // Mouse.leftButton   ¡æ Touch: 1 finger tap or drag
        // Mouse.rightButton  ¡æ Touch: 2-finger drag or long press (implementation-dependent)
        // Mouse.middleButton ¡æ Touch: 3-finger tap (used for camera reset)
        protected override void LateUpdate()
        {
            if (_rotateCoroutine != null || _projectionSwitchCoroutine != null)
            {
                return;
            }

            // base.LateUpdate();

            bool outsideGUI = !_Base_Framework._EventSystem.IsPointerOverGameObject();

            if (IsRotateInteraction())
            {
                DoRotate();
            }

            else if (IsPanningInteraction() && outsideGUI)
            {
                DoPanning();
            }

#if ENABLE_INPUT_SYSTEM
            // Mouse.middleButton ¡æ Touch: 3-finger tap
            else if (((Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame) || 
                (Touchscreen.current != null && Touchscreen.current.touches.Count(t => t.press.wasPressedThisFrame) >= 3)) && 
                outsideGUI)
#else
            else if (Input.GetMouseButtonDown(2) && outsideGUI)
#endif
            {
                Debug.LogFormat(LOG_FORMAT, "Reset");

                CamTargetTransform.position = orgCamTargetPosition;
                CamTargetTransform.position += targetOffset;
            }

            // #region anim
            if (anim == true)
            {
                DoAnim();
            }

            Quaternion currentRotation = this.transform.rotation;
            Quaternion _rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            this.transform.rotation = _rotation;
            // #endregion
            ////////Orbit Position

            // affect the desired Zoom distance if we roll the scrollwheel
            float scrollinp = 0.0f;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                scrollinp = 0.01f * Mouse.current.scroll.ReadValue().y;
            }
#else
            scrollinp = Input.GetAxis("Mouse ScrollWheel");
#endif

#if UNITY_WEBGL
            //scrollinp *= 0.1f;
#endif

#if ENABLE_INPUT_SYSTEM
            if (EnhancedTouch.Touch.activeTouches.Count > 1)
            {
                Vector2 d1 = EnhancedTouch.Touch.activeTouches[0].screenPosition;
                Vector2 d2 = EnhancedTouch.Touch.activeTouches[1].screenPosition;
                float newpinchdistance = Vector2.Distance(d1, d2);
                scrollinp = 0.0005f * (newpinchdistance - pinchdistance);
                pinchdistance = newpinchdistance;
            }
#else
            if (Input.touchCount > 1)
            {
                float newpinchdistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                scrollinp = 0.0005f * (newpinchdistance - pinchdistance);
                pinchdistance = newpinchdistance;
            }
#endif

            desiredDistance -= scrollinp * Time.deltaTime * ZoomSpeed * Mathf.Abs(desiredDistance);
            //clamp the zoom min/max
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            // For smoothing of the zoom, lerp distance
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

            // calculate position based on the new currentDistance 
            Vector3 _position = CamTargetTransform.position - (_rotation * Vector3.forward * currentDistance);
            this.transform.position = _position;

            if (plane != null)
            {
                if (this.transform.position.y < plane.transform.position.y)
                {
                    this.transform.position = new Vector3(this.transform.position.x, plane.transform.position.y, this.transform.position.z);
                }
            }

        }

        protected override bool IsRotateInteraction()
        {
#if ENABLE_INPUT_SYSTEM
            // Avoid NullReferenceException on Android (no Mouse.current)
            if (Mouse.current != null && rotationButton.isPressed)
            {
                return true;
            }

            // Touch input: use single-finger drag for rotation
            if (Touchscreen.current != null &&
                EnhancedTouch.Touch.activeTouches.Count == 1 && Touchscreen.current.primaryTouch.press.isPressed)
            {
                return true;
            }
#else
            // Old input backends are enabled.
            if (Input.GetMouseButton(1))
            {
                 return true;
            }
#endif

            return false;
        }

        protected override void DoRotate()
        {
#if ENABLE_INPUT_SYSTEM
            // New Input System
            if ((Mouse.current != null && rotationButton.isPressed) ||
                (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed))
            {
                // Initialize rotation angle from current camera rotation when starting drag
                if (Mouse.current != null && rotationButton.wasPressedThisFrame)
                {
                    Vector3 currentEuler = this.transform.rotation.eulerAngles;
                    _xDeg = NormalizeAngle(currentEuler.y); // Yaw (horizontal)
                    _yDeg = NormalizeAngle(currentEuler.x); // Pitch (vertical)
                    desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0f);
                }
                else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                {
                    Vector2 delta = Touchscreen.current.primaryTouch.delta.ReadValue();
                    _xDeg += delta.x * xSpeed * 0.002f;
                    _yDeg -= delta.y * ySpeed * 0.002f;

                    _yDeg = ClampAngle(_yDeg, yMinLimit, yMaxLimit);
                    desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0f);
                }
                else
                {
                    // Accumulate angle based on mouse movement delta
                    _xDeg += Mouse.current.delta.ReadValue().x * xSpeed * 0.002f;
                    _yDeg -= Mouse.current.delta.ReadValue().y * ySpeed * 0.002f;

                    _yDeg = ClampAngle(_yDeg, yMinLimit, yMaxLimit);

                    desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0f);
                }
            }
#else // ENABLE_INPUT_SYSTEM
            // Legacy Input
            if (Input.GetMouseButton(1))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    Vector3 currentEuler = transform.rotation.eulerAngles;
                    _xDeg = NormalizeAngle(currentEuler.y);
                    _yDeg = NormalizeAngle(currentEuler.x);
                    desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0f);
                }
                else
                {
                    _xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    _yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                    _yDeg = ClampAngle(_yDeg, yMinLimit, yMaxLimit);

                    desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0f);
                }
            }
#endif // ENABLE_INPUT_SYSTEM

#if ENABLE_INPUT_SYSTEM
            // Check pinch distance for multi-touch input
            if (EnhancedTouch.Touch.activeTouches.Count > 1)
            {
                pinchdistance = Vector2.Distance(
                    EnhancedTouch.Touch.activeTouches[0].screenPosition,
                    EnhancedTouch.Touch.activeTouches[1].screenPosition);
            }
#else
            if (Input.touchCount > 1)
            {
                pinchdistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            }
#endif
        }

        protected override bool IsPanningInteraction()
        {
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && panningButton.wasPressedThisFrame)
            {
                return true;
            }

            if (EnhancedTouch.Touch.activeTouches.Count == 2)
            {
                var t0 = EnhancedTouch.Touch.activeTouches[0];
                var t1 = EnhancedTouch.Touch.activeTouches[1];

                if (t0.phase == UnityEngine.InputSystem.TouchPhase.Moved &&
                    t1.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    return true;
                }
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }
#endif

            return false;
        }

        protected override void DoPanning()
        {
            RaycastHit hit;
            Vector3 pos = Vector3.zero;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                pos = Mouse.current.position.ReadValue();
            }
            else if (EnhancedTouch.Touch.activeTouches.Count == 2)
            {
                Vector2 avg = 0.5f * (EnhancedTouch.Touch.activeTouches[0].screenPosition + EnhancedTouch.Touch.activeTouches[1].screenPosition);
                pos = new Vector3(avg.x, avg.y, 0);
            }
            else
            {
                Debug.Assert(false);
            }
#else
            pos = Input.mousePosition;
#endif

            // Ray ray = Camera.main.ScreenPointToRay(
            Ray ray = _cam.ScreenPointToRay(pos);

            bool _hit = false;
            if (plane != null)
            {
                _hit = plane.Raycast(ray, out hit, 1000f);
            }
            else
            {
                // Plane geomPlane = new Plane(Vector3.up, Vector3.zero);
                float enter = 0f;
                _hit = geomPlane.Raycast(ray, out enter);
            }

            if (_hit == true)
            {
                Debug.LogFormat(LOG_FORMAT, "_hit");

                if (Panning == false)
                {
                    StartCoroutine(DragTarget(pos));
                }
            }
            else
            {
                Panning = false;
            }
        }

        protected virtual void OnGizmoClicked(GizmoComponent component)
        {
            Debug.LogFormat(LOG_FORMAT, "OnGizmoClicked(), component : <b>" + component + "</b>");

            if (_rotateCoroutine != null || _projectionSwitchCoroutine != null)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "busy~~~~~~~~~~~~~");
                // skip it
                return;
            }

            switch (component)
            {
                case GizmoComponent.Center:
                    // SwitchOrthographicMode();
                    _projectionSwitchCoroutine = _GizmoMaxManager.Instance.gizmoListener.SwitchOrthographicMode(_cam, OnProjectionSwitched);
                    break;
                case GizmoComponent.XNegative:
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(Vector3.right));
                    break;
                case GizmoComponent.XPositive:
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(-Vector3.right));
                    break;
                case GizmoComponent.YNegative:
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(Vector3.up));
                    break;
                case GizmoComponent.YPositive:
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(-Vector3.up));
                    break;
                case GizmoComponent.ZNegative:
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(Vector3.forward));
                    break;
                case GizmoComponent.ZPositive:
                    _rotateCoroutine = StartCoroutine(RotateCoroutine(-Vector3.forward));
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
        }

        protected virtual void OnProjectionSwitched()
        {
            Debug.LogFormat(LOG_FORMAT, "OnProjectionSwitched()");

            // projectionChangeCoroutine = null;
            _projectionSwitchCoroutine = null;
        }

#if true // ChatGPT
        protected Coroutine _rotateCoroutine;
        [SerializeField]
        protected float angleLerpSpeed = 500f;
        protected virtual IEnumerator RotateCoroutine(Vector3 direction)
        {
            Debug.LogFormat(LOG_FORMAT, "RotateCoroutine(), direction : " + direction);

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            Vector3 targetEuler = targetRotation.eulerAngles;

            float startX = _xDeg;
            float startY = _yDeg;

            float targetX = startX + Mathf.DeltaAngle(startX, targetEuler.x);
            float targetY = startY + Mathf.DeltaAngle(startY, targetEuler.y);

            float elapsed = 0f;
            float maxDuration = 1f;

            while (elapsed < maxDuration)
            {
                elapsed += Time.deltaTime;

                _xDeg = Mathf.MoveTowardsAngle(_xDeg, targetX, angleLerpSpeed * Time.deltaTime);
                _yDeg = Mathf.MoveTowardsAngle(_yDeg, targetY, angleLerpSpeed * Time.deltaTime);

                desiredRotation = Quaternion.Euler(_xDeg, _yDeg, 0f);

                bool doneX = Mathf.Abs(Mathf.DeltaAngle(_xDeg, targetX)) < 0.1f;
                bool doneY = Mathf.Abs(Mathf.DeltaAngle(_yDeg, targetY)) < 0.1f;

                if (doneX && doneY)
                {
                    break;
                }

                yield return null;
            }

            _xDeg = targetX;
            _yDeg = targetY;
            desiredRotation = Quaternion.Euler(_xDeg, _yDeg, 0f);

            _rotateCoroutine = null;
        }
#endif // ChatGPT
    }
}
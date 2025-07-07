//
//Filename: MaxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Linq;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;
using RuntimeSceneGizmo;

namespace _Magenta_WebGL
{
    // [AddComponentMenu("Camera-Control/3dsMax Camera Style")]
    [RequireComponent(typeof(_GizmoListener))]
    public class MWGL_GizmoMaxCamera : _Magenta_Framework.GizmoMaxCameraEx
    {
        private static string LOG_FORMAT = "<color=#8145B5><b>[MWGL_GizmoMaxCamera]</b></color> {0}";

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

            Debug.LogFormat(LOG_FORMAT, "Init(), Target : " + Target.gameObject.name);

            distance = Vector3.Distance(this.transform.position, CamTargetTransform.position);
            currentDistance = distance;
            desiredDistance = distance;

            //be sure to grab the current rotations as starting points.
            // position = this.transform.position;
            // rotation = this.transform.rotation;
            // currentRotation = this.transform.rotation;
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

            bool outsideGUI = !MWGL_EventSystem.IsPointerOverGameObject();

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
            if (EnhancedTouch.Touch.activeTouches.Count == 2)
            {
                Vector2 d1 = EnhancedTouch.Touch.activeTouches[0].screenPosition;
                Vector2 d2 = EnhancedTouch.Touch.activeTouches[1].screenPosition;
                float newpinchdistance = Vector2.Distance(d1, d2);
                // scrollinp = 0.0005f * (newpinchdistance - pinchdistance);
                scrollinp = 0.00025f * (newpinchdistance - pinchdistance);
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
        }
    }
}
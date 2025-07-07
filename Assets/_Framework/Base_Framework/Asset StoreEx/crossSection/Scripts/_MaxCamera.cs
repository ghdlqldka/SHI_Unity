//
//Filename: MaxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _CrossSection
{
    // [AddComponentMenu("Camera-Control/3dsMax Camera Style")]
    public class _MaxCamera : MaxCamera
    {
        private static string LOG_FORMAT = "<color=magenta><b>[_MaxCamera]</b></color> {0}";

        protected Camera _cam;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
            _cam = this.GetComponent<Camera>();
        }

        protected override void LateUpdate()
        {
            bool outsideNewGUI = (eventsystem.currentSelectedGameObject == null);

#if ENABLE_INPUT_SYSTEM
            // New input system backends are enabled.
            if (Mouse.current.rightButton.isPressed)
            {
                if (!Mouse.current.rightButton.wasPressedThisFrame)
                {
                    xDeg += Mouse.current.delta.ReadValue().x * xSpeed * 0.002f;
                    yDeg -= Mouse.current.delta.ReadValue().y * ySpeed * 0.002f;

                    ////////OrbitAngle

                    //Clamp the vertical axis for the orbit
                    yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

                    // set camera rotation 
                    desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                }
                else
                {
                    if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 1)
                        pinchdistance = Vector2.Distance(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition,
                            UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition);
                }
            }
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
            else if (Mouse.current.leftButton.wasPressedThisFrame && outsideNewGUI)
            {
                RaycastHit hit;
                // Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

                bool _hit = false;
                if (plane)
                {
                    _hit = plane.Raycast(ray, out hit, 1000f);
                }
                else
                {
                    Plane geomPlane = new Plane(Vector3.up, Vector3.zero);
                    float enter = 0f;
                    _hit = geomPlane.Raycast(ray, out enter);
                }

                if (_hit)
                {
                    Debug.Log("_hit");
                    if (Physics.Raycast(ray, out hit, 1000f, layerMask))
                    {
                        dragging = false;
                    }
                    else
                    {
                        if (!dragging)
                            StartCoroutine(DragTarget(Mouse.current.position.ReadValue()));
                    }
                }
                else
                {
                    dragging = false;
                }
            }
#else
            // Old input backends are enabled.
            if (Input.GetMouseButton(1))
            {
                if (!Input.GetMouseButtonDown(1))
                {
                    xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                    ////////OrbitAngle

                    //Clamp the vertical axis for the orbit
                    yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

                    // set camera rotation 
                    desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                }
                else
                {
                    if (Input.touchCount > 1) 
                        pinchdistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                }
            }
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace

            else if (Input.GetMouseButtonDown(0) && outsideNewGUI)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                bool _hit = false;
                if (plane)
                {
                    _hit = plane.Raycast(ray, out hit, 1000f);
                }
                else
                {
                    Plane geomPlane = new Plane(Vector3.up, Vector3.zero);
                    float enter = 0f;
                    _hit = geomPlane.Raycast(ray, out enter);
                }

                if (_hit)
                {
                    Debug.Log("_hit");
                    if (Physics.Raycast(ray, out hit, 1000f, layerMask))
                    {
                        dragging = false;
                    }
                    else
                    {
                        if (!dragging)
                            StartCoroutine(DragTarget(Input.mousePosition));
                    }
                }
                else
                {
                    dragging = false;
                }
            }
#endif

#region anim
            if (anim)
            {
                rotspeed = Mathf.Min(rotspeed + 0.0006f, 0.2f);
                xDeg += a * rotspeed;
                desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            }

            currentRotation = this.transform.rotation;
            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            this.transform.rotation = rotation;
#endregion
            ////////Orbit Position

            // affect the desired Zoom distance if we roll the scrollwheel
#if ENABLE_INPUT_SYSTEM
            float scrollinp = 0.01f * Mouse.current.scroll.ReadValue().y;
#else
            float scrollinp = Input.GetAxis("Mouse ScrollWheel");
#endif


#if UNITY_WEBGL
        //scrollinp *= 0.1f;
#endif

#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 1)
            {
                float newpinchdistance = Vector2.Distance(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition,
                    UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition);

                scrollinp = 0.0005f * (newpinchdistance - pinchdistance);
                pinchdistance = newpinchdistance;
            }

            desiredDistance -= scrollinp * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
            //clamp the zoom min/max
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            // For smoothing of the zoom, lerp distance
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

            // calculate position based on the new currentDistance 
            position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
            this.transform.position = position;

            if (plane)
            {
                if (this.transform.position.y < plane.transform.position.y)
                    this.transform.position = new Vector3(this.transform.position.x, plane.transform.position.y, this.transform.position.z);
            }
#else

            if (Input.touchCount > 1)
            {
                float newpinchdistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                                scrollinp = 0.0005f * (newpinchdistance - pinchdistance);
                pinchdistance = newpinchdistance;
            }

            desiredDistance -= scrollinp * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
            //clamp the zoom min/max
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            // For smoothing of the zoom, lerp distance
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

            // calculate position based on the new currentDistance 
            position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
            this.transform.position = position;

            if (plane)
            {
                if (this.transform.position.y < plane.transform.position.y)
                    this.transform.position = new Vector3(this.transform.position.x, plane.transform.position.y, this.transform.position.z);
            }
#endif

        }
    }
}
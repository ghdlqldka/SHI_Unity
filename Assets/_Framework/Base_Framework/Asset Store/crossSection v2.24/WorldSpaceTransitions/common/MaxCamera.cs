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

[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class MaxCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset;
    public float distance = 5.0f;
    public float maxDistance = 20;
    public float minDistance = .6f;
    public float xSpeed = 200.0f;
    public float ySpeed = 200.0f;
    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public int zoomRate = 40;
    public float panSpeed = 0.3f;
    public float zoomDampening = 5.0f;

    public LayerMask layerMask = -1;

    //[HideInInspector]
    public MeshCollider plane;

    protected float xDeg = 0.0f;
    protected float yDeg = 0.0f;
    protected float currentDistance;
    [HideInInspector]
    public float desiredDistance;
    protected Quaternion currentRotation;
    protected Quaternion desiredRotation;
    protected Quaternion rotation;
    protected Vector3 position;

    protected bool dragging = false;

    protected float pinchdistance = 0;

    protected EventSystem eventsystem;
    //
    [HideInInspector]
    protected float rotspeed = 0;
    [HideInInspector]
    public bool anim = false;
    [HideInInspector]
    [Range(0.5f,3f)]
    public float a = 1.5f;
 
    protected virtual void Start() 
    {
#pragma warning disable 0618
        eventsystem = FindObjectOfType<EventSystem>() as EventSystem;
#pragma warning restore 0618
        Init(); 
    }
    protected virtual void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
#endif
        Init();
    }

 
    public virtual void Init()
    {
        //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
        rotspeed = 0;
        if (!target)
        {
            GameObject go = new GameObject("Cam Target");
            go.transform.position = transform.position + (transform.forward * distance);
            target = go.transform;
        }
 
        distance = Vector3.Distance(transform.position, target.position);
        currentDistance = distance;
        desiredDistance = distance;
 
        //be sure to grab the current rotations as starting points.
        position = transform.position;
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;
        Vector3 cross = Vector3.Cross(Vector3.right, transform.right);
        xDeg = Vector3.Angle(Vector3.right, transform.right );
        if (cross.y < 0) xDeg = 360 - xDeg;
        yDeg = Vector3.Angle(Vector3.up, transform.up );
    }
 
    /*
     * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
     */
    protected virtual void LateUpdate()
    {
        bool outsideNewGUI = (eventsystem.currentSelectedGameObject == null);

/*
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            outsideNewGUI = (!EventSystem.current.IsPointerOverGameObject(touch.fingerId) && touch.phase != TouchPhase.Ended) ;

        }
#endif
*/

#if ENABLE_INPUT_SYSTEM
        // New input system backends are enabled.
        if (Mouse.current.rightButton.isPressed)
        { 
            if (!Mouse.current.rightButton.wasPressedThisFrame)
            {
                xDeg += Mouse.current.delta.ReadValue().x * xSpeed * 0.002f;
                yDeg -= Mouse.current.delta.ReadValue().y * ySpeed * 0.002f;
#else
//#endif
//#if ENABLE_LEGACY_INPUT_MANAGER
            // Old input backends are enabled.
        if (Input.GetMouseButton(1))
        {
            
            if (!Input.GetMouseButtonDown(1))
            {
                xDeg += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                yDeg -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
#endif
                ////////OrbitAngle

                //Clamp the vertical axis for the orbit
                yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);

                // set camera rotation 
                desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
                //currentRotation = transform.rotation;
                //rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
                //transform.rotation = rotation;
            }
            else
            {
#if ENABLE_INPUT_SYSTEM
                if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 1)
                    pinchdistance = Vector2.Distance(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition,
                        UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition);
#else

                if (Input.touchCount > 1) pinchdistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
#endif
            }

        }
        // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
        else if (
#if ENABLE_INPUT_SYSTEM
            Mouse.current.leftButton.wasPressedThisFrame
#else

            Input.GetMouseButtonDown(0) 
#endif
            && outsideNewGUI)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(
#if ENABLE_INPUT_SYSTEM
                    Mouse.current.position.ReadValue()
#else
                    Input.mousePosition 
#endif
                );
            bool _hit = false;
            if(plane)
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
                    if (!dragging) StartCoroutine(DragTarget(
#if ENABLE_INPUT_SYSTEM
                    Mouse.current.position.ReadValue()
#else
                    Input.mousePosition 
#endif
                        ));
                }
            }
            else
            {
                dragging = false;
            }


        }
        #region anim
        if (anim)
        {
            rotspeed = Mathf.Min(rotspeed + 0.0006f, 0.2f);
            xDeg += a*rotspeed;
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
        }

        currentRotation = transform.rotation;
        rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        transform.rotation = rotation;
        #endregion
        ////////Orbit Position

        // affect the desired Zoom distance if we roll the scrollwheel
        float scrollinp =
#if ENABLE_INPUT_SYSTEM
            0.01f * Mouse.current.scroll.ReadValue().y
#else

            Input.GetAxis("Mouse ScrollWheel") 
#endif
            ;
#if UNITY_WEBGL
        //scrollinp *= 0.1f;
#endif

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 1)
        {
            float newpinchdistance = Vector2.Distance(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition,
                UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1].screenPosition);
#else

        if (Input.touchCount > 1)
        {
            float newpinchdistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
#endif
            scrollinp = 0.0005f * (newpinchdistance - pinchdistance);
            pinchdistance = newpinchdistance;
        }

        desiredDistance -= scrollinp * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        //clamp the zoom min/max
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        // For smoothing of the zoom, lerp distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
        //currentRotation = transform.rotation;
        //rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        //transform.rotation = rotation;
 
        // calculate position based on the new currentDistance 
        position = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
        transform.position = position;

        //currentRotation = transform.rotation;
        //rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
        //transform.rotation = rotation;
        if (plane)
        {
            if (transform.position.y < plane.transform.position.y) 
                transform.position = new Vector3(transform.position.x, plane.transform.position.y, transform.position.z);
        }
    }

    protected virtual IEnumerator DragTarget(Vector3 startingHit)
    {
        dragging = true;
        Vector3 startTargetPos = target.position;
        while (
#if ENABLE_INPUT_SYSTEM
                Mouse.current.leftButton.isPressed && !Mouse.current.rightButton.isPressed
#else

                Input.GetMouseButton(0) && !Input.GetMouseButton(1)
#endif
            )
        {
            Vector3 mouseMove = 0.005f * transform.position.y * (
#if ENABLE_INPUT_SYSTEM
                new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y,0)
#else

                Input.mousePosition 
#endif
                - startingHit);
            //Vector3 translation = new Vector3(mouseMove.x, 0, mouseMove.y);
            //float clampVal = 0.04f * (transform.position.y - target.position.y);
            Vector3 zDir = transform.forward;
            zDir.y = 0;
            target.position = startTargetPos - transform.right * mouseMove.x - zDir.normalized * mouseMove.y;
            yield return null;
        }
        dragging = false;
    }
 
    protected static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;

        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace _Magenta_Framework
{

	[RequireComponent(typeof(Collider))]
	public class RotateObjectEx : _InputTouches._RotateObject
	{
		private static string LOG_FORMAT = "<color=magenta><b>[RotateObjectEx]</b></color> {0}";

		[Space(10)]
		[ReadOnly]
		[SerializeField]
		protected Camera cam;
		public Camera Cam
		{
			set
			{
				cam = value;
			}
		}

		protected override void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);
		}

		protected override void OnEnable()
		{
			IT_GestureEx.onDraggingStartE += OnDraggingStart;
            IT_GestureEx.onDraggingE += OnDragging;

            IT_GestureEx.onPinchE += OnPinch;

			orbitSpeedX = 0;
			orbitSpeedY = 0;
			zoomSpeed = 0;
		}

		protected override void OnDisable()
		{
            IT_GestureEx.onDraggingStartE -= OnDraggingStart;
            IT_GestureEx.onDraggingE -= OnDragging;

            IT_GestureEx.onPinchE -= OnPinch;
		}

        protected override void Update()
        {
            if (EventSystemEx.IsPointerOverGameObject() == true)
            {
                return;
            }

            //get the current rotation
            float x = this.transform.rotation.eulerAngles.x;
            float y = this.transform.rotation.eulerAngles.y;

            // make sure x is between -180 to 180 so we can clamp it propery later
            if (x > 180)
                x -= 360;

            //calculate the x and y rotation
            Quaternion rotationY = Quaternion.Euler(0, y, 0) * Quaternion.Euler(0, -orbitSpeedY, 0);
            Quaternion rotationX = Quaternion.Euler(Mathf.Clamp(x - orbitSpeedX, minRotX, maxRotX), 0, 0);

            //apply the rotation
            this.transform.rotation = rotationY * rotationX;

            _scale += Time.deltaTime * zoomSpeed * 0.01f;
            _scale = Mathf.Clamp(_scale, 0.5f, 3);
            this.transform.localScale = new Vector3(Mathf.Lerp(this.transform.localScale.x, _scale, Time.deltaTime * 10),
                    Mathf.Lerp(this.transform.localScale.y, _scale, Time.deltaTime * 10),
                    Mathf.Lerp(this.transform.localScale.z, _scale, Time.deltaTime * 10));

            //reduce all the speed
            orbitSpeedX *= (1 - Time.deltaTime * 12);
            orbitSpeedY *= (1 - Time.deltaTime * 3);
            zoomSpeed *= (1 - Time.deltaTime * 4);

            //use mouse scroll wheel to simulate pinch, sorry I sort of cheated here
            zoomSpeed += Input.GetAxis("Mouse ScrollWheel") * 500 * zoomSpeedModifier;
        }

        protected RaycastHit hit;
		[ReadOnly]
		[SerializeField]
		protected bool isRotationMode = false; // Rotate or Move
		protected virtual void OnDraggingStart(DragInfo dragInfo)
		{
			// Debug.LogFormat(LOG_FORMAT, "OnDraggingStart(), dragInfo.isMouse : " + dragInfo.isMouse);

			if (EventSystemEx.IsPointerOverGameObject() == true)
			{
				return;
			}
			// Debug.Log("a");

			if (dragInfo.isMouse && dragInfo.index == IT_GestureEx.Mouse_Left_Button)
			{
				//
			}
			else
			{
// #if UNITY_ANDROID
				{
					Ray ray = cam.ScreenPointToRay(Input.mousePosition);
					if (Physics.Raycast(ray, out hit) == true && hit.collider.gameObject.Equals(this.gameObject))
					{
						isRotationMode = false; // Move
						
					}
					else
					{
						isRotationMode = true; // Rotate
					}
				}
// #endif
			}
		}

		protected override void OnDragging(DragInfo dragInfo)
		{
			if (EventSystemEx.IsPointerOverGameObject() == true)
			{
				return;
			}
			// Debug.Log("a");

			// if the drag is perform using mouse2, use it as a two finger drag
			if (dragInfo.isMouse && dragInfo.index == IT_GestureEx.Mouse_Left_Button)
			{
				OnMFDragging(dragInfo);
			}
			else // else perform normal orbiting
			{
				if (isRotationMode == false) // Move
				{
					OnMFDragging(dragInfo);
				}
				else // Rotate
				{
					//vertical movement is corresponded to rotation in x-axis
					orbitSpeedX = -dragInfo.delta.y * rotXSpeedModifier;
					//horizontal movement is corresponded to rotation in y-axis
					orbitSpeedY = dragInfo.delta.x * rotYSpeedModifier;
				}
			}
		}
	}
}
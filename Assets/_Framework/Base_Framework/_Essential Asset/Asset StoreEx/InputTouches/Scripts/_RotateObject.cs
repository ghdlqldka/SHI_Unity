using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using _Base_Framework;

namespace _InputTouches
{

	public class _RotateObject : RotateObject
	{
		private static string LOG_FORMAT = "<color=magenta><b>[_RotateObject]</b></color> {0}";

		protected float _scale = 0;

		protected virtual void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);
		}

		// Use this for initialization
		protected override void Start()
		{
			// dist = transform.localPosition.z;
			_scale = this.transform.localScale.x;
		}

		protected override void OnEnable()
		{
			IT_Gesture.onDraggingE += OnDragging;
			// IT_Gesture.onMFDraggingE += OnMFDragging;

			IT_Gesture.onPinchE += OnPinch;

			orbitSpeedX = 0;
			orbitSpeedY = 0;
			zoomSpeed = 0;
		}

		protected override void OnDisable()
		{
			IT_Gesture.onDraggingE -= OnDragging;
			// IT_Gesture.onMFDraggingE -= OnMFDragging;

			IT_Gesture.onPinchE -= OnPinch;
		}

		// Update is called once per frame
		protected override void Update()
		{
			/*
			if (EventSystem.current)
			{
				bool mouseOutsideGUI = !EventSystem.current.IsPointerOverGameObject();
				if (mouseOutsideGUI == false)
				{
					return;
				}
			}
			*/
			if (_EventSystem.IsPointerOverGameObject() == true)
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

			/*
			//calculate the zoom and apply it
			dist += Time.deltaTime * zoomSpeed * 0.01f;
			dist = Mathf.Clamp(dist, -15, -3);
			transform.localPosition = new Vector3(0, 0, dist);
			*/
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

		protected override void OnDragging(DragInfo dragInfo)
		{
			/*
			if (EventSystem.current)
			{
				bool mouseOutsideGUI = !EventSystem.current.IsPointerOverGameObject();
				if (mouseOutsideGUI == false)
				{
					return;
				}
			}
			*/
			if (_EventSystem.IsPointerOverGameObject() == true)
			{
				return;
			}

			//if the drag is perform using mouse2, use it as a two finger drag
			if (dragInfo.isMouse && dragInfo.index == _IT_Gesture.Mouse_Left_Button)
			{
				OnMFDragging(dragInfo);
			}
			else //else perform normal orbiting
			{
				//vertical movement is corresponded to rotation in x-axis
				orbitSpeedX = -dragInfo.delta.y * rotXSpeedModifier;
				//horizontal movement is corresponded to rotation in y-axis
				orbitSpeedY = dragInfo.delta.x * rotYSpeedModifier;
			}
		}

		protected override void OnMFDragging(DragInfo dragInfo)
		{
			// base.OnMFDragging(dragInfo);

			//make a new direction, pointing horizontally at the direction of the camera y-rotation
			Quaternion direction = Quaternion.Euler(0, this.transform.parent.rotation.eulerAngles.y, 0);

			//calculate forward movement based on vertical input
			Vector3 moveDirZ = this.transform.parent.InverseTransformDirection(direction * Vector3.forward * dragInfo.delta.y);
			//calculate sideway movement base on horizontal input
			Vector3 moveDirX = this.transform.parent.InverseTransformDirection(direction * Vector3.right * dragInfo.delta.x);

			this.transform.parent.Translate(moveDirZ * panSpeedModifier * Time.deltaTime);
			this.transform.parent.Translate(moveDirX * panSpeedModifier * Time.deltaTime);
		}

		protected override void OnGUI()
		{
#if DEBUG
			string title = "RTS camera, the camera will orbit around a pivot point but the rotation in z-axis is locked.";
			GUI.Label(new Rect(150, 10, 400, 60), title);

			if (instruction == false)
			{
				if (GUI.Button(new Rect(10, 55, 130, 35), "Instruction On"))
				{
					instruction = true;
				}
			}
			else
			{
				if (GUI.Button(new Rect(10, 55, 130, 35), "Instruction Off"))
				{
					instruction = false;
				}

				GUI.Box(new Rect(10, 100, 400, 100), "");

				string instInfo = "";
				instInfo += "- swipe or drag on screen to rotate the camera\n";
				instInfo += "- pinch or using mouse wheel to zoom in/out\n";
				instInfo += "- swipe or drag on screen with 2 fingers to move around\n";
				instInfo += "- single finger interaction can be simulate using left mosue button\n";
				instInfo += "- two fingers interacion can be simulate using right mouse button";

				GUI.Label(new Rect(15, 105, 390, 90), instInfo);
			}
#endif
		}
	}
}
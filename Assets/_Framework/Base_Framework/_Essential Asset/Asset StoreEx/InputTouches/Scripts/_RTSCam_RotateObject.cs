using UnityEngine;
using System.Collections;

namespace _InputTouches
{
	[RequireComponent(typeof(Camera))]
	public class _RTSCam_RotateObject : RTSCam_RotateObject
	{
		// public float panSpeedModifier = 1f;

		protected Transform CameraParent
		{
			get;
			set;
		}

		protected Transform TargetParent
		{
			get
			{
				return targetP;
			}
			set
			{
				targetP = value;
			}
		}

		public Transform Target
		{
			get
			{
				return targetT;
			}
			set
			{
				targetT = value;
				if (targetT != null)
				{
					TargetParent.position = targetT.position;
					targetT.parent = TargetParent;
				}
			}
		}

		protected virtual void Awake()
		{
			CameraParent = this.transform.parent;
			Debug.Assert(CameraParent != null);

			//create a dummy transform as the parent of the target
			//we will rotate this parent along y-axis, so the target will only rotate along local x-axis
			GameObject obj = new GameObject("Target Parent");
			TargetParent = obj.transform;
		}

		protected override void OnEnable()
		{
			IT_Gesture.onDraggingE += OnDragging;
			// IT_Gesture.onMFDraggingE += OnMFDragging;

			IT_Gesture.onPinchE += OnPinch;
		}

		protected override void OnDisable()
		{
			IT_Gesture.onDraggingE -= OnDragging;
			// IT_Gesture.onMFDraggingE -= OnMFDragging;

			IT_Gesture.onPinchE -= OnPinch;
		}

		protected override void Start()
		{
			dist = this.transform.localPosition.z;

			TargetParent.position = targetT.position;
			targetT.parent = TargetParent;

			/*
			DemoSceneUI.SetSceneTitle("Alternate RTS camera, the object in focus moves instead of the camera");

			string instInfo = "";
			instInfo += "- swipe or drag on screen to rotate the camera\n";
			instInfo += "- pinch or using mouse wheel to zoom in/out\n";
			instInfo += "- swipe or drag on screen with 2 fingers to move around\n";
			instInfo += "- single finger interaction can be simulate using left mosue button\n";
			instInfo += "- two fingers interacion can be simulate using right mouse button";
			DemoSceneUI.SetSceneInstruction(instInfo);
			*/
		}

		// Update is called once per frame
		protected override void Update()
		{
			//original code
			//targetT.Rotate(new Vector3(-orbitSpeedX, -orbitSpeedY, 0), Space.World);

			// calculate the x-axis rotation, this is applied to the target get the current rotation
			float x = Target.localRotation.eulerAngles.x;
			//make sure x is between -180 to 180 so we can clamp it propery later
			if (x > 180)
				x -= 360;
			//calculate the x and y rotation
			Quaternion rotationX = Quaternion.Euler(Mathf.Clamp(x + orbitSpeedX, minRotX, maxRotX), 0, 0);
			//apply the rotation
			Target.localRotation = rotationX;

			//calculate the y-axis rotation, this is applied to the target's parent
			float y = TargetParent.localRotation.eulerAngles.y;
			Quaternion rotationY = Quaternion.Euler(0, y, 0) * Quaternion.Euler(0, orbitSpeedY, 0);
			TargetParent.rotation = rotationY;

			CameraParent.position = TargetParent.position;

			//calculate the zoom and apply it
			dist += Time.deltaTime * zoomSpeed * 0.01f;
			dist = Mathf.Clamp(dist, -15, -3);
			this.transform.localPosition = new Vector3(0, 0, dist);

			//reduce all the speed
			orbitSpeedX *= (1 - Time.deltaTime * 12);
			orbitSpeedY *= (1 - Time.deltaTime * 3);
			zoomSpeed *= (1 - Time.deltaTime * 4);

			//use mouse scroll wheel to simulate pinch, sorry I sort of cheated here
			zoomSpeed += Input.GetAxis("Mouse ScrollWheel") * 500 * zoomSpeedModifier;
		}

		protected override void OnDragging(DragInfo dragInfo)
		{
			//if the drag is perform using mouse2, use it as a two finger drag
			if (dragInfo.isMouse && dragInfo.index == _IT_Gesture.Mouse_Left_Button)
			{
				// OnMFDragging(dragInfo);
			}
			else //else perform normal orbiting
			{
				//apply the DPI scaling
				dragInfo.delta /= IT_Gesture.GetDPIFactor();
				//vertical movement is corresponded to rotation in x-axis
				orbitSpeedX = -dragInfo.delta.y * rotXSpeedModifier;
				//horizontal movement is corresponded to rotation in y-axis
				orbitSpeedY = dragInfo.delta.x * rotYSpeedModifier;
			}
		}

	}
}
using UnityEngine;
using System.Collections;

//using InputTouches;

namespace _InputTouches
{

	public class _BasicDetector : BasicDetector
	{
        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
		{
			if (Input.touchCount > 0)
			{
				for (int i = 0; i < Input.touches.Length; i++)
				{
					Touch touch = Input.touches[i];

					if (touch.phase == TouchPhase.Began)
					{
						IT_Gesture.OnTouchDown(touch);
					}
					else if (touch.phase == TouchPhase.Ended)
					{
						IT_Gesture.OnTouchUp(touch);
					}
					else
					{
						IT_Gesture.OnTouch(touch);
					}
				}
			}
			else if (Input.touchCount == 0)
			{
#if true
				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Left_Button))
					_IT_Gesture.OnMouseLeftButtonDown(Input.mousePosition);
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Left_Button))
					_IT_Gesture.OnMouseLeftButtonUp(Input.mousePosition);
				else if (Input.GetMouseButton(_IT_Gesture.Mouse_Left_Button))
					_IT_Gesture.OnMouseLeftButton(Input.mousePosition);

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Right_Button))
					_IT_Gesture.OnMouseRightButtonDown(Input.mousePosition);
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Right_Button))
					_IT_Gesture.OnMouseRightButtonUp(Input.mousePosition);
				else if (Input.GetMouseButton(_IT_Gesture.Mouse_Right_Button))
					_IT_Gesture.OnMouseRightButton(Input.mousePosition);

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Middle_Button))
					_IT_Gesture.OnMouseMiddleButtonDown(Input.mousePosition);
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Middle_Button))
					_IT_Gesture.OnMouseMiddleButtonUp(Input.mousePosition);
				else if (Input.GetMouseButton(_IT_Gesture.Mouse_Middle_Button))
					_IT_Gesture.OnMouseMiddleButton(Input.mousePosition);
#else
				//#if !(UNITY_ANDROID || UNITY_IPHONE) || UNITY_EDITOR
				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Left_Button))
					IT_Gesture.OnMouse1Down(Input.mousePosition);
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Left_Button))
					IT_Gesture.OnMouse1Up(Input.mousePosition);
				else if (Input.GetMouseButton(_IT_Gesture.Mouse_Left_Button))
					IT_Gesture.OnMouse1(Input.mousePosition);

				if (Input.GetMouseButtonDown(_IT_Gesture.Mouse_Right_Button))
					IT_Gesture.OnMouse2Down(Input.mousePosition);
				else if (Input.GetMouseButtonUp(_IT_Gesture.Mouse_Right_Button))
					IT_Gesture.OnMouse2Up(Input.mousePosition);
				else if (Input.GetMouseButton(_IT_Gesture.Mouse_Right_Button))
					IT_Gesture.OnMouse2(Input.mousePosition);
				//#endif
#endif
			}
		}


	}


}
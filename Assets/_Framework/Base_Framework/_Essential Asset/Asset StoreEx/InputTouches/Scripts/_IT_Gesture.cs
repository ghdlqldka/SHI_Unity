using UnityEngine;
using System.Collections;

namespace _InputTouches
{
	[RequireComponent(typeof(_BasicDetector))]
	[RequireComponent(typeof(_DragDetector))]
	[RequireComponent(typeof(_TapDetector))]
	[RequireComponent(typeof(SwipeDetector))]
	[RequireComponent(typeof(DualFingerDetector))]

	public class _IT_Gesture : IT_Gesture
	{
		// 
		private static string LOG_FORMAT = "<color=#94B530><b>[_IT_Gesture]</b></color> {0}";

		public static _IT_Gesture Instance
		{
			get
			{
				return instance as _IT_Gesture;
			}
			protected set
			{
				instance = value;
			}
		}

		public const int Mouse_Left_Button = 0;
		public const int Mouse_Right_Button = 1;
		public const int Mouse_Middle_Button = 2;

		// Mouse Left Button
		public delegate void MouseLeftButtonDownHandler(Vector3 pos);
		public static event MouseLeftButtonDownHandler onMouseLeftButtonDownE;

		public delegate void MouseMouseLeftButtonUpHandler(Vector3 pos);
		public static event MouseMouseLeftButtonUpHandler onMouseLeftButtonUpE;

		public delegate void MouseMouseLeftButtonHandler(Vector3 pos);
		public static event MouseMouseLeftButtonHandler onMouseLeftButtonE;

		// Mouse Right Button
		public delegate void MouseMouseRightButtonDownHandler(Vector3 pos);
		public static event MouseMouseRightButtonDownHandler onMouseRightButtonDownE;

		public delegate void MouseMouseRightButtonUpHandler(Vector3 pos);
		public static event MouseMouseRightButtonUpHandler onMouseRightButtonUpE;

		public delegate void MouseMouseRightButtonHandler(Vector3 pos);
		public static event MouseMouseRightButtonHandler onMouseRightButtonE;

		public delegate void MouseMouseMiddleButtonDownHandler(Vector3 pos);
		public static event MouseMouseMiddleButtonDownHandler onMouseMiddleButtonDownE;

		public delegate void MouseMouseMiddleButtonUpHandler(Vector3 pos);
		public static event MouseMouseMiddleButtonUpHandler onMouseMiddleButtonUpE;

		public delegate void MouseMouseMiddleButtonHandler(Vector3 pos);
		public static event MouseMouseMiddleButtonHandler onMouseMiddleButtonE;

		public static void OnMouseLeftButtonDown(Vector3 pos)
		{
			if (onMouseLeftButtonDownE != null)
				onMouseLeftButtonDownE(pos);
		}
		public static void OnMouseLeftButtonUp(Vector3 pos)
		{
			if (onMouseLeftButtonUpE != null)
				onMouseLeftButtonUpE(pos);
		}
		public static void OnMouseLeftButton(Vector3 pos)
		{
			if (onMouseLeftButtonE != null)
				onMouseLeftButtonE(pos);
		}

		public static void OnMouseRightButtonDown(Vector3 pos)
		{
			if (onMouseRightButtonDownE != null) 
				onMouseRightButtonDownE(pos);
		}
		public static void OnMouseRightButtonUp(Vector3 pos)
		{
			if (onMouseRightButtonUpE != null)
				onMouseRightButtonUpE(pos);
		}
		public static void OnMouseRightButton(Vector3 pos)
		{
			if (onMouseRightButtonE != null) 
				onMouseRightButtonE(pos);
		}

		public static void OnMouseMiddleButtonDown(Vector3 pos)
		{
			if (onMouseMiddleButtonDownE != null)
				onMouseMiddleButtonDownE(pos);
		}
		public static void OnMouseMiddleButtonUp(Vector3 pos)
		{
			if (onMouseMiddleButtonUpE != null)
				onMouseMiddleButtonUpE(pos);
		}
		public static void OnMouseMiddleButton(Vector3 pos)
		{
			if (onMouseMiddleButtonE != null)
				onMouseMiddleButtonE(pos);
		}

		protected override void Awake()
		{
			if (Instance == null)
			{
				Debug.LogFormat(LOG_FORMAT, "Awake()");
                Instance = this;

				Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>DPI : " + GetCurrentDPI() + "</color></b>");
			}
			else
			{
				Debug.LogErrorFormat(LOG_FORMAT, "");
				Destroy(this);
				return;
			}
		}

		protected virtual void OnDestroy()
		{
			if (Instance != this)
			{
				return;
			}

            Instance = null;
		}

		protected virtual void OnEnable()
		{
			//
		}
	}
}
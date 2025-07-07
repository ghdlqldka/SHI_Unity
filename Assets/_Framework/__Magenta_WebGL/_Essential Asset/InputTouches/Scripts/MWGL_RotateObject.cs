using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace _Magenta_WebGL
{

	[RequireComponent(typeof(Collider))]
	public class MWGL_RotateObject : _Magenta_Framework.RotateObjectEx
    {
		private static string LOG_FORMAT = "<color=magenta><b>[MWGL_RotateObject]</b></color> {0}";

		protected override void Awake()
		{
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);
		}

		protected override void OnEnable()
		{
			MWGL_IT_Gesture.onDraggingStartE += OnDraggingStart;
            MWGL_IT_Gesture.onDraggingE += OnDragging;

            MWGL_IT_Gesture.onPinchE += OnPinch;

			orbitSpeedX = 0;
			orbitSpeedY = 0;
			zoomSpeed = 0;
		}

		protected override void OnDisable()
		{
            MWGL_IT_Gesture.onDraggingStartE -= OnDraggingStart;
            MWGL_IT_Gesture.onDraggingE -= OnDragging;

            MWGL_IT_Gesture.onPinchE -= OnPinch;
		}
	}
}
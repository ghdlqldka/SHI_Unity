﻿using UnityEngine;

namespace RuntimeSceneGizmo
{
	public class CameraMovement : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField]
        protected float sensitivity = 0.5f;
#pragma warning restore 0649

		protected Vector3 prevMousePos;
		protected Transform mainCamParent;

		protected virtual void Awake()
		{
			mainCamParent = Camera.main.transform.parent;
		}

		protected virtual void Update()
		{
			if( Input.GetMouseButtonDown( 0 ) )
				prevMousePos = Input.mousePosition;
			else if( Input.GetMouseButton( 0 ) )
			{
				Vector3 mousePos = Input.mousePosition;
				Vector2 deltaPos = ( mousePos - prevMousePos ) * sensitivity;

				Vector3 rot = mainCamParent.localEulerAngles;
				while( rot.x > 180f )
					rot.x -= 360f;
				while( rot.x < -180f )
					rot.x += 360f;

				rot.x = Mathf.Clamp( rot.x - deltaPos.y, -89.8f, 89.8f );
				rot.y += deltaPos.x;
				rot.z = 0f;

				mainCamParent.localEulerAngles = rot;
				prevMousePos = mousePos;
			}
		}
	}
}
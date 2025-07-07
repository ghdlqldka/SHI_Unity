using UnityEngine;
using System.Collections;

namespace _Base_Framework._EndlessJumper
{

	[RequireComponent(typeof(Camera))]
	public class _PlayerCamera : EndlessJumper.PlayerCamera
	{
		protected Camera _cam;

		protected virtual void Awake()
		{
			_cam = this.GetComponent<Camera>();
		}

		// Update is called once per frame
		protected override void Update()
		{

			//Camera follow target

			if (target != null)
			{
				// Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
				// Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
				Vector3 point = _cam.WorldToViewportPoint(target.position);
				Vector3 delta = target.position - _cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
				Vector3 destination = transform.position + delta;
				destination.x = 0f;

				if (destination.y > this.transform.position.y)
				{
					transform.position = Vector3.SmoothDamp(
					this.transform.position, destination, ref velocity, dampTime);
				}
			}
		}


	}
}
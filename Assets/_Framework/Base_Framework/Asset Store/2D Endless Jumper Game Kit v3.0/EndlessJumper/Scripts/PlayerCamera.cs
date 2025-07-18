using UnityEngine;
using System.Collections;

namespace EndlessJumper
{

	public class PlayerCamera : MonoBehaviour
	{

		// Use this for initialization
		protected virtual void Start()
		{

		}

		protected float dampTime = 1f; //offset from the viewport center to fix damping
		protected Vector3 velocity = Vector3.zero;
		public Transform target;


		// Update is called once per frame
		protected virtual void Update()
		{

			//Camera follow target

			if (target)
			{
				Vector3 point = GetComponent<Camera>().WorldToViewportPoint(target.position);
				Vector3 delta = target.position - Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
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
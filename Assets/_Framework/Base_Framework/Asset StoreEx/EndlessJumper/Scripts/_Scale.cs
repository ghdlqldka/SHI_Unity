using UnityEngine;
using System.Collections;

namespace EndlessJumper
{

	public class _Scale : Scale
	{
		[SerializeField]
		protected Camera _cam;

		protected override void Resize()
		{

			//scale this object to screen
			SpriteRenderer sr = GetComponent<SpriteRenderer>();
			if (sr == null) 
				return;

			transform.localScale = new Vector3(1, 1, 1);

			float width = sr.sprite.bounds.size.x;
			// float height = sr.sprite.bounds.size.y;


			// float worldScreenHeight = Camera.main.orthographicSize * 2f;
			float worldScreenHeight = _cam.orthographicSize * 2f;
			float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

			Vector3 xWidth = transform.localScale;
			xWidth.x = worldScreenWidth / width;
			transform.localScale = xWidth;


		}
	}
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/*
CanvasScreenShot by programmer.
http://stackoverflow.com/questions/36555521/unity3d-build-png-from-panel-of-a-unity-ui#36555521
http://stackoverflow.com/users/3785314/programmer

https://gist.github.com/Shubhra22/bab1052cd90b9f4b89b3
*/

namespace _Base_Framework
{
	public class _UI_CaptureManager : MonoBehaviour
	{
		private static string LOG_FORMAT = "<color=#A8FF60><b>[_UI_CaptureManager]</b></color> {0}";

		protected static _UI_CaptureManager _instance;
		public static _UI_CaptureManager Instance
		{
			get
			{
				return _instance;
			}
			protected set
			{
				_instance = value;
			}
		}

		public delegate void CaptureResultCallback(Texture2D screenshot);

		protected virtual void Awake()
		{
			if (Instance == null)
			{
				Debug.LogFormat(LOG_FORMAT, "Awake()");

				Instance = this;
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

		// rectT - the UI element which you wanna capture
		public virtual IEnumerator DoUICapture(RectTransform rectT, CaptureResultCallback result = null)
		{
			Debug.LogWarningFormat(LOG_FORMAT, "DoUICapture()");

			int width = System.Convert.ToInt32(rectT.rect.width);
			int height = System.Convert.ToInt32(rectT.rect.height);

			yield return new WaitForEndOfFrame(); // it must be a coroutine 

			Vector2 position = rectT.transform.position;
			float startX = position.x - width / 2;
			float startY = position.y - height / 2;

			Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
			tex.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
			tex.Apply();

			// Encode texture into PNG
			/*
			var bytes = tex.EncodeToPNG();
			Destroy(tex);

			File.WriteAllBytes(Application.dataPath + "ScreenShot.png", bytes);

			string imgsrc = System.Convert.ToBase64String(bytes);
			Texture2D scrnShot = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			scrnShot.LoadImage(System.Convert.FromBase64String(imgsrc));

			Sprite sprite = Sprite.Create(scrnShot, new Rect(0, 0, scrnShot.width, scrnShot.height), new Vector2(.5f, .5f));
			img.sprite = sprite;
			*/

#if false
			yield return null; // timing

			Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Capture~~~~");
			Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
			yield return new WaitForEndOfFrame();
			screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
			screenshot.Apply();

			/*
			if (OnScreenCapturing != null) // Recover UI
			{
				Debug.LogFormat(LOG_FORMAT, "Do<b>ScreenCapture</b>() - Step 2 => Recover UI");
				OnScreenCapturing(false);
			}
			*/
#endif

			if (result != null)
			{
				result(tex);
			}
		}
	}
}
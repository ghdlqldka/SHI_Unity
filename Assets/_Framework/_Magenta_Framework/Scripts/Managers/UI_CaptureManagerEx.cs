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

namespace _Magenta_Framework
{
	public class UI_CaptureManagerEx : _Base_Framework._UI_CaptureManager
    {
		private static string LOG_FORMAT = "<color=#A8FF60><b>[UI_CaptureManagerEx]</b></color> {0}";

		public static new UI_CaptureManagerEx Instance
		{
			get
			{
				return _instance as UI_CaptureManagerEx;
			}
			protected set
			{
				_instance = value;
			}
		}

		protected override void Awake()
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

		protected override void OnDestroy()
		{
			if (Instance != this)
			{
				return;
			}

			Instance = null;
		}

	}
}
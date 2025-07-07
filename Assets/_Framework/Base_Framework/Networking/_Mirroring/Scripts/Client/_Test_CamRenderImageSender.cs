using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace _Base_Framework
{
	public class _Test_CamRenderImageSender : MonoBehaviour
	{
		[SerializeField]
		protected _MirroringSender sender;

		[SerializeField]
		protected Camera renderCam;
		protected Texture2D sendTexture;
		protected RenderTexture camTexture;

		protected virtual void Awake()
		{
			Debug.Assert(sender != null);
			Debug.Assert(renderCam != null);
		}

		// Use this for initialization
		protected virtual void Start()
		{
			camTexture = renderCam.targetTexture;

			sendTexture = new Texture2D(camTexture.width, camTexture.height);

			// Set send texture
			sender.SetSourceTexture(sendTexture);

			// sender.InitAndStart();
		}

		// Update is called once per frame
		protected virtual void Update()
		{
			RenderTexture.active = camTexture;
			sendTexture.ReadPixels(new Rect(0, 0, camTexture.width, camTexture.height), 0, 0, false);
		}

		protected virtual void OnGUI()
		{
            // DeviceFunc.GetIPAddress()
#if DEBUG
			GUIStyle myStyle = new GUIStyle();
			myStyle.fontSize = 80;

			int width = Screen.width / 4;
			int height = Screen.height / 4;
			GUILayout.BeginArea(new Rect(20 + width, 20 , 1366, 768));
			GUILayout.Label("<b><color=red>_Test_CamRenderImageSender</color></b>", myStyle);
			myStyle.fontSize = 45;
			myStyle.normal.textColor = Color.white;
			GUILayout.Label("", myStyle);
			GUILayout.Label("ip : " + Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString(), myStyle);

			GUILayout.EndArea();

			if (sender.IsConnected == false)
			{
				if (GUI.Button(new Rect(10 + width, 10 + height, 400, 200), "Mirroring Sender\nInitAndStart"))
				{
					sender.InitAndStart();
				}
			}
			else
			{
				if (GUI.Button(new Rect(10 + width, 10 + height, 400, 200), "Stop"))
				{
					sender.Stop();
				}
			}

#endif
		}
    }
}
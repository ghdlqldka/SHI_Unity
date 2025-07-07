using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
	public class _Test_ExampleReceiver : MonoBehaviour 
	{
		[SerializeField]
		protected _MirroringReceiver receiver;
		[SerializeField]
		protected RawImage rawImage;

		[Space(10)]
		[SerializeField]
		protected string ip;


		// Use this for initialization
		protected virtual void Start () 
		{
			Debug.Assert(receiver != null);

			rawImage.texture = receiver.texture;
		}

        protected virtual void OnGUI()
        {
#if DEBUG
			// GUIStyle myStyle = new GUIStyle();
			// myStyle.fontSize = 30;
			// myStyle.normal.textColor = Color.white;

			if (receiver.IsConnected() == false)
			{
				if (GUI.Button(new Rect(10, 10, 400, 200), "Mirroring Receiver\nInitAndStart"))
				{
					if (string.IsNullOrEmpty(ip) == true)
					{
						Debug.LogWarning("<color=red>Must be set CLIENT IP!!!!!</color>");
						return;
					}
					receiver.InitAndStart(ip, _MirroringReceiver.port);
				}
			}
			else
			{
				if (GUI.Button(new Rect(10, 10, 400, 200), "Stop"))
				{
					receiver.Stop();
				}
			}
#endif
		}
	}
}
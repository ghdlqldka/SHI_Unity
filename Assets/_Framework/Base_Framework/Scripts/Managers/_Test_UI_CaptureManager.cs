using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Base_Framework
{
    public class _Test_UI_CaptureManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[_Test_UI_CaptureManager]</b></color> {0}";

        public RectTransform rectT; // Assign the UI element which you wanna capture
        public Image img;

        // Start is called before the first frame update
        void Start()
        {
            //
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                // StartCoroutine(takeScreenShot ()); // screenshot of a particular UI Element.
                StartCoroutine(_UI_CaptureManager.Instance.DoUICapture(rectT, DoneCapture));
            }
        }

        protected virtual void DoneCapture(Texture2D capturedTexture)
        {
            Debug.LogFormat(LOG_FORMAT, "<color=red><b>Done</b></color>Capture() - Step 3");

			// Encode texture into PNG
			var bytes = capturedTexture.EncodeToPNG();
			Destroy(capturedTexture);

			// File.WriteAllBytes(Application.dataPath + "ScreenShot.png", bytes);

			string imgsrc = System.Convert.ToBase64String(bytes);
			Texture2D scrnShot = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			scrnShot.LoadImage(System.Convert.FromBase64String(imgsrc));

            Sprite sprite = Sprite.Create(scrnShot, new Rect(0, 0, scrnShot.width, scrnShot.height), new Vector2(.5f, .5f));
			img.sprite = sprite;
			
        }
    }
}
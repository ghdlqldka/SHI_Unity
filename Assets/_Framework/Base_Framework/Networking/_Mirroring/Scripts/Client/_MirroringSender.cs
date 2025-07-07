using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine;

namespace _Base_Framework
{

    /**
     * 클라이언트에서 미러링을 위해 쓰이는 클래스(이미지를 전달한다.)
     */
    public class _MirroringSender : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#D7A27F><b>[_MirroringSender]</b></color> {0}";

        public enum Encoding
        {
            JPG = 0,
            PNG = 1
        }

        [ReadOnly]
        [SerializeField]
        protected Texture2D sendTexture;

        protected TcpListener listner;
        protected TcpClient _client;
        protected TcpClient Client
        {
            get
            {
                return _client;
            }
            set
            {
                if (_client != null)
                {
                    _client.Close();
                }
                _client = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool isConnected = false;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool play = false;

        protected const int port = 8001;
        public Encoding encoding = Encoding.JPG;

        [Header("Must be the same in sender and receiver")]
        public int messageByteLength = 24;

        protected NetworkStream stream = null;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            Application.runInBackground = true;
        }

        protected virtual void Update()
        {
            //
        }

        protected virtual void OnGUI()
        {
#if DEBUG
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontSize = 80;

            // int width = Screen.width / 4;
            GUILayout.BeginArea(new Rect(20, 20, 1366, 768));
            GUILayout.Label("<b><color=red>_MirroringSender</color></b>", myStyle);
            myStyle.fontSize = 45;
            myStyle.normal.textColor = Color.white;
            GUILayout.Label("", myStyle);
            if (listner != null)
            {
                myStyle.normal.textColor = Color.yellow;
                GUILayout.Label("listner is SET!!!!!", myStyle);
            }
            else
            {
                GUILayout.Label("listner is NULL", myStyle);
            }
            myStyle.normal.textColor = Color.white;
            GUILayout.Label("isConnected(TcpClient) : " + isConnected, myStyle);
            GUILayout.Label("", myStyle);

            if (Client != null)
            {
                myStyle.normal.textColor = Color.yellow;
                GUILayout.Label("Client is SET!!!!!", myStyle);
                myStyle.normal.textColor = Color.red;
                GUILayout.Label("Client is Connected : " + Client.Connected, myStyle);
            }
            else
            {
                GUILayout.Label("Client is NULL", myStyle);
            }
            myStyle.normal.textColor = Color.white;
            GUILayout.Label("", myStyle);

            GUILayout.Label("play : " + play, myStyle);

            GUILayout.EndArea();
#endif
        }

        // stop everything
        protected virtual void OnApplicationQuit()
        {
            if (listner != null)
            {
                listner.Stop();
                listner = null;
            }

            Client = null;
            isConnected = false;
        }

        public void SetSourceTexture(Texture2D t)
        {
            sendTexture = t;
        }

        public void Stop()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>Stop</color></b>()");

            play = false;

            Client = null;
            isConnected = false;

            listner.Stop();
            listner = null;

            StopAllCoroutines();
        }

        public void InitAndStart()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>InitAndStart</color></b>()");

            play = true;

            Debug.Assert(sendTexture != null);

            if (listner == null)
            {
                // Connect to the server
                listner = new TcpListener(IPAddress.Any, port);
                listner.Start();

                //Start sending coroutine
                StartCoroutine(PostInitAndStart());
            }
            else
            {
                StopAllCoroutines();
                StartCoroutine(PostInitAndStart());
            }

            // Wait for client to connect in another Thread 
            _Loom.RunAsync(() => {
                while (play == true)
                {
                    // Wait for client connection
                    Client = listner.AcceptTcpClient();

                    isConnected = true;
                    stream = Client.GetStream();
                }
            });

            _Loom.RunAsync(() => {
                Thread.Sleep(100);
                while (play == true)
                {
                    if (Client != null)
                    {
                        if (Client.Connected == false)
                        {
                            Stop();
                        }
                    }
                }
            });
        }

        protected IEnumerator PostInitAndStart()
        {
            // Wait until client has connected
            while (isConnected == false)
            {
                yield return null;
            }

            bool readyToGetFrame = true;

            byte[] frameBytesLength = new byte[messageByteLength];

            while (play == true)
            {
                //Wait for End of frame
                yield return new WaitForEndOfFrame();
                byte[] imageBytes = EncodeImage();

                //Fill total byte length to send. Result is stored in frameBytesLength
                byteLengthToFrameByteArray(imageBytes.Length, frameBytesLength);

                //Set readyToGetFrame false
                readyToGetFrame = false;

                _Loom.RunAsync(() => {
                    //Send total byte count first
                    stream.Write(frameBytesLength, 0, frameBytesLength.Length);

                    //Send the image bytes
                    stream.Write(imageBytes, 0, imageBytes.Length);
                    //Sent. Set readyToGetFrame true
                    readyToGetFrame = true;
                });

                //Wait until we are ready to get new frame(Until we are done sending data)
                while (readyToGetFrame == false)
                {
                    yield return null;
                }
            }
        }

        //Converts the data size to byte array and put result to the fullBytes array
        protected void byteLengthToFrameByteArray(int byteLength, byte[] fullBytes)
        {
            //Clear old data
            Array.Clear(fullBytes, 0, fullBytes.Length);
            //Convert int to bytes
            byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
            //Copy result to fullBytes
            bytesToSendCount.CopyTo(fullBytes, 0);
        }

        protected byte[] EncodeImage()
        {
            if (encoding == Encoding.PNG)
            {
                return sendTexture.EncodeToPNG();
            }

            return sendTexture.EncodeToJPG();
        }
    }

}
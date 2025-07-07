using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace _Base_Framework
{
    /**
     * 클라이언트에서 전달해주는 이미지를 받는 클래스
     */
    public class _MirroringReceiver : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#D7A27F><b>[_MirroringReceiver]</b></color> {0}";

        protected TcpClient client;

        [HideInInspector]
        public Texture2D texture;

        [ReadOnly]
        [SerializeField]
        protected bool play = false;

        [Header("Must be the same in sender and receiver")]
        public int messageByteLength = 24;  //사이즈를 고정함, 헤더를 따로가져않고 계속 텍스쳐 데이터를 받는다.

        public const int port = 8001;

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            texture = new Texture2D(640, 480);
        }

        // Use this for initialization
        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            Application.runInBackground = true;
        }

        protected virtual void OnApplicationQuit()
        {
            play = false;

            if (client != null)
            {
                client.Close();
            }

            client = null;
        }

#if DEBUG
        protected virtual void OnGUI()
        {
            GUIStyle myStyle = new GUIStyle();
            myStyle.fontSize = 80;

            int width = Screen.width / 4;
            GUILayout.BeginArea(new Rect(20 + width, 20, 1366, 768));
            GUILayout.Label("<b><color=red>_MirroringReceiver</color></b>", myStyle);
            myStyle.fontSize = 45;
            myStyle.normal.textColor = Color.white;
            GUILayout.Label("", myStyle);

            if (client != null)
            {
                myStyle.normal.textColor = Color.yellow;
                GUILayout.Label("Client is SET!!!!!", myStyle);
                myStyle.normal.textColor = Color.red;
                GUILayout.Label("Client is Connected : " + client.Connected, myStyle);
            }
            else
            {
                GUILayout.Label("Client is NULL", myStyle);
            }
            myStyle.normal.textColor = Color.white;
            GUILayout.Label("", myStyle);

            GUILayout.Label("play : " + play, myStyle);

            GUILayout.EndArea();
        }
#endif

        public bool IsConnected()
        {
            // Debug.Assert(client != null);

            if (client != null)
            {
                return client.Connected;
            }
            return false;
        }

        public virtual void Stop()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>Stop</color></b>()");
            Debug.Assert(play == true);
            Debug.Assert(client != null);

            play = false;

            // if (client != null)
            {
                client.Close();
            }

            client = null;
        }

        /// <summary>
        /// 운영툴이 플레이어 PC에 접속한다. (미러링을 실행하면 플레이어 PC에서 서버소켓을 열기때문에
        /// 결과적으로 운영툴과 플레이어 PC 각각 서버를 갖게 된다.헷갈리지 않도록 주의)
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public virtual void InitAndStart(string ip, int port)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>InitAndStart</color></b>(), ip : " + ip + ", port : " + port);

            play = true;

            client = new TcpClient();

            try
            {
                client.Connect(IPAddress.Parse(ip), port);
            }
            catch (SocketException se)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "" + se.ToString());
            }

            //While loop in another Thread is fine so we don't block main Unity Thread
            _Loom.RunAsync(() => {
                while (play == true)
                {
                    //Read Image Count
                    int imageSize = ReadImageByteSize(messageByteLength);

                    //Read Image Bytes and Display it
                    ReadFrameByteArray(imageSize);
                }
            });
        }

        //Converts the byte array to the data size and returns the result
        protected int FrameByteArrayToByteLength(byte[] frameBytesLength)
        {
            int byteLength = BitConverter.ToInt32(frameBytesLength, 0);
            return byteLength;
        }

        protected int ReadImageByteSize(int size)
        {
            bool disconnected = false;

            NetworkStream serverStream = client.GetStream();
            byte[] imageBytesCount = new byte[size];
            int total = 0;
            do
            {
                var read = serverStream.Read(imageBytesCount, total, size - total);
                if (read == 0)
                {
                    disconnected = true;
                    break;
                }
                total += read;
            } while (total != size);

            int byteLength;

            if (disconnected)
            {
                byteLength = -1;
            }
            else
            {
                byteLength = FrameByteArrayToByteLength(imageBytesCount);
            }

            return byteLength;
        }

        protected void ReadFrameByteArray(int size)
        {
            bool disconnected = false;

            NetworkStream serverStream = client.GetStream();
            byte[] imageBytes = new byte[size];
            int total = 0;
            do
            {
                var read = serverStream.Read(imageBytes, total, size - total);
                if (read == 0)
                {
                    disconnected = true;
                    break;
                }
                total += read;
            }
            while (total != size);

            bool readyToReadAgain = false;

            //Display Image
            if (disconnected == false)
            {
                //Display Image on the main Thread
                _Loom.QueueOnMainThread(() => {
                    LoadReceivedImage(imageBytes);
                    readyToReadAgain = true;
                });
            }

            //Wait until old Image is displayed
            while (readyToReadAgain == false)
            {
                System.Threading.Thread.Sleep(1);
            }
        }

        protected void LoadReceivedImage(byte[] receivedImageBytes)
        {
            if (texture != null)
            {
                texture.LoadImage(receivedImageBytes);
            }
        }

        public void SetTargetTexture(Texture2D t)
        {
            texture = t;
        }
    }
}
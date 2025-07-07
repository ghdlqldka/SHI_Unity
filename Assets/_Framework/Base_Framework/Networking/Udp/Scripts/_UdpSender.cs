 
/*
    -----------------------
    UDP-Send
    -----------------------
   
    // 127.0.0.1 : 5009
    // nc -lu 127.0.0.1 5009
*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace _Base_Framework
{
    public class _UdpSender : _GitHub.UDPSend
    {
        private static string LOG_FORMAT = "<color=#40FF5E><b>[_UdpSender]</b></color> {0}";

        [SerializeField]
        protected string ip;
        public string IP
        {
            get
            {
                return ip;
            }
            set
            {
                ip = value;
            }
        }
        // protected UdpClient client;
        protected UdpClient udpClient
        {
            get
            {
                return client;
            }
            set
            {
                client = value;
            }
        }

        protected virtual void OnDestroy()
        {
            Debug.LogFormat(LOG_FORMAT, "OnDestroy()");

            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");
            // base.Start();

            // init();
            Init();
        }

        protected override void OnGUI()
        {
#if DEBUG
            Rect rectObj = new Rect(40, 380, 200, 400);
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 25;
            GUI.Box(rectObj, "# UDPSend-Data\n127.0.0.1 " + port + " #\n"
                        + "shell> nc -lu 127.0.0.1  " + port + " \n", style);
            strMessage = GUI.TextField(new Rect(40, 520, 200, 40), strMessage);
            if (GUI.Button(new Rect(250, 520, 40, 40), "send"))
            {
                SendData(strMessage + "\n");
            }
#endif
        }

        public override void init()
        {
            base.init();

            throw new System.NotSupportedException("");
        }

        public virtual void Init()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Init()");

            IP = "127.0.0.1";
            port = 5009;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
            udpClient = new UdpClient();
            // print("Sending to " + IP + " : " + port);
            // print("Testing: nc -lu " + IP + " : " + port);
        }

        protected override void sendString(string message)
        {
            base.sendString(message);

            throw new System.NotSupportedException("Instead use \"SendData()\"");
        }

        public virtual void SendData(string _data)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "SendData(), _data : <b><color=yellow>" + _data + "</color></b>");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(_data);
                udpClient.Send(data, data.Length, remoteEndPoint);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat(LOG_FORMAT, ex.Message);
            }
        }
    }
}
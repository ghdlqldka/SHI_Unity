 
/*
    -----------------------
    UDP-Receive (send to)
    -----------------------
    // > receive
    // 127.0.0.1 : 5009
   
    // send
    // nc -u 127.0.0.1 5009
*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace _Base_Framework
{

    public class _UdpReceiver : _GitHub.UDPReceive
    {
        private static string LOG_FORMAT = "<color=#40FF5E><b>[_UdpReceiver]</b></color> {0}";

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
                udpClient.Dispose(); // Releases the managed and unmanaged resources used by the UdpClient

                udpClient = null;
            }

            if (receiveThread != null)
            {
                // receiveThread.Join();
                receiveThread.Abort();

                receiveThread = null;
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");
            // base.Start();

            // init();
            Init(5009);
        }

        protected override void OnGUI()
        {
#if DEBUG
            Rect rectObj = new Rect(40, 10, 200, 400);
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 25;
            GUI.Box(rectObj, "# UDPReceive\n127.0.0.1 " + port + " #\n"
                        + "shell> nc -u 127.0.0.1 : " + port + " \n"
                        + "\nLast Packet: \n" + lastReceivedUDPPacket
                        + "\n\nAll Messages: \n" + allReceivedUDPPackets
                    , style);
#endif
        }

        protected override void init()
        {
            base.init();

            throw new System.NotSupportedException("");
        }

        public virtual void Init(int port)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Init()");
            this.port = port;

            // print("Sending to 127.0.0.1 : " + port);
            // print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        protected override void ReceiveData()
        {
            Debug.LogFormat(LOG_FORMAT, "ReceiveData(), port : " + port);

            Debug.Assert(udpClient == null);
            udpClient = new UdpClient(port);
            while (true)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = client.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);
                    Debug.LogWarningFormat(LOG_FORMAT, "Server: " + text);
                    lastReceivedUDPPacket = text;
                    allReceivedUDPPackets = allReceivedUDPPackets + text;
                }
                catch (Exception ex)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "<color=red>" + ex.Message + "</color>");
                }
            }
        }
    }
}
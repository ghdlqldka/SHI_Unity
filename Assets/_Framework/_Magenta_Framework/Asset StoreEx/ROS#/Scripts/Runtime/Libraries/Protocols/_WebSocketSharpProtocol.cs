/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Diagnostics;
using WebSocketSharp;

using Debug = UnityEngine.Debug;

namespace RosSharp.RosBridgeClient.Protocols
{
    public class _WebSocketSharpProtocol : WebSocketSharpProtocol
    {
        private static string LOG_FORMAT = "<color=#FF5C00><b>[_WebSocketSharpProtocol]</b></color> {0}";

        // private WebSocket WebSocket;
        protected WebSocket _WebSocket
        {
            get
            {
                return WebSocket;
            }
            set
            {
                WebSocket = value;
            }
        }

        public _WebSocketSharpProtocol(string url) : base()
        {
            Debug.LogFormat(LOG_FORMAT, "_WebSocketSharpProtocol constructor!!!!!!!!!!!!!!, url : " + url);

            /*
            WebSocket = new WebSocket(url);
            WebSocket.OnMessage += Receive;

            WebSocket.OnClose += Closed;
            WebSocket.OnOpen += Connected;
            */

            _WebSocket = new WebSocket(url);
            _WebSocket.OnMessage += OnReceiveEventHandler;

            _WebSocket.OnClose += OnClosedEventHandler;
            _WebSocket.OnOpen += OnConnectedEventHandler;
        }

        public override void Connect()
        {
            Debug.LogFormat(LOG_FORMAT, "<b><color=cyan>Connect()</color></b>");
            // base.Connect();

            _WebSocket.ConnectAsync();
        }

        public override void Close()
        {
            Debug.LogFormat(LOG_FORMAT, "Close()");
            // base.Close();

            _WebSocket.CloseAsync();
        }

        public override void Send(byte[] data)
        {
            _WebSocket.SendAsync(data, null);
        }

        protected void OnReceiveEventHandler(object sender, WebSocketSharp.MessageEventArgs e)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>OnReceive</color></b>EventHandler()");

            Receive(sender, e);
        }

        protected void OnClosedEventHandler(object sender, EventArgs e)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>OnClosed</color></b>EventHandler()");

            Closed(sender, e);
        }

        protected void OnConnectedEventHandler(object sender, EventArgs e)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>OnConnected</color></b>EventHandler()");

            Connected(sender, e);
        }
    }
}

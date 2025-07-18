﻿/*
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

using WebSocketSharp;

namespace RosSharp.RosBridgeClient.Protocols
{
    public class WebSocketSharpProtocol: IProtocol
    {
        public event EventHandler OnReceive;
        public event EventHandler OnConnected;
        public event EventHandler OnClosed;

        protected WebSocket WebSocket;

        public WebSocketSharpProtocol(/*string url*/)
        {
            /*
            WebSocket = new WebSocket(url);
            WebSocket.OnMessage += Receive;

            WebSocket.OnClose += Closed;
            WebSocket.OnOpen += Connected;
            */
        }

        public WebSocketSharpProtocol(string url)
        {
            WebSocket = new WebSocket(url);
            WebSocket.OnMessage += Receive;

            WebSocket.OnClose += Closed;
            WebSocket.OnOpen += Connected;
        }
                
        public virtual void Connect()
        {
            WebSocket.ConnectAsync();            
        }

        public virtual void Close()
        {
            WebSocket.CloseAsync();
        }

        public bool IsAlive()
        {
            return WebSocket.IsAlive;
        }

        public virtual void Send(byte[] data)
        {
            WebSocket.SendAsync(data, null);
        }

        protected void Receive(object sender, WebSocketSharp.MessageEventArgs e)
        {
            OnReceive?.Invoke(sender, new MessageEventArgs(e.RawData));
        }

        protected void Closed(object sender, EventArgs e)
        {
            OnClosed?.Invoke(sender, e);
        }

        protected void Connected(object sender, EventArgs e)
        {
            OnConnected?.Invoke(sender, e);
        }
    }
}

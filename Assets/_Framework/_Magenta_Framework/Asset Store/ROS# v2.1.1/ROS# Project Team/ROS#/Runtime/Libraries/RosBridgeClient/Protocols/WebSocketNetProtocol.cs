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

// this class (System.Net.WebSockets) requires .NET 4.6+ to compile and Windows 8+ to work

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RosSharp.RosBridgeClient.Protocols
{
    public class WebSocketNetProtocol : IProtocol
    {
        protected ClientWebSocket clientWebSocket;
        protected /*readonly*/ Uri uri;
        protected readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        protected /*readonly*/ CancellationToken cancellationToken;
        protected ManualResetEvent IsConnected = new ManualResetEvent(false);
        private AutoResetEvent IsReadyToSend = new AutoResetEvent(true);

        protected const int ReceiveChunkSize = 1024;
        protected const int SendChunkSize = 1024;

        protected ChannelReader<ArraySegment<byte>> reader;
        protected ChannelWriter<ArraySegment<byte>> writer;

        protected Task listener;
        protected Task sender;

        public event EventHandler OnReceive;
        public event EventHandler OnConnected;
        public event EventHandler OnClosed;

        public WebSocketNetProtocol(/*string uriString, int queueSize = 1000*/)
        {
            /*
            Channel<ArraySegment<byte>> channel = Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false,
            });

            reader = channel.Reader;
            writer = channel.Writer;

            clientWebSocket = new ClientWebSocket();
            uri = new Uri(uriString);
            cancellationToken = cancellationTokenSource.Token;
            */
        }

        public WebSocketNetProtocol(string uriString, int queueSize = 1000)
        {
            Channel<ArraySegment<byte>> channel = Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false,
            });

            reader = channel.Reader;
            writer = channel.Writer;

            clientWebSocket = new ClientWebSocket();
            uri = new Uri(uriString);
            cancellationToken = cancellationTokenSource.Token;
        }

        public virtual void Connect()
        {
            Task.Run(() => ConnectAsync());
        }

        protected void Invoke_OnConnected(object sender, EventArgs e)
        {
            OnConnected?.Invoke(null, EventArgs.Empty);
        }

        public virtual async void ConnectAsync()
        {
            await clientWebSocket.ConnectAsync(uri, cancellationToken);
            IsConnected.Set();
            OnConnected?.Invoke(null, EventArgs.Empty);
            listener = Task.Run(StartListen);
            sender = Task.Run(StartSend);
        }

        public void Close()
        {
            if (IsAlive())
            {
                writer.Complete();
            }
        }

        public bool IsAlive()
        {
            return clientWebSocket.State == WebSocketState.Open;
        }

        public void Send(byte[] message)
        {
            Send(new ArraySegment<byte>(message));
        }

        public void Send(ArraySegment<byte> msg)
        {
            if (!writer.TryWrite(msg))
            {
                throw new Exception();
            }
        }

        protected void Invoke_OnClosed(object sender, EventArgs e)
        {
            OnClosed?.Invoke(sender, e);
        }

        protected virtual async Task StartSend()
        {
            while (await reader.WaitToReadAsync())
            {
                if (reader.TryRead(out ArraySegment<byte> message))
                {
                    if (clientWebSocket.State != WebSocketState.Open)
                        throw new WebSocketException(WebSocketError.InvalidState, "Error Sending Message. WebSocket State is: " + clientWebSocket.State);

                    int messageCount = (int)Math.Ceiling((double)message.Count / SendChunkSize);

                    for (int i = 0; i < messageCount; i++)
                    {
                        int offset = SendChunkSize * i;
                        bool endOfMessage = (i == messageCount - 1);
                        int count = endOfMessage ? message.Count - offset : SendChunkSize;
                        await clientWebSocket.SendAsync(new ArraySegment<byte>(message.Array, offset, count), WebSocketMessageType.Binary, endOfMessage, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            // close the socket (listener will therminate after that)
            clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).Wait();
            IsConnected.Reset();
            OnClosed?.Invoke(null, EventArgs.Empty);
        }

        protected void Invoke_OnReceive(object sender, EventArgs e)
        {
            OnReceive?.Invoke(sender, e);
        }

        protected virtual async Task StartListen()
        {
            byte[] buffer = new byte[ReceiveChunkSize];

            while (clientWebSocket.State == WebSocketState.Open)
            {
                MemoryStream memoryStream = new MemoryStream();
                WebSocketReceiveResult result;
                do
                {
                    result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                        return;

                    memoryStream.Write(buffer, 0, result.Count);

                } while (!result.EndOfMessage);

                OnReceive?.Invoke(this, new MessageEventArgs(memoryStream.ToArray()));
            }
        }
    }
}


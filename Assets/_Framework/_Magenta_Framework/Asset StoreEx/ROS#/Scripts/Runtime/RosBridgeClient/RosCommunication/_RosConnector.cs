/*
© Siemens AG, 2017-2019
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
using System.Threading;
using RosSharp.RosBridgeClient.Protocols;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class _RosConnector : RosConnector
    {
        private static string LOG_FORMAT = "<color=#FB78F5><b>[_RosConnector]</b></color> {0}";

        // public RosSocket RosSocket { get; protected set; }
        public _RosSocket _RosSocket 
        { 
            get
            {
                return RosSocket as _RosSocket;
            }
            protected set
            {
                RosSocket = value;
#if DEBUG
                DEBUG_RosSocket = value;
#endif
            }
        }

#if DEBUG
        [Header("")]
        [ReadOnly]
        [SerializeField]
        protected _RosSocket DEBUG_RosSocket;
#endif

        public override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            IsConnected = new ManualResetEvent(false);
            new Thread(ConnectAndWait).Start();
        }

        protected override void ConnectAndWait()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "@@@@@ <b><color=yellow>ConnectAndWait()</color></b> @@@@@");
            // base.ConnectAndWait();

            // RosSocket = ConnectToRos(protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);
            _RosSocket = ConnectToRosEx(protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);

            if (IsConnected.WaitOne(SecondsTimeout * 1000) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Failed to connect to RosBridge at: <b>" + RosBridgeServerUrl + "</b>");
            }
        }

        public static _RosSocket ConnectToRosEx(Protocol protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.Microsoft)
        {
            // ConnectToRos(protocolType, serverUrl, onConnected, onClosed, serializer);

            IProtocol protocol = _ProtocolInitializer.GetProtocolEx(protocolType, serverUrl);
            protocol.OnConnected += onConnected;
            protocol.OnClosed += onClosed;

            // return new RosSocket(protocol, serializer);
            return new _RosSocket(protocol, serializer);
        }

        protected override void OnConnected(object sender, EventArgs e)
        {
            // Debug.LogFormat(LOG_FORMAT, "Connected to RosBridge: " + RosBridgeServerUrl);
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=cyan>OnConnected()</color></b>, RosBridgeServerUrl : <b>" + RosBridgeServerUrl + "</b>");
            // base.OnConnected(sender, e);

            IsConnected.Set();
        }

        protected override void OnClosed(object sender, EventArgs e)
        {
            // Debug.LogFormat(LOG_FORMAT, "Disconnected from RosBridge: " + RosBridgeServerUrl);
            Debug.LogWarningFormat(LOG_FORMAT, "OnClosed(), RosBridgeServerUrl : <b>" + RosBridgeServerUrl + "</b>");
            // base.OnClosed(sender, e);

            IsConnected.Reset();
        }
    }
}
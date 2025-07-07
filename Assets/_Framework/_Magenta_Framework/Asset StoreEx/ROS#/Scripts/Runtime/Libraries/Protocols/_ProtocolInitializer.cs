/*
© Siemens AG, 2019
Author: Sifan Ye (sifan.ye@siemens.com)

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

using System.Diagnostics;

namespace RosSharp.RosBridgeClient.Protocols
{
    public class _ProtocolInitializer : ProtocolInitializer
    {
        private static string LOG_FORMAT = "<color=#FFBB00><b>[_ProtocolInitializer]</b></color> {0}";

        public static IProtocol GetProtocolEx(Protocol protocol, string serverURL)
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "GetProtocolEx(), protocol : <b>" + protocol + "</b>, serverURL : <b>" + serverURL + "</b>");

            switch (protocol)
            {
                case Protocol.WebSocketSharp:
                    return new _WebSocketSharpProtocol(serverURL);
                case Protocol.WebSocketNET:
                    return new _WebSocketNetProtocol(serverURL);
                default:
                    Debug.Assert(false);
                    return null;
            }
        }
    }
}

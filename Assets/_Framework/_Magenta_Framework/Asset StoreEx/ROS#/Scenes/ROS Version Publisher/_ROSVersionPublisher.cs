/*
© Siemens AG, 2024
Author: Mehmet Emre Cakal (emre.cakal@siemens.com / m.emrecakal@gmail.com)

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

using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class _ROSVersionPublisher : ROSVersionPublisher
    {
        private static string LOG_FORMAT = "<color=#0082FF><b>[_ROSVersionPublisher]</b></color> {0}";

        // protected RosConnector rosConnector;
        protected _RosConnector _rosConnector
        {
            get
            {
                return rosConnector as _RosConnector;
            }
        }

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");
            // base.Start();

            // base.Start();
            rosConnector = GetComponent<_RosConnector>();
            Debug.Assert(rosConnector != null);
            publicationId = _rosConnector._RosSocket.Advertise<MessageTypes.Std.String>(Topic);

            InitializeMessage();
        }

        protected override void FixedUpdate()
        {
            // base.FixedUpdate();

            UpdateMessage();
        }

        protected override void InitializeMessage()
        {
            Debug.LogFormat(LOG_FORMAT, "InitializeMessage()");
            // base.InitializeMessage();

            message = new MessageTypes.Std.String();
        }

        public override void UpdateMessage()
        {
            // Debug.LogFormat(LOG_FORMAT, "UpdateMessage()");
            // base.UpdateMessage();

#if ROS2
            message.data = StringData + "2!";
#else
            message.data = StringData + "1!";
#endif
            
            Publish(message);
        }

        protected override void Publish(MessageTypes.Std.String message)
        {
            Debug.LogFormat(LOG_FORMAT, "Publishing \"<b>" + message.data + "</b>\" over topic \"/<b>" + this.Topic + "</b>\"");

            _rosConnector._RosSocket.Publish(publicationId, message);
        }
    }
}

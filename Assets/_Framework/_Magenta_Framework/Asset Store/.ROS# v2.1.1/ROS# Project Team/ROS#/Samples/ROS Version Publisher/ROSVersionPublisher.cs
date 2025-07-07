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
    public class ROSVersionPublisher : UnityPublisher<MessageTypes.Std.String>
    {
        protected string StringData = "Hello ROS";

        protected MessageTypes.Std.String message;


        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        protected virtual void FixedUpdate()
        {
            UpdateMessage();
            Debug.Log("Publishing \"" + message.data + "\" over topic \"/" + this.Topic + "\"");
        }

        protected virtual void InitializeMessage()
        {
            message = new MessageTypes.Std.String();
        }
        public virtual void UpdateMessage()
        {
            #if ROS2
            message.data = StringData + "2!";
            #else
            message.data = StringData + "1!";
            #endif
            Publish(message);
        }

    }
}

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

- Adding BSON (de-)seriliazation option
    Shimadzu corp , 2019, Akira NODA (a-noda@shimadzu.co.jp / you.akira.noda@gmail.com)

- Added ROS2 action support:
    - Added ActionProvider and ActionConsumer dictionaries.
    - Added AdvertiseAction<TActionGoal, TActionFeedback, TActionResult> method.
    - Added RespondFeedback<TActionFeedback, TFeedback> method.
    - Added RespondResult<TActionResult, TResult> method.
    - Added UnadvertiseAction method.
    - Added CancelActionGoalRequest<TActionResult> method.
    - Added SendActionGoalRequest<TActionGoal, TGoal, TActionFeedback, TActionResult> method.
    - Added handling for send_action_goal message, cancel_action_goal message, action_feedback message, and action_result message.

    © Siemens AG 2025, Mehmet Emre Cakal, emre.cakal@siemens.com/m.emrecakal@gmail.com
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using RosSharp.RosBridgeClient.Protocols;
using AYellowpaper.SerializedCollections;

using Debug = UnityEngine.Debug;

namespace RosSharp.RosBridgeClient
{
    [System.Serializable]
    public class _RosSocket : RosSocket
    {
        private static string LOG_FORMAT = "<color=#F10045><b>[_RosSocket]</b></color> {0}";

        public _WebSocketSharpProtocol _sharpProtocol
        {
            get
            {
                return protocol as _WebSocketSharpProtocol;
            }
        }

        public _WebSocketNetProtocol _netProtocol
        {
            get
            {
                return protocol as _WebSocketNetProtocol;
            }
        }

        public _RosSocket(IProtocol protocol, SerializerEnum serializer = SerializerEnum.Microsoft) : base()
        {
            Debug.LogFormat(LOG_FORMAT, "_RosSocket constructor!!!!!!, protocol : <b>" + protocol + "</b>, serializer : <b>" + serializer + "</b>");

            /*
            this.protocol = protocol;

            if (serializerDictionary.TryGetValue(serializer, out Serializer))
            {
                SerializerType = serializer;
            }
            else
            {
                throw new ArgumentException("Invalid serializer type specified.");
            }

            this.protocol.OnReceive += (sender, e) => Receive(sender, e);
            this.protocol.Connect();
            */

            if (protocol is _WebSocketSharpProtocol)
            {
                this.protocol = protocol;

                if (serializerDictionary.TryGetValue(serializer, out Serializer))
                {
                    SerializerType = serializer;
                }
                else
                {
                    throw new ArgumentException("Invalid serializer type specified.");
                }

                this._sharpProtocol.OnReceive += (sender, e) => Receive(sender, e);
                this._sharpProtocol.Connect();
            }
            else if (protocol is _WebSocketNetProtocol)
            {
                this.protocol = protocol;

                if (serializerDictionary.TryGetValue(serializer, out Serializer))
                {
                    SerializerType = serializer;
                }
                else
                {
                    throw new ArgumentException("Invalid serializer type specified.");
                }

                this._netProtocol.OnReceive += (sender, e) => Receive(sender, e);
                this._netProtocol.Connect();
            }
            else
            {
                Debug.Assert(false);
            }

            
        }

        protected override void Receive(object sender, EventArgs e)
        {
            // base.Receive(sender, e);

            byte[] buffer = ((MessageEventArgs)e).RawData;
            DeserializedObject jsonElement = Serializer.Deserialize(buffer);

            Debug.LogFormat(LOG_FORMAT, "Receive(), jsonElement : <color=magenta>" + jsonElement.GetAll() + "</color>");

            string opProperty = jsonElement.GetProperty("op");
            switch (opProperty)
            {
                case "publish":
                    {
                        string topic = jsonElement.GetProperty("topic");
                        string msg = jsonElement.GetProperty("msg");
                        foreach (Subscriber subscriber in SubscribersOf(topic))
                            subscriber.Receive(msg, Serializer);
                        return;
                    }
                case "service_response":
                    {
                        string id = jsonElement.GetProperty("id");
                        string values = jsonElement.GetProperty("values");
                        ServiceConsumers[id].Consume(values, Serializer);
                        return;
                    }
                case "call_service":
                    {
                        string id = jsonElement.GetProperty("id");
                        string service = jsonElement.GetProperty("service");
                        string args = jsonElement.GetProperty("args");
                        Send(ServiceProviders[service].Respond(id, args, Serializer));
                        return;
                    }
#if ROS2
                // Provider side
                case "send_action_goal":
                    {
                        string action = jsonElement.GetProperty("action");

                        //Console.WriteLine("Complete incoming goal message: " + jsonElement.GetAll());
                        ActionProviders[action].ListenSendGoalAction(jsonElement.GetAll(), Serializer);
                        return;
                    }
                // Provider side
                case "cancel_action_goal":
                    {
                        string frameId = jsonElement.GetProperty("id");
                        string action = jsonElement.GetProperty("action");

                        //Console.WriteLine("Complete incoming cancel message: " + jsonElement.GetAll());
                        ActionProviders[action].ListenCancelGoalAction(frameId, action, Serializer);
                        return;
                    }
                // Consumer side
                case "action_feedback":
                    {
                        string id = jsonElement.GetProperty("id");

                        //Console.WriteLine("Complete server response for feedback: " + jsonElement.GetAll());
                        ActionConsumers[id].ConsumeFeedbackResponse(jsonElement.GetAll(), Serializer);
                        return;
                    }
                // Consumer side
                case "action_result":
                    {
                        string id = jsonElement.GetProperty("id");

                        //Console.WriteLine("Complete server response for result: " + jsonElement.GetAll());
                        ActionConsumers[id].ConsumeResultResponse(jsonElement.GetAll(), Serializer);
                        return;
                    }
#endif // ROS2
                default:
                    Debug.LogWarningFormat(LOG_FORMAT, "Unhandled opProperty : <b>" + opProperty + "</b>");
                    break;
            }
        }

        public override void Publish(string id, Message message)
        {
            Debug.LogFormat(LOG_FORMAT, "Publish()");

            Send(Publishers[id].Publish(message));
        }

#if ROS2
        public string AdvertiseAction_Fibonacci(
            string action,
            SendActionGoalHandler<MessageTypes.ActionTutorialsInterfaces.FibonacciActionGoal> sendActionGoalHandler,
            CancelActionGoalHandler cancelActionGoalHandler)
        {
            Debug.LogFormat(LOG_FORMAT, "AdvertiseAction_Fibonacci()");

            string id = action;
            if (ActionProviders.ContainsKey(id))
                UnadvertiseAction(id);

            ActionAdvertisement actionAdvertisement;
            ActionProviders.Add(id, new ActionProvider<MessageTypes.ActionTutorialsInterfaces.FibonacciActionGoal>(
                action,
                sendActionGoalHandler,
                cancelActionGoalHandler,
                out actionAdvertisement));

            Debug.LogFormat(LOG_FORMAT, "actionAdvertisement : " + actionAdvertisement);
            Send(actionAdvertisement);

            return id;
        }
#endif
    }
}

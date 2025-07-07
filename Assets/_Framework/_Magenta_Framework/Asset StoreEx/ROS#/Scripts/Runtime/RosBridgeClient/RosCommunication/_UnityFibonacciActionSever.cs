/*
© Siemens AG, 2019
Author: Berkay Alp Cakal (berkay_alp.cakal.ct@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

- Added ROS2 action support: ROS2 server does not need to publish status in the update loop.
- Added ReadOnlyAttribute and ReadOnlyDrawer for read-only fields in the Unity Editor: status and feedback should not be modified by the user.
    © Siemens AG, 2025, Mehmet Emre Cakal, emre.cakal@siemens.com/m.emrecakal@gmail.com
*/

using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
    [RequireComponent(typeof(_RosConnector))]
    public class _UnityFibonacciActionSever : UnityFibonacciActionSever
    {
        private static string LOG_FORMAT = "<b><color=#00E011>[_UnityFibonacciActionSever]</color></b> {0}";

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

            rosConnector = GetComponent<_RosConnector>();
            Log log = new Log(x => Debug.Log(x));
            fibonacciActionServer = new _FibonacciActionServer(actionName, _rosConnector._RosSocket, log);
            fibonacciActionServer.Initialize();
        }
    }
}
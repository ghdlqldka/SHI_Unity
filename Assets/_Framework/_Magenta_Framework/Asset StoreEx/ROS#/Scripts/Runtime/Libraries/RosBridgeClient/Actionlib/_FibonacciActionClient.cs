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

- Added ROS2 action support: 
    - ROS2 uses RosSharp.RosBridgeClient.MessageTypes.ActionTutorialsInterfaces and RosSharp.RosBridgeClient.MessageTypes.Action instead of ActionlibTutorials and Actionlib.
    - Added the SetActionGoal method in ROS2 to set additional parameters such as feedback, fragmentSize, and compression.
    - Replaced MessageTypes.Actionlib.GoalStatus with GoalStatus.
    - Adjusted data structures to handle ROS2-specific feedback and result fields:
        - Feedback uses action.action_feedback.values.partial_sequence.
        - Result uses action.action_result.values.sequence.
        - Metadata like frame ID is accessed through action.action_result.id.
    - Added Console.WriteLine statements in OnFeedbackReceived and OnResultReceived to log feedback, results, and metadata to the console.
    - In ROS2, action.action_goal.action and action.action_goal.args are used to set the goal, replacing direct manipulation of action.action_goal.goal.order in ROS1.
    - Removed ROS1-specific fields like fibonacciOrder and string-based status, feedback, and result tracking.
    - Updated string formatting for feedback and result:
        - ROS2 constructs strings using action.action_feedback.values.partial_sequence and action.action_result.values.sequence.
        - ROS1 constructs strings using action.action_feedback.feedback.sequence and action.action_result.result.sequence.

    © Siemens AG 2025, Mehmet Emre Cakal, emre.cakal@siemens.com/m.emrecakal@gmail.com
*/

using System;
using System.Diagnostics;
using static _Base_Framework._PlayerPrefsManager;



#if !ROS2
using RosSharp.RosBridgeClient.MessageTypes.ActionlibTutorials;

namespace RosSharp.RosBridgeClient.Actionlib
{
    public class _FibonacciActionClient : FibonacciActionClient
    {
        public _FibonacciActionClient(string actionName, RosSocket rosSocket) : base(actionName, rosSocket)
        {
            //
        }
    }
}

#else
using RosSharp.RosBridgeClient.MessageTypes.ActionTutorialsInterfaces;
using RosSharp.RosBridgeClient.MessageTypes.Action;

namespace RosSharp.RosBridgeClient.Actionlib
{
    public class _FibonacciActionClient : FibonacciActionClient
    {
        private static string LOG_FORMAT = "<color=#00FF62><b>[_FibonacciActionClient]</b></color> {0}";

        // public RosSocket rosSocket;
        public RosSocket _rosSocket
        {
            get
            {
                return rosSocket;
            }
        }

        public _FibonacciActionClient(string actionName, _RosSocket rosSocket) : base()
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "_FibonacciActionClient constructor!!!!!");

            /*
            this.actionName = actionName;
            this.rosSocket = rosSocket;

            action = new FibonacciAction();
            goalStatus = new GoalStatus();
            */

            this.actionName = actionName;
            this.rosSocket = rosSocket;

            action = new FibonacciAction();
            goalStatus = new GoalStatus();
        }

        public override void SendGoal()
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "SendGoal()");
            // base.SendGoal();

            _rosSocket.SendActionGoalRequest<FibonacciActionGoal, FibonacciGoal, FibonacciActionFeedback, FibonacciActionResult>(
                action.action_goal,
                ResultCallback,
                FeedbackCallback);

            lastResultSuccess = false;
        }

        public override void CancelGoal(string frameId = null)
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "CancelGoal()");
            // base.CancelGoal(frameId);

            _rosSocket.CancelActionGoalRequest<FibonacciActionResult>(
                frameId ?? this.frameId,
                actionName,
                ResultCallback);
        }

        protected override void ResultCallback(FibonacciActionResult actionResult)
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "ResultCallback()");
            // base.ResultCallback(actionResult);

            if (actionResult.result == false)
            {
                Console.WriteLine("Request failed!");
            }
            else
            {
                action.action_result = actionResult;
                goalStatus.status = actionResult.status;
                lastResultSuccess = actionResult.result;
                frameId = actionResult.id;
                OnResultReceived();
            }

        }

        protected override void FeedbackCallback(FibonacciActionFeedback actionFeedbackValues)
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "FeedbackCallback()");
            // base.FeedbackCallback(actionFeedbackValues);

            action.action_feedback = actionFeedbackValues;
            frameId = actionFeedbackValues.id;
            OnFeedbackReceived();
        }
    }
}
#endif
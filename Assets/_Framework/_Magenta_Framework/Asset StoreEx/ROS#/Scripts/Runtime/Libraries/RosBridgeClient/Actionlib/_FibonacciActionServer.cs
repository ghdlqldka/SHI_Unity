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
    - Namespace Updates: Replaced MessageTypes.Actionlib and MessageTypes.ActionlibTutorials with MessageTypes.ActionTutorialsInterfaces.
    - Goal Validation: Updated action.action_goal.goal.order to action.action_goal.args.order in the IsGoalValid method.
    - Feedback Handling: ROS2 uses action.action_feedback.values.partial_sequence instead of action.action_feedback.feedback.sequence.
    - Result Handling:
        - ROS2 uses action.action_result.values.sequence instead of action.action_result.result.sequence.
        - Added a new field action.action_result.result to indicate the success of the goal execution (true or false).
        - Updated result string generation in GetResultSequenceString.
    - Thread Execution:
        - Reduced Thread.Sleep in ExecuteFibonacciGoal from 1000ms (ROS1) to 500ms (ROS2).
        - Added a log message in ROS2 for feedback publication during goal execution.
    - Goal Reception:
        - ROS2 calls SetExecuting instead of SetAccepted in OnGoalReceived.
        - Added logging for accepted and rejected goals.
    - Goal Execution:
        - ROS2 implements OnGoalExecuting for starting goal execution, replacing OnGoalActive in ROS1.
        - Improved logging in OnGoalExecuting and during feedback/result handling.
    - Goal Cancellation:
        - Changed OnGoalPreempting (ROS1) to OnGoalCanceling (ROS2).
        - Enhanced logging for goal cancellation in ROS2.
    - Goal Status Updates:
        - Added ROS2 ActionStatus conventions.
        - Enhanced logging in OnGoalSucceeded, OnGoalAborted, and OnGoalCanceled.
    - General Logging:
         comprehensive log messages in various methods (e.g., ExecuteFibonacciGoal, OnGoalExecuting, OnGoalAborted) to improve debugging and monitoring.
    - Miscellaneous:
        - Removed redundant methods like OnGoalRecalling and simplified certain callbacks to align with ROS2's action handling.
        - Introduced consistent use of UpdateAndPublishStatus and PublishResult for publishing status and results.

    © Siemens AG 2025, Mehmet Emre Cakal, emre.cakal@siemens.com/m.emrecakal@gmail.com
*/

using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;




#if !ROS2
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;
using RosSharp.RosBridgeClient.MessageTypes.ActionlibTutorials;


namespace RosSharp.RosBridgeClient.Actionlib
{
    public class _FibonacciActionServer : FibonacciActionServer
    {
        public _FibonacciActionServer(string actionName, RosSocket rosSocket, Log log) : base(actionName, rosSocket, log)
        {
            //
        }

    }
}
#else

using RosSharp.RosBridgeClient.MessageTypes.ActionTutorialsInterfaces;


namespace RosSharp.RosBridgeClient.Actionlib
{
    public class _FibonacciActionServer : FibonacciActionServer
    {
        private static string LOG_FORMAT = "<color=#D7A27F><b>[_FibonacciActionServer]</b></color> {0}";

        // public RosSocket rosSocket;
        public _RosSocket _rosSocket
        {
            get
            {
                return rosSocket as _RosSocket;
            }
        }

        // protected ActionStatus actionStatus = ActionStatus.STATUS_NO_GOAL;
        protected ActionStatus _actionStatus
        {
            get
            {
                return actionStatus;
            }
            set
            {
                UnityEngine.Debug.LogWarningFormat(LOG_FORMAT, "<color=cyan>_actionStatus : <b>" + value + "</b></color>");

                actionStatus = value;
            }
        }

        public _FibonacciActionServer(string actionName, _RosSocket _rosSocket, Log log) : base()
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "_FibonacciActionServer constructor!!!!!");

            /*
            this.actionName = actionName;
            this.rosSocket = rosSocket;
            this.log = log;
            action = new FibonacciAction();
            */

            this.actionName = actionName;
            this.rosSocket = _rosSocket;
            this.log = log;
            action = new FibonacciAction();
        }

        public override void Initialize()
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "Initialize(), actionName : <b>" + actionName + "</b>");
            // base.Initialize();

            // base.Initialize();
            // string action, sendActionGoalHandler, cancelActionGoalHandler
            // ActionAdvertismentId = _rosSocket.AdvertiseAction<FibonacciActionGoal, FibonacciActionResult, FibonacciActionFeedback>(actionName, GoalCallback, CancelCallback);
            ActionAdvertismentId = _rosSocket.AdvertiseAction_Fibonacci(actionName, GoalCallback, CancelCallback);

            UpdateAndPublishStatus(ActionStatus.STATUS_NO_GOAL);
        }

        protected override void UpdateAndPublishStatus(ActionStatus actionStatus) // not necessary anymore?
        {
            // base.UpdateAndPublishStatus(actionStatus);

            this._actionStatus = actionStatus;
            //PublishStatus();
        }

        protected override void GoalCallback(FibonacciActionGoal actionGoal)
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "GoalCallback()");
            // base.GoalCallback(actionGoal);

            action.action_goal = actionGoal;
            UpdateAndPublishStatus(ActionStatus.STATUS_ACCEPTED); // PENDING
            OnGoalReceived();
        }

        protected override void CancelCallback(string frameId, string action)
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "CancelCallback()");
            // base.CancelCallback(frameId, action);

            switch (_actionStatus)
            {
                case ActionStatus.STATUS_ACCEPTED: // todo: what to do here?

                case ActionStatus.STATUS_EXECUTING:
                    UpdateAndPublishStatus(ActionStatus.STATUS_CANCELING);
                    OnGoalCanceling(); // OnGoalPreempting();
                    break;
                default:
                    log("Goal cannot be 'canceling' under current state: " + _actionStatus.ToString() + ". Ignored");
                    break;
            }
        }

        protected override void ExecuteFibonacciGoal()
        {
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "ExecuteFibonacciGoal()");
            // base.ExecuteFibonacciGoal();

            isProcessingGoal.Set();
            Thread.Sleep(500);
            List<int> sequence = new List<int> { 0, 1 };

            action.action_feedback.values.partial_sequence = sequence.ToArray();
            PublishFeedback();

            for (int i = 1; i < action.action_goal.args.order; i++)
            {
                if (!isProcessingGoal.WaitOne(0))
                {
                    action.action_result.values.sequence = sequence.ToArray();

                    if (this.GetStatus() != ActionStatus.STATUS_ABORTED)
                    {
                        SetCanceled();
                    }

                    return;
                }

                sequence.Add(sequence[i] + sequence[i - 1]);

                action.action_feedback.values.partial_sequence = sequence.ToArray();
                PublishFeedback();

                // log("Fibonacci Action Server: Publishing feedback: " + GetFeedbackSequenceString());
                UnityEngine.Debug.LogFormat(LOG_FORMAT, "Fibonacci Action Server: Publishing feedback: " + GetFeedbackSequenceString());
                Thread.Sleep(500);
            }

            action.action_result.values.sequence = sequence.ToArray();
            action.action_result.result = true;
            SetSucceeded();

            // log("Final result: " + GetResultSequenceString());
            UnityEngine.Debug.LogFormat(LOG_FORMAT, "Final result: " + GetResultSequenceString());
        }
    }
}
#endif
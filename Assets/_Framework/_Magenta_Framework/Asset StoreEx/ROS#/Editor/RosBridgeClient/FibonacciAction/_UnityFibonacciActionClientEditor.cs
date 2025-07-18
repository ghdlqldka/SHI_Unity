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
*/

using UnityEditor;
using UnityEngine;

namespace RosSharp.RosBridgeClient.Actionlib
{
	[CustomEditor(typeof(_UnityFibonacciActionClient))]
    public class _UnityFibonacciActionClientEditor : FibonacciActionClientEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Send Goal"))
            {
                ((_UnityFibonacciActionClient)target).RegisterGoal();
                ((_UnityFibonacciActionClient)target)._fibonacciActionClient.SendGoal();
            }

            if (GUILayout.Button("Cancel Goal"))
            {
                ((_UnityFibonacciActionClient)target)._fibonacciActionClient.CancelGoal();
            }
        }
    }
}
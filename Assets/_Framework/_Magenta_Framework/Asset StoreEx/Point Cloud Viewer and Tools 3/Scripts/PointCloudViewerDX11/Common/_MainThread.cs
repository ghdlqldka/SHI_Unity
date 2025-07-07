// source: http://forum.unity3d.com/threads/mainthread-function-caller.348198/
// https://github.com/PimDeWitte/UnityMainThreadDispatcher

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace pointcloudviewer.tools
{
    [DefaultExecutionOrder(-10)]
    public class _MainThread : MainThread
    {
        private static string LOG_FORMAT = "<color=#EA1362><b>[_MainThread]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(instanceCount == 0);
            instanceCount++;
            calls = new List<CallInfo>();
            functions = new List<Func>();

            StartCoroutine(Executer());
        }

        protected override void OnDestroy()
        {
            // base.OnDestroy();
            instanceCount--;
        }

        public static new void Call(Function Func, object Parameter)
        {
            lock (callsLock)
            {
                calls.Add(new CallInfo(Func, Parameter));
            }
        }

        public static new void Call(Func func)
        {
            lock (functionsLock)
            {
                functions.Add(func);
            }
        }

        protected override IEnumerator Executer()
        {
            while (true)
            {
                yield return null; // <===========================
                lock (callsLock)
                {
                    while (calls.Count > 0)
                    {
                        calls[0].Execute();
                        calls.RemoveAt(0);
                    }
                }

                lock (functionsLock)
                {
                    while (functions.Count > 0)
                    {
                        functions[0]();
                        functions.RemoveAt(0);
                    }
                }
            }
        }
    }
}

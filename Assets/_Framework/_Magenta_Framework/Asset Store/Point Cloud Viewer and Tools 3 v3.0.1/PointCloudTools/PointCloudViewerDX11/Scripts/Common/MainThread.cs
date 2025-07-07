// source: http://forum.unity3d.com/threads/mainthread-function-caller.348198/
// https://github.com/PimDeWitte/UnityMainThreadDispatcher

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace pointcloudviewer.tools
{
    [DefaultExecutionOrder(-10)]
    public class MainThread : MonoBehaviour
    {

        public class CallInfo
        {
            public Function func;
            public object parameter;
            public CallInfo(Function Func, object Parameter)
            {
                func = Func;
                parameter = Parameter;
            }
            public void Execute()
            {
                func(parameter);
            }
        }

        public delegate void Function(object parameter);
        public delegate void Func();

        protected static List<CallInfo> calls = new List<CallInfo>();
        protected static List<Func> functions = new List<Func>();

        protected static Object callsLock = new Object();
        protected static Object functionsLock = new Object();

        public static int instanceCount = 0;

        protected virtual void Awake()
        {
            instanceCount++;
            calls = new List<CallInfo>();
            functions = new List<Func>();

            StartCoroutine(Executer());
        }

        public static void Call(Function Func, object Parameter)
        {
            lock (callsLock)
            {
                calls.Add(new CallInfo(Func, Parameter));
            }
        }
        public static void Call(Func func)
        {
            lock (functionsLock)
            {
                functions.Add(func);
            }
        }

        protected virtual void OnDestroy()
        {
            instanceCount--;
        }

        //IEnumerator Executer()
        //{
        //    while (true)
        //    {
        //        yield return null;
        //        while (calls.Count > 0)
        //        {
        //            calls[0].Execute();
        //            lock (callsLock)
        //            {
        //                calls.RemoveAt(0);
        //            }
        //        }

        //        while (functions.Count > 0)
        //        {
        //            functions[0]();
        //            lock (functionsLock)
        //            {
        //                functions.RemoveAt(0);
        //            }
        //        }
        //    }
        //}

        protected virtual IEnumerator Executer()
        {
            while (true)
            {
                yield return null;
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

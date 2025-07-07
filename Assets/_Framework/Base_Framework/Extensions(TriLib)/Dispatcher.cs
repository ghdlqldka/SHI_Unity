using UnityEngine;
using System;
using System.Collections.Generic;
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
using System.Threading.Tasks;
#else
using System.Threading;
#endif

namespace _Base_Framework
{
    /// <summary>
    /// Represents a system for dispatching code to execute on the main thread.
    /// </summary>
    public class Dispatcher : MonoBehaviour
    {
        protected static Dispatcher _instance;
        protected static bool _instanceExists;

        private static readonly object LockObject = new object();
        private static readonly Queue<Action> _Actions = new Queue<Action>();

        /// <summary>
        /// Checks if there is any instance.
        /// </summary>
        public static void CheckInstance()
        {
            if (_instanceExists == false)
            {
                var gameObject = new GameObject("Dispatcher");
                gameObject.AddComponent<Dispatcher>();
                _instanceExists = true;
            }
        }

        /// <summary>
        /// Queues an action to be invoked on the main game thread.
        /// </summary>
        /// <param name="action">The action to be queued.</param>
        public static void InvokeAsync(Action action)
        {
            if (_instanceExists == false)
            {
                Debug.LogError("No Dispatcher exists in the scene. Actions will not be invoked!");
                return;
            }
            lock (LockObject)
            {
                _Actions.Enqueue(action);
            }
        }

        protected virtual void Awake()
        {
            if (_instance != null)
            {
                DestroyImmediate(this);
            }
            else
            {
                _instance = this;
                _instanceExists = true;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _instanceExists = false;
            }
        }

        protected virtual void Update()
        {
            lock (LockObject)
            {
                while (_Actions.Count > 0)
                {
                    _Actions.Dequeue()();
                }
            }
        }
    }
}

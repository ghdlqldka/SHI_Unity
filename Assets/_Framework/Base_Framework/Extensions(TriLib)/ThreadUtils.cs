﻿using System;
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
using System.Threading.Tasks;
#else
using System.Threading;
#endif

namespace _Base_Framework
{
    /// <summary>
    /// Contains Thread helper functions.
    /// </summary>
    public static class ThreadUtils
    {
        /// <summary>
        /// Creates a new Thread and runs it.
        /// </summary>
        /// <returns>The Thread.</returns>
        /// <param name="action">Action for the Thread to run.</param>
        /// <param name="onComplete">Action to run under completion.</param>
        ///
#if !UNITY_EDITOR && (NETFX_CORE || NET_4_6 || NET_STANDARD_2_0) && !ENABLE_IL2CPP && !ENABLE_MONO
        public static Task RunThread(Action action, Action onComplete) {
            Dispatcher.CheckInstance();
            var task = Task.Run(delegate
            {
                    try
                    {
                        action();
                        Dispatcher.InvokeAsync(onComplete);
                    }
                    catch (Exception exception)
                    {
                        Dispatcher.InvokeAsync(delegate
                        {
                            throw exception;
                        });
                    }
                });
            return task;
        }
#else
        public static Thread RunThread(Action action, Action onComplete)
        {
            Dispatcher.CheckInstance();

            Thread thread = new Thread(delegate ()
                {
                    try
                    {
                        action();
                        Dispatcher.InvokeAsync(onComplete);
                    }
                    catch (Exception exception)
                    {
                        Dispatcher.InvokeAsync(delegate
                        {
                            throw exception;
                        });
                    }
                });
            thread.Start();

            return thread;
        }
#endif
    }
}

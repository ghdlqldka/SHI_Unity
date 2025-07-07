using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
// using UnityTimer;

namespace _Magenta_Framework
{
    public class TimerManagerEx : _Base_Framework._TimerManager
    {
        private static string LOG_FORMAT = "<color=#9300FB><b>[TimerManagerEx]</b></color> {0}";

        public static new TimerManagerEx Instance
        {
            get
            {
                return _instance as TimerManagerEx;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                _timers = null; // Not used!!!!!
                _timersToAdd = null; // Not used!!!!!
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

    }
}

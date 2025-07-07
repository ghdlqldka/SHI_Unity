using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityTimer
{
    public class __TimerManager : Timer.TimerManager
    {
        private static string LOG_FORMAT = "<color=#9300FB><b>[__TimerManager]</b></color> {0}";

        protected static __TimerManager _instance;
        public static __TimerManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        [SerializedDictionary("GUID", "Timer")]
        protected SerializedDictionary<string, __Timer> _timerDic = new SerializedDictionary<string, __Timer>();

        // [ReadOnly]
        // [SerializeField]
        protected List<__Timer> _timerList = new List<__Timer>();

        // buffer adding timers so we don't edit a collection during iteration
        // [ReadOnly]
        // [SerializeField]
        protected List<__Timer> _timerListToAdd = new List<__Timer>();

        protected virtual void Awake()
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

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        // update all the registered timers on every frame
        [UsedImplicitly]
        protected override void Update()
        {
            this.UpdateAllTimers();
        }

        protected override void UpdateAllTimers()
        {
            if (this._timerListToAdd.Count > 0)
            {
                for (int i = 0; i < this._timerListToAdd.Count; i++)
                {
                    Debug.Assert(this._timerListToAdd[i]._Status != __Timer.Status.None);
                    _timerDic.Add(this._timerListToAdd[i]._Guid.ToString(), this._timerListToAdd[i]);
                    this._timerList.Add(this._timerListToAdd[i]);
                }
                this._timerListToAdd.Clear();
            }

            for (int i = (this._timerList.Count - 1); i >= 0; i--)
            {
                __Timer _timer = this._timerList[i];
                if (_timer._Status != __Timer.Status.None)
                {
                    _timer._Update();
                }
                else
                {
                    Debug.LogFormat(LOG_FORMAT, "<color=red>DeregisterTimer</color>(), _timer : " + _timer);
                    this._timerList.RemoveAt(i);
                    _timerDic.Remove(_timer._Guid.ToString());
                    // Destroy(_timer);
                }
            }
        }

        public override void RegisterTimer(Timer timer)
        {
            throw new System.NotSupportedException("");
        }

        public virtual void RegisterTimer(__Timer timer)
        {
            Debug.Assert(timer._Status == __Timer.Status.Ready);
            Debug.LogFormat(LOG_FORMAT, "<color=red>RegisterTimer</color>(), timer : " + timer);

            this._timerListToAdd.Add(timer);

            // _timer._Status = _Timer.Status.Running;
        }

#if false //
        /// <summary>
        /// Cancels a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to cancel.</param>
        public static void Cancel(_Timer timer)
        {
            Debug.LogFormat(LOG_FORMAT, "Cancel(), timer : " + timer);

            if (timer != null)
            {
                timer.Cancel();
            }
        }

        /// <summary>
        /// Pause a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to pause.</param>
        public static void Pause(_Timer timer)
        {
            if (timer != null)
            {
                timer.Pause();
            }
        }

        /// <summary>
        /// Resume a timer. The main benefit of this over the method on the instance is that you will not get
        /// a <see cref="NullReferenceException"/> if the timer is null.
        /// </summary>
        /// <param name="timer">The timer to resume.</param>
        public static void Resume(_Timer timer)
        {
            if (timer != null)
            {
                timer.Resume();
            }
        }
#endif
    }
}

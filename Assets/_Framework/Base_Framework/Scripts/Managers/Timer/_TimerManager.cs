using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using JetBrains.Annotations;
using UnityEngine;
// using UnityTimer;

namespace _Base_Framework
{
    public class _TimerManager : UnityTimer.__TimerManager
    {
        private static string LOG_FORMAT = "<color=#9300FB><b>[_TimerManager]</b></color> {0}";

        public static new _TimerManager Instance
        {
            get
            {
                return _instance as _TimerManager;
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
                    Debug.Assert(this._timerListToAdd[i]._Status != _Timer.Status.None);
                    _timerDic.Add(this._timerListToAdd[i]._Guid.ToString(), this._timerListToAdd[i]);
                    this._timerList.Add(this._timerListToAdd[i]);
                }
                this._timerListToAdd.Clear();
            }

            for (int i = (this._timerList.Count - 1); i >= 0; i--)
            {
                _Timer _timer = this._timerList[i] as _Timer;
                if (_timer._Status != _Timer.Status.None)
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

        public override void RegisterTimer(UnityTimer.__Timer timer)
        {
            throw new System.NotSupportedException("");
        }

        public virtual void RegisterTimer(_Timer timer)
        {
            Debug.Assert(timer._Status == _Timer.Status.Ready);
            Debug.LogFormat(LOG_FORMAT, "<color=red>RegisterTimer</color>(), timer : " + timer);

            this._timerListToAdd.Add(timer);

            // _timer._Status = _Timer.Status.Running;
        }

    }
}

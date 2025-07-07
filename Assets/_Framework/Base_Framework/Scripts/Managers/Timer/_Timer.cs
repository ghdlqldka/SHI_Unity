using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace _Base_Framework
{
    [Serializable]
    public class _Timer : UnityTimer.__Timer
    {
        private static string LOG_FORMAT = "<color=#02F5FF><b>[_Timer]</b></color> {0}";

        public _Timer() : base()
        {
            //
        }

        public _Timer(float duration, Action<Guid> onCompleted, Action<Guid, float> onUpdated, bool isLoop, bool usesRealTime) 
            : base()
        {
            this._Guid = Guid.NewGuid();
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!, _id : <b><color=yellow>" + _id + "</color></b>");

            this.Duration = duration;
            this._onComplete = onCompleted;
            this._onUpdate = onUpdated;

            this.IsLoop = isLoop;
            this.UsesRealTime = usesRealTime;

            this._startTime = this.GetSinceStarted();
            this._lastUpdateTime = this._startTime;

            this._timeElapsedBeforeCancel = null; // Not used!!!!!
            this._timeElapsedBeforePause = null;

            this._autoDestroyOwner = null; // Not used!!!!!
            this._hasAutoDestroyOwner = false; // Not used!!!!!

            this._Status = Status.Ready;
        }

        public override void _Update()
        {
            Debug.Assert(_Status != Status.None);
            if (_Status == Status.Ready || _Status == Status.Completed)
            {
                return;
            }

            if (_Status == Status.Paused)
            {
                this._startTime += this.GetTimeDelta();
                this._lastUpdateTime = this.GetSinceStarted();
                return;
            }

            // Running
            Debug.Assert(_Status == Status.Running);

            this._lastUpdateTime = this.GetSinceStarted();
            TimeElapsed = this.GetSinceStarted() - this._startTime;

            if (this._onUpdate != null)
            {
                this._onUpdate(_Guid, TimeElapsed);
            }

            if (this.GetSinceStarted() >= this.GetFireTime())
            {
                if (this.IsLoop == true)
                {
                    if (this._onComplete != null)
                    {
                        this._onComplete(_Guid);
                    }
                    this._startTime = this.GetSinceStarted();
                    TimeElapsed = 0;
                }
                else
                {
                    TimeElapsed = Duration;
                    // this.isCompleted = true;
                    this._Status = Status.Completed;
                    if (this._onComplete != null)
                    {
                        this._onComplete(_Guid);
                    }
                }
            }
        }

        public override void Play()
        {
            Debug.AssertFormat(_Status == Status.Ready || _Status == Status.Completed, "_Status : " + _Status);

            this._startTime = this.GetSinceStarted();
            this._lastUpdateTime = this._startTime;
            _Status = Status.Running;
        }

        /// <summary>
        /// Stop a timer that is in-progress or paused. The timer's on completion callback will not be called.
        /// </summary>
        public override void Cancel()
        {
            Debug.LogFormat(LOG_FORMAT, "Cancel(), currentStatus : " + _Status);

            Debug.Assert(this._timeElapsedBeforeCancel == null);
            // this._timeElapsedBeforeCancel = this.GetTimeElapsed();
            this._timeElapsedBeforePause = null;

            _Status = Status.None;
        }

        /// <summary>
        /// Pause a running timer. A paused timer can be resumed from the same point it was paused.
        /// </summary>
        public override void Pause()
        {
            Debug.Assert(this._Status == Status.Running);

            this._timeElapsedBeforePause = TimeElapsed;
            this._Status = Status.Paused;
        }

        /// <summary>
        /// Continue a paused timer. Does nothing if the timer has not been paused.
        /// </summary>
        public override void Resume()
        {
            Debug.Assert(this._Status == Status.Paused);

            this._timeElapsedBeforePause = null;
            this._Status = Status.Running;
        }

    }
}

using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace UnityTimer
{
    [Serializable]
    public class __Timer : Timer
    {
        private static string LOG_FORMAT = "<color=#02F5FF><b>[__Timer]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected string _id;
        protected Guid _guid;
        public Guid _Guid
        {
            get
            {
                return _guid;
            }
            protected set
            {
                _id = value.ToString();
                _guid = value;
            }
        }

        public enum Status
        {
            None,

            Ready,
            Running, // prev Ready, Completed
            Paused,
            Completed,
        }

        [ReadOnly]
        [SerializeField]
        protected Status _status;
        public Status _Status
        {
            get
            {
                return _status;
            }
            set
            {
                Debug.LogFormat(LOG_FORMAT, "_Status : <b><color=red>" + value + "</color></b>" + ", GUID : <b><color=yellow>" + _Guid + "</color></b>");
                _status = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        // public float duration { get; protected set; }
        protected float _duration;
        public float Duration 
        { 
            get
            {
                return _duration;
            }
            protected set
            {
                _duration = value;
                // duration = value;
            }
        }

        public new bool isLooped 
        { 
            get
            {
                throw new System.NotSupportedException("");
            }
            set
            {
                throw new System.NotSupportedException("");
            }
        }
        [ReadOnly]
        [SerializeField]
        protected bool _isLoop = false;
        public bool IsLoop
        {
            get
            {
                return _isLoop;
            }
            set
            {
                _isLoop = value;
            }
        }

        /// <summary>
        /// Whether or not the timer completed running. This is false if the timer was cancelled.
        /// </summary>
        public new bool isCompleted 
        {
            get
            {
                throw new System.NotSupportedException("");
            }
            set
            {
                throw new System.NotSupportedException("");
            }
        }

        /// <summary>
        /// Whether the timer uses real-time or game-time. Real time is unaffected by changes to the timescale
        /// of the game(e.g. pausing, slow-mo), while game time is affected.
        /// </summary>
        public new bool usesRealTime // Real time is unaffected by changes to the timescale
        {
            get
            {
                throw new System.NotSupportedException("");
            }
            protected set
            {
                throw new System.NotSupportedException("");
            }
        }
        [ReadOnly]
        [SerializeField]
        protected bool _usesRealTime = false;
        public bool UsesRealTime // Real time is unaffected by changes to the timescale
        { 
            get
            {
                return _usesRealTime;

            }
            protected set
            {
                _usesRealTime = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected float _timeElapsed;
        public float TimeElapsed
        {
            get
            {
                return _timeElapsed;
            }
            protected set
            {
                _timeElapsed = value;
                _timeRemaining = this.Duration - _timeElapsed;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected float _timeRemaining;
        public float TimeRemaining
        {
            get
            {
                return _timeRemaining;
            }
        }

        /// <summary>
        /// Whether the timer is currently paused.
        /// </summary>
        public new bool isPaused
        {
            get 
            {
                throw new System.NotSupportedException("");
            }
        }

        protected bool _isCancelled = false;
        /// <summary>
        /// Whether or not the timer was cancelled.
        /// </summary>
        public new bool isCancelled
        {
            // get { return this._timeElapsedBeforeCancel.HasValue; }
            get
            {
                Debug.Assert(_timeElapsedBeforeCancel == null);
                return _isCancelled;
            }
            set
            {
                Debug.Assert(_timeElapsedBeforeCancel == null);
                _isCancelled = value;
            }
        }

        /// <summary>
        /// Get whether or not the timer has finished running for any reason.
        /// </summary>
        public new bool isDone
        {
            get 
            {
                // return this.isCompleted || this.isCancelled || this.isOwnerDestroyed; 
                throw new System.NotSupportedException("");
            }
        }

        protected bool isOwnerDestroyed
        {
            get
            {
                // get { return this._hasAutoDestroyOwner && this._autoDestroyOwner == null; }
                throw new System.NotSupportedException("");
            }
        }

        protected new Action<Guid> _onComplete;
        protected new Action<Guid, float> _onUpdate;

        public override string ToString()
        {
            return "GUID : <b><color=yellow>" + _id + "</color></b>, Duration : <color=yellow>" + Duration + "</color>, IsLoop : <color=yellow>" + IsLoop + "</color>" 
                + ", UsesRealTime : <color=yellow>" + UsesRealTime + "</color>";
        }

        public __Timer() : base()
        {
            //
        }

        public __Timer(float duration, Action<Guid> onCompleted, Action<Guid, float> onUpdated, bool isLoop, bool usesRealTime) : base()
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
            TimeElapsed = 0;
        }

        public static __Timer Register(float duration, Action onCompleted, Action<float> onUpdated = null,
            bool isLoop = false, bool useRealTime = false)
        {
            throw new System.NotSupportedException("");
        }

        public static new void Cancel(Timer timer)
        {
            throw new System.NotSupportedException("");
        }

        public static new void Pause(Timer timer)
        {
            throw new System.NotSupportedException("");
        }

        public static new void Resume(Timer timer)
        {
            throw new System.NotSupportedException("");
        }

        protected override float GetWorldTime()
        {
            throw new System.NotSupportedException("");
        }

        protected virtual float GetSinceStarted()
        {
            if (UsesRealTime == true)
            {
                // The real time in seconds since the game started (Read Only).
                return Time.realtimeSinceStartup; // unaffected by changes to the timescale
            }
            else
            {
                // The time at the beginning of the current frame in seconds since the start of the application (Read Only).
                return Time.time; // affected by changes to the timescale
            }
        }

        protected override float GetFireTime()
        {
            return this._startTime + this.Duration;
        }

        protected override float GetTimeDelta()
        {
            return this.GetSinceStarted() - this._lastUpdateTime;
        }

        public override void Update()
        {
            throw new System.NotSupportedException("");
        }

        public virtual void _Update()
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

        public virtual void Play()
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

        /// <summary>
        /// Get how many seconds have elapsed since the start of this timer's current cycle.
        /// </summary>
        /// <returns>The number of seconds that have elapsed since the start of this timer's current cycle, i.e.
        /// the current loop if the timer is looped, or the start if it isn't.
        ///
        /// If the timer has finished running, this is equal to the duration.
        ///
        /// If the timer was cancelled/paused, this is equal to the number of seconds that passed between the timer
        /// starting and when it was cancelled/paused.</returns>
        public override float GetTimeElapsed()
        {
            throw new System.NotSupportedException("");
        }

        /// <summary>
        /// Get how many seconds remain before the timer completes.
        /// </summary>
        /// <returns>The number of seconds that remain to be elapsed until the timer is completed. A timer
        /// is only elapsing time if it is not paused, cancelled, or completed. This will be equal to zero
        /// if the timer completed.</returns>
        public override float GetTimeRemaining()
        {
            throw new System.NotSupportedException("");
        }

        /// <summary>
        /// Get how much progress the timer has made from start to finish as a ratio.
        /// </summary>
        /// <returns>A value from 0 to 1 indicating how much of the timer's duration has been elapsed.</returns>
        public override float GetRatioComplete()
        {
            return this.TimeElapsed / this.Duration;
        }

        /// <summary>
        /// Get how much progress the timer has left to make as a ratio.
        /// </summary>
        /// <returns>A value from 0 to 1 indicating how much of the timer's duration remains to be elapsed.</returns>
        public override float GetRatioRemaining()
        {
            return TimeRemaining / this.Duration;
        }
    }
}

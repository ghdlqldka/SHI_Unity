using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace _Magenta_Framework
{
    [Serializable]
    public class TimerEx : _Base_Framework._Timer
    {
        private static string LOG_FORMAT = "<color=#02F5FF><b>[_Timer]</b></color> {0}";

        public TimerEx() : base()
        {
            //
        }

        public TimerEx(float duration, Action<Guid> onCompleted, Action<Guid, float> onUpdated, bool isLoop, bool usesRealTime) 
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

    }
}

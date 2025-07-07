using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTG
{
    public class _RTUndoRedo : RTUndoRedo
    {
        private static string LOG_FORMAT = "<color=#BFA259><b>[_RTUndoRedo]</b></color> {0}";

        public static _RTUndoRedo Instance
        {
            get
            {
                return Get as _RTUndoRedo;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }
    }
}

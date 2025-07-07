using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_Framework
{
    public class RTUndoRedoEx : RTG._RTUndoRedo
    {
        private static string LOG_FORMAT = "<color=#RTUndoRedoEx><b>[_RTUndoRedo]</b></color> {0}";

        public static new RTUndoRedoEx Instance
        {
            get
            {
                return Get as RTUndoRedoEx;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }
    }
}

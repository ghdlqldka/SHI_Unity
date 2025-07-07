using UnityEngine;
using System;
using System.Collections.Generic;
using RTG;

namespace _Magenta_WebGL
{
    public class MWGL_RTUndoRedo : _Magenta_Framework.RTUndoRedoEx
    {
        public static new MWGL_RTUndoRedo Instance
        {
            get
            {
                return Get as MWGL_RTUndoRedo;
            }
        }
    }
}

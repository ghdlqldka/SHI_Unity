using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTG;

namespace _Magenta_WebGL
{
    public class MWGL_RTFocusCamera : _Magenta_Framework.RTFocusCameraEx
    {
        public static new MWGL_RTFocusCamera Instance
        {
            get
            {
                return Get as MWGL_RTFocusCamera;
            }
        }
    }
}

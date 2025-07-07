using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RTG;

namespace _Magenta_Framework
{
    public class RTFocusCameraEx : RTG._RTFocusCamera
    {
        public static new RTFocusCameraEx Instance
        {
            get
            {
                return Get as RTFocusCameraEx;
            }
        }
    }
}

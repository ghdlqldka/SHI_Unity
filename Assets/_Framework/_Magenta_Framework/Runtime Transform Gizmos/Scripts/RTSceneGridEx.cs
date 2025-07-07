using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_Framework
{
    [Serializable]
    public class RTSceneGridEx : RTG._RTSceneGrid
    {
        public static new RTSceneGridEx Instance
        {
            get
            {
                return Get as RTSceneGridEx;
            }
        }
    }
}

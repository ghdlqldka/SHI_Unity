using UnityEngine;
using System;
using System.Collections.Generic;

namespace _Magenta_WebGL
{
    [Serializable]
    public class MWGL_RTSceneGrid : _Magenta_Framework.RTSceneGridEx
    {
        public static new MWGL_RTSceneGrid Instance
        {
            get
            {
                return Get as MWGL_RTSceneGrid;
            }
        }
    }
}

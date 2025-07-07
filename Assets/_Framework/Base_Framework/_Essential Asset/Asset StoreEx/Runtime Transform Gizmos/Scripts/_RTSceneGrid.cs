using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTG
{
    [Serializable]
    public class _RTSceneGrid : RTSceneGrid
    {
        public static _RTSceneGrid Instance
        {
            get
            {
                return Get as _RTSceneGrid;
            }
        }
    }
}

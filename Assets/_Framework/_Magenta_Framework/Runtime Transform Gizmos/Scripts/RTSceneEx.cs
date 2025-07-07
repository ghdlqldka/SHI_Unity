using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using RTG;

namespace _Magenta_Framework
{
    // TODO: Manual vs Automatic object management;
    public class RTSceneEx : RTG._RTScene
    {
        public static new RTSceneEx Instance
        {
            get
            {
                return Get as RTSceneEx;
            }
        }
    }
}

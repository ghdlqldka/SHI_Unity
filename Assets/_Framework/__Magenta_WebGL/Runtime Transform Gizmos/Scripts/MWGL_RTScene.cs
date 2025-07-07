using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using RTG;

namespace _Magenta_WebGL
{
    // TODO: Manual vs Automatic object management;
    public class MWGL_RTScene : _Magenta_Framework.RTSceneEx
    {
        public static new MWGL_RTScene Instance
        {
            get
            {
                return Get as MWGL_RTScene;
            }
        }
    }
}

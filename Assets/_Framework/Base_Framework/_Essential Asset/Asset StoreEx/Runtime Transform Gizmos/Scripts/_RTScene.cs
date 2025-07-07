using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace RTG
{
    // TODO: Manual vs Automatic object management;
    public class _RTScene : RTScene
    {
        private static string LOG_FORMAT = "<color=#0AE090><b>[_RTScene]</b></color> {0}";

        public static _RTScene Instance
        {
            get
            {
                return Get as _RTScene;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + 
                "</b>, _settings.PhysicsMode : <b><color=yellow>" + _settings.PhysicsMode + "</color></b>");
        }
    }
}

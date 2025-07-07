using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lean.Pool
{
    public class UI_LeanPoolHandler : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FFF4D6><b>[UI_LeanPoolHandler]</b></color> {0}";

        [SerializeField]
        protected _LeanGameObjectPool leanGameObjectPool;

        public void OnClickSpawnButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>Spawn</b>Button()");

            leanGameObjectPool.Spawn();
        }

        public void OnClickDespawnOldestButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickDespawnOldestButton()");

            leanGameObjectPool.DespawnOldest();
        }

        public void OnClickDespawnAllButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickDespawnAllButton()");

            leanGameObjectPool.DespawnAll();
        }
    }
}
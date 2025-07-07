using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _Test_3DModelColorLerp : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=white><b>[Test_3DModelColorLerp]</b></color> {0}";

        [SerializeField]
        protected __3DModelColorLerp colorLerp;

        public virtual void OnTriggerGameObjectRub(_GameObjectRub gameObjectRub)
        {
            // throw new System.NotImplementedException();
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=red>OnTrigger</color>GameObjectRub</b>(), gameObject : " + gameObjectRub.gameObject.name);

            colorLerp.DoLerp(OnEndLerp);
            this.GetComponent<Collider>().enabled = false;
        }

        protected virtual void OnEndLerp()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnEndLerp()");
        }
    }
}
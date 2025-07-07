using UnityEngine;

namespace BioIK {

	//This objective aims to minimise the rotational distance between the transform and the target.
	// [AddComponentMenu("")]
	public class _Orientation : Orientation
    {

        private static string LOG_FORMAT = "<color=#E0BA4B><b>[_Orientation]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        protected override void OnDestroy()
        {
            //
        }
    }

}
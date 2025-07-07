using UnityEngine;

namespace BioIK {

	//This objective aims to minimise the viewing distance between the transform and the target.
	// [AddComponentMenu("")]
	public class _LookAt : LookAt
    {
        private static string LOG_FORMAT = "<color=#B604FF><b>[_LookAt]</b></color> {0}";

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
using UnityEngine;

//!!!!!!
//This objective type is still under development
//!!!!!!

namespace BioIK 
{
	//This objective aims to minimise the joint configuration changes between consecutive solutions.
	//It should only be used once as it acts as a global objective for the whole body posture.
	//Preferably add it to the root of your character.
	// [AddComponentMenu("")]
	public class _Displacement : Displacement
    {
        private static string LOG_FORMAT = "<color=#B604FF><b>[_Position]</b></color> {0}";

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
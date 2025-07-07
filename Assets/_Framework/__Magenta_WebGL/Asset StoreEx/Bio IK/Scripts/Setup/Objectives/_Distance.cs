using UnityEngine;

//!!!!!!
//This objective type is still under development
//!!!!!!

namespace BioIK 
{

	//This objective aims to keep particular distances to the defined transform positions. This can be used to integrate
	//real-time collision avoidance. However, note that collision avoidance is typically a very challenging thing for motion generation,
	//so please do not expect any wonders or some sort of black magic. It works well for typical scenarios, but it will not solve the world for you.
	//Note that you should use preferably small weights in order to get good-looking results. Best thing is to play around with it and see what happens.
	//It is not generally clear how to chose those weights.
	// [AddComponentMenu("")]
	public class _Distance : Distance
    {
        private static string LOG_FORMAT = "<color=#B604FF><b>[_Distance]</b></color> {0}";

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
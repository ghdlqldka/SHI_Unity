using UnityEngine;

namespace BioIK 
{

	//This objective aims to minimise both the translational and rotational distance between
	//the projected transform with respect to the normal of the game object to other collideable objects in the scene.
	// [AddComponentMenu("")]
	public class _Projection : Projection
    {

        private static string LOG_FORMAT = "<color=#E0BA4B><b>[_Projection]</b></color> {0}";

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
using BioIK;
using UnityEngine;

namespace _SHI_BA
{

	// [AddComponentMenu("")]
	public class BA_BioSegment : BioIK._BioSegment
    {

        private static string LOG_FORMAT = "<color=#11E3EC><b>[BA_BioSegment]</b></color> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        public BA_BioSegment Create(BA_BioIK character)
        {
            Debug.LogFormat(LOG_FORMAT, "Create()");

            Character = character;
            Transform = this.transform;
            // hideFlags = HideFlags.HideInInspector;
            return this;
        }
    }

}
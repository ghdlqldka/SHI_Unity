using UnityEngine;

//!!!!!!
//This objective type is still under development
//!!!!!!

namespace BioIK {

	//This objective aims to keep particular joint values acting as soft-constraints. Using this requires
	//a joint added to the segment, and introduces some sort of stiffness controlled by the weight to the joint.
	//Currently, you will require one objective for each >enabled< motion axis you wish to control.
	[AddComponentMenu("")]
	public class _JointValue : JointValue
    {
        private static string LOG_FORMAT = "<color=#997AC1><b>[_JointValue]</b></color> {0}";

        protected override void Awake()
        {
			Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : " + this.gameObject.name);
        }

        protected override void OnDestroy()
        {
            //
        }

        public BioJoint.Motion GetMotion()
		{
			return Motion;
		}

        protected override bool IsValid()
        {
            if (Segment.Joint == null)
            {
                return false;
            }
            if (X == false && Y == false && Z == false)
            {
                return false;
            }
            if (X && Segment.Joint.X.IsEnabled() == false)
            {
                return false;
            }
            if (Y && Segment.Joint.Y.IsEnabled() == false)
            {
                return false;
            }
            if (Z && Segment.Joint.Z.IsEnabled() == false)
            {
                return false;
            }
            return true;
        }

    }
}
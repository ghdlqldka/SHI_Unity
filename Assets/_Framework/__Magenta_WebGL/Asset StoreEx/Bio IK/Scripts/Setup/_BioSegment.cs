using UnityEngine;

namespace BioIK
{

	// [AddComponentMenu("")]
	public class _BioSegment : BioSegment
    {
        private static string LOG_FORMAT = "<color=#11E3EC><b>[_BioSegment]</b></color> {0}";

        // public BioIK Character;
        public _BioIK _Character
        {
            get
            {
                return Character as _BioIK;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        public _BioSegment Create(_BioIK character)
        {
            Debug.LogFormat(LOG_FORMAT, "Create()");

            Character = character;
            Transform = this.transform;
            // hideFlags = HideFlags.HideInInspector;
            return this;
        }

        public override BioJoint AddJoint()
        {
            if (Joint != null)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "The segment already has a joint.");
            }
            else
            {
                Joint = _Utility.AddBioJoint(this);
                Character.Refresh();
            }
            return Joint;
        }

        public override BioObjective AddObjective(ObjectiveType type)
        {
            BioObjective objective = _Utility.AddObjective(this, type);
            if (objective == null)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "The objective could not be found.");
                return null;
            }
            else
            {
                System.Array.Resize(ref Objectives, Objectives.Length + 1);
                Objectives[Objectives.Length - 1] = objective;
                Character.Refresh();
                return Objectives[Objectives.Length - 1];
            }
        }
    }

}
using UnityEngine;

namespace BioIK 
{

	// [AddComponentMenu("")]
	public class _BioJoint : BioJoint
    {
        private static string LOG_FORMAT = "<color=#1FE3EC><b>[_BioJoint]</b></color> {0}";

        public class _Motion : BioJoint.Motion
        {
            public _Motion(/*_BioJoint joint, Vector3 axis*/)
            {
                /*
                Joint = joint;
                Axis = axis;
                */
            }

            public _Motion(_BioJoint joint, Vector3 axis) : base()
            {
                Joint = joint;
                Axis = axis;
            }

            protected override void UpdateRealistic()
            {
                if (Time.deltaTime == 0f)
                {
                    return;
                }

                // double maxVel = Joint.JointType == JointType.Rotational ? Utility.Rad2Deg * Joint.Segment.Character.MaximumVelocity : Joint.Segment.Character.MaximumVelocity;
                double maxVel;
                if (Joint.JointType == JointType.Rotational)
                {
                    maxVel = Utility.Rad2Deg * Joint.Segment.Character.MaximumVelocity;
                }
                else
                {
                    maxVel = Joint.Segment.Character.MaximumVelocity;
                }
                double maxAcc = Joint.JointType == JointType.Rotational ? Utility.Rad2Deg * Joint.Segment.Character.MaximumAcceleration : Joint.Segment.Character.MaximumAcceleration;

                //Compute current error
                CurrentError = TargetValue - CurrentValue;

                //Minimum distance to stop: s = |(v^2)/(2a_max)| + |a/2*t^2| + |v*t|
                double stoppingDistance =
                    System.Math.Abs((CurrentVelocity * CurrentVelocity) / (2.0 * maxAcc * Slowdown))
                    + System.Math.Abs(CurrentAcceleration) / 2.0 * Time.deltaTime * Time.deltaTime
                    + System.Math.Abs(CurrentVelocity) * Time.deltaTime;

                if (System.Math.Abs(CurrentError) > stoppingDistance)
                {
                    //Accelerate
                    CurrentAcceleration = System.Math.Sign(CurrentError) * System.Math.Min(System.Math.Abs(CurrentError) / Time.deltaTime, maxAcc * Speedup);
                }
                else
                {
                    //Deccelerate
                    if (CurrentError == 0.0)
                    {
                        CurrentAcceleration = -System.Math.Sign(CurrentVelocity) *
                        System.Math.Min(System.Math.Abs(CurrentVelocity) / Time.deltaTime, maxAcc);

                    }
                    else
                    {
                        CurrentAcceleration = -System.Math.Sign(CurrentVelocity) *
                        System.Math.Min(
                            System.Math.Min(System.Math.Abs(CurrentVelocity) / Time.deltaTime, maxAcc),
                            System.Math.Abs((CurrentVelocity * CurrentVelocity) / (2.0 * CurrentError))
                        );
                    }
                }

                //Compute new velocity
                CurrentVelocity += CurrentAcceleration * Time.deltaTime;

                //Clamp velocity
                if (CurrentVelocity > maxVel)
                {
                    CurrentVelocity = maxVel;
                }
                if (CurrentVelocity < -maxVel)
                {
                    CurrentVelocity = -maxVel;
                }

                //Update Current Value
                CurrentValue += CurrentVelocity * Time.deltaTime;
            }
        }

        // public BioSegment Segment;
        public _BioSegment _Segment
        {
            get
            {
                return Segment as _BioSegment;
            }
        }

        // [SerializeField] protected Vector3 Orientation = Vector3.zero;				//Joint orientation		
        protected Vector3 _Orientation
        {
            get
            {
                return Orientation;
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake(), this.gameObject : <b>" + this.gameObject.name + "</b>");
        }

        public _BioJoint Create(_BioSegment segment)
        {
            Debug.LogFormat(LOG_FORMAT, "Create()");

            Segment = segment;
            _Segment.Transform.hideFlags = HideFlags.NotEditable;
            // hideFlags = HideFlags.HideInInspector;

            X = new _Motion(this, Vector3.right);
            Y = new _Motion(this, Vector3.up);
            Z = new _Motion(this, Vector3.forward);

            SetDefaultFrame(this.transform.localPosition, this.transform.localRotation);
            SetAnchor(Anchor);

            Vector3 forward = Vector3.zero;
            if (_Segment.Childs.Length == 1)
            {
                forward = _Segment.Childs[0].Transform.localPosition;
            }
            else if (_Segment.Parent != null)
            {
                forward = Quaternion.Inverse(_Segment.Transform.localRotation) * _Segment.Transform.localPosition;
            }
            if (forward.magnitude != 0f)
            {
                SetOrientation(Quaternion.LookRotation(forward, Vector3.up).eulerAngles);
            }
            else
            {
                SetOrientation(_Orientation);
            }

            LastPosition = this.transform.localPosition;
            LastRotation = this.transform.localRotation;

            return this;
        }
    }

}
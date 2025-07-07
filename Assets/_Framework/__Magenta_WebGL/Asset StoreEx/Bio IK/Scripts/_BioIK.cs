using UnityEngine;

namespace BioIK 
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class _BioIK : BioIK
    {
        private static string LOG_FORMAT = "<color=#EC007A><b>[_BioIK]</b></color> {0}";

        // public BioSegment Root = null;
        public _BioSegment _Root
        {
            get 
            { 
                return Root as _BioSegment;
            }
            set
            {
                Root = value;
            }
        }
        // public Evolution Evolution = null;
        public _Evolution _evolution
        {
            get
            {
                return Evolution as _Evolution;
            }
            set
            {
                if (Evolution != value)
                {
                    Evolution = value;
                }
            }
        }
        // public double[] Solution = null;
        public double[] _solutions
        {
            get
            {
                return Solution;
            }
            protected set
            {
                Solution = value;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake(), this.gameObject : <b><color=cyan>" + this.gameObject.name + "</color></b>");

            UseThreading = false; // In Unity WebGL, multiple threads cannot be used by default.
            Refresh();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Initialise();
        }

        protected override void OnDisable()
        {
            DeInitialise();
        }

        protected override void Update()
        {
            PrecaptureAnimation(Root);
        }

        protected override void LateUpdate()
        {
            PostcaptureAnimation(_Root);

            UpdateData(_Root);

            for (int i = 0; i < _solutions.Length; i++)
            {
                _solutions[i] = _evolution.GetModel().MotionPtrs[i].Motion.GetTargetValue(true);
            }
            _solutions = _evolution.Optimise(Generations, _solutions);

            for (int i = 0; i < _solutions.Length; i++)
            {
                BioJoint.Motion motion = _evolution.GetModel().MotionPtrs[i].Motion;
                motion.SetTargetValue(_solutions[i], true);
                /*
				if(motion.Joint.GetJointType() == JointType.Revolute) {
					motion.SetTargetValue((float)_solutions[i]);
				} else if(motion.Joint.GetJointType() == JointType.Continuous) {
					motion.SetTargetValue(motion.GetTargetValue() + Mathf.Deg2Rad*Mathf.DeltaAngle(Mathf.Rad2Deg*motion.GetTargetValue(), Mathf.Rad2Deg*(float)Solution[i]));
				} else if(motion.Joint.GetJointType() == JointType.Prismatic) {
					motion.SetTargetValue((float)_solutions[i]);
				} else if(motion.Joint.GetJointType() == JointType.Floating) {
					motion.SetTargetValue((float)_solutions[i]);
				}
				*/
            }

            ProcessMotion(_Root);
        }

        public override void Refresh(bool evolution = true)
        {
            // base.Refresh(evolution);

            if (Destroyed == true)
            {
                return;
            }

            for (int i = 0; i < Segments.Count; i++)
            {
                if (Segments[i] == null)
                {
                    Segments.RemoveAt(i);
                    i--;
                }
            }
            Refresh(this.transform);

            BioSegment segment = FindSegment(this.transform);
            if (segment is _BioSegment)
            {
                _Root = segment as _BioSegment;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "======> <color=red>cccccchhhhhhaaaaaannnnngggggggeeeeee to _BioSegment</color>!!!!!!!!! <========, " + this.transform.name);
                Debug.LogWarningFormat(LOG_FORMAT, "" + segment.GetType().Name);
                Root = segment;
            }
            Debug.Assert(_Root != null);

            if (evolution == true && Application.isPlaying == true)
            {
                DeInitialise();
                Initialise();
                int DoF = _evolution.GetModel().GetDoF();
                _solutions = new double[DoF];
            }
        }

        protected override void Refresh(Transform t)
        {
            // base.Refresh(t);

            BioSegment segment = FindSegment(t);
            if (segment == null)
            {
                segment = _Utility.AddBioSegment(this, t);
                Segments.Add(segment);
            }
            _BioSegment _segment = segment as _BioSegment;
            Debug.AssertFormat(_segment != null, "_segment is NOT <color=red>_BioSegment</color> type!!!!!!, <b>" + segment.name + "</b>");
            _segment.Character = this;
            _segment.RenewRelations();

            for (int i = 0; i < t.childCount; i++)
            {
                Refresh(t.GetChild(i));
            }
        }

        protected override void Initialise()
        {
            Debug.LogFormat(LOG_FORMAT, "@@@@@@@@@@@@@@ Initialise(), _evolution : " + _evolution);
            if (_evolution == null)
            {
                Debug.Assert(UseThreading == false);
                _evolution = new _Evolution(new _Model(this), PopulationSize, Elites/*, UseThreading*/);
            }
        }

        protected override void DeInitialise()
        {
            if (_evolution != null)
            {
#if false // In Unity WebGL, multiple threads cannot be used by default.
                _evolution.Kill();
#endif
                _evolution = null;
            }
        }

        public override void SetThreading(bool enabled)
        {
#if true //
            throw new System.NotSupportedException("In Unity WebGL, multiple threads cannot be used by default.");
#else
            if (UseThreading != enabled)
            {
                UseThreading = enabled;
                if (Application.isPlaying == true)
                {
                    Refresh();
                }
            }
#endif
        }

        public override bool GetThreading()
        {
#if true //
            throw new System.NotSupportedException("In Unity WebGL, multiple threads cannot be used by default.");
#else
            return UseThreading;
#endif
        }

        public override void UpdateData(BioSegment segment)
        {
            if (segment.Joint != null)
            {
                if (segment.Joint.enabled)
                {
                    segment.Joint.UpdateData();
                }
            }
            for (int i = 0; i < segment.Objectives.Length; i++)
            {
                if (segment.Objectives[i].enabled)
                {
                    segment.Objectives[i].UpdateData();
                }
            }
            for (int i = 0; i < segment.Childs.Length; i++)
            {
                UpdateData(segment.Childs[i]);
            }
        }
    }

}
using UnityEngine;
using BioIK;
using System.Collections;

namespace _SHI_BA
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class BA_BioIK : BioIK._BioIK
    {

        private static string LOG_FORMAT = "<color=#EC007A><b>[BA_BioIK]</b></color> {0}";

        public new BA_BioSegment _Root
        {
            get
            {
                return Root as BA_BioSegment;
            }
            set
            {
                Root = value;
            }
        }

        public new BA_Evolution _evolution
        {
            get
            {
                return Evolution as BA_Evolution;
            }
            set
            {
                if (Evolution != value)
                {
                    Evolution = value;
                }
            }
        }

        [ReadOnly]
        [SerializeField]
        protected bool _autoIK;
        public bool autoIK
        {
            get
            {
                return _autoIK;
            }
            set
            {
                if (_autoIK != value)
                {
                    _autoIK = value;
                }
            }
        }

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
            // base.Awake();

            UseThreading = false; // In Unity WebGL, multiple threads cannot be used by default.

            Refresh();
		}

        protected override void Update()
        {
            // base.Update();
            PrecaptureAnimation(_Root);
        }

        protected override void LateUpdate()
        {
            // base.LateUpdate();

            if (autoIK == false)
            {
                return;
            }

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

        public void SolveIK()
        {
            Refresh();

            // IK 시스템이 준비되지 않았으면 실행하지 않음
            if (_evolution == null || Root == null)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Evolution 또는 Root가 초기화되지 않았습니다.");
                return;
            }

            // LateUpdate에 있던 핵심 로직을 그대로 가져옵니다.
            // 1. 애니메이션 데이터 캡처 (필요시)
            PostcaptureAnimation(Root);

            // 2. 목표 및 조인트 데이터 업데이트
            UpdateData(Root);

            // 3. _solutions 배열 초기화 또는 크기 확인
            if (_solutions == null || _solutions.Length != _evolution.GetModel().GetDoF())
            {
                _solutions = new double[_evolution.GetModel().GetDoF()];
            }

            // 4. 현재 관절값을 최적화의 시작점(seed)으로 사용 (중요)
            for (int i = 0; i < _solutions.Length; i++)
            {
                _solutions[i] = _evolution.GetModel().MotionPtrs[i].Motion.GetTargetValue(true);
            }

            // 5. IK 최적화 실행
            _solutions = _evolution.Optimise(Generations, _solutions);

            // 6. 계산된 결과를 각 관절의 TargetValue에 저장
            for (int i = 0; i < _solutions.Length; i++)
            {
                BioJoint.Motion motion = _evolution.GetModel().MotionPtrs[i].Motion;
                motion.SetTargetValue(_solutions[i], false); // true: 라디안 값으로 설정
            }

            // 7. ProcessMotion을 호출하여 관절의 CurrentValue를 TargetValue로 이동 시작
            ProcessMotion(Root);
        }

        protected override void Initialise()
        {
            Debug.LogFormat(LOG_FORMAT, "Initialise()");

            if (_evolution == null)
            {
                // Debug.Log("[BioIK.Initialise] Creating new Model and Evolution objects...", this);
                BA_Model model = new BA_Model(this); // 이 시점에서 Model.cs의 kuka1 로그 등이 발생합니다.

                // --- 추가된 로그 ---
                int calculatedDoF = model.GetDoF();

                if (calculatedDoF >= 0) // DoF가 음수가 아닌지 확인
                {
                    _evolution = new BA_Evolution(model, PopulationSize, Elites/*, UseThreading*/);
                }
            }
            else
            {
                Debug.Log("[BioIK.Initialise] Evolution object already exists. Skipping re-creation.", this);
            }
        }

        protected override void Refresh(Transform t)
        {
            // base.Refresh(t);

            BioSegment segment = FindSegment(t);
            if (segment == null)
            {
                segment = BA_Utility.AddBioSegment(this, t);
                Segments.Add(segment);
            }
            BA_BioSegment _segment = segment as BA_BioSegment;
            Debug.AssertFormat(_segment != null, "_segment is NOT <color=red>_BioSegment</color> type!!!!!!, <b>" + segment.name + "</b>");
            _segment.Character = this;
            _segment.RenewRelations();

            for (int i = 0; i < t.childCount; i++)
            {
                Refresh(t.GetChild(i));
            }
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
            Debug.LogFormat(LOG_FORMAT, "this.transform : " + this.transform.name);
            BioSegment segment = FindSegment(this.transform);
            if (segment is BA_BioSegment)
            {
                _Root = segment as BA_BioSegment;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "======> cccccchhhhhhaaaaaannnnngggggggeeeeee to BA_BioSegment!!!!!!!!! <========, " + segment.name);
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
    }

}
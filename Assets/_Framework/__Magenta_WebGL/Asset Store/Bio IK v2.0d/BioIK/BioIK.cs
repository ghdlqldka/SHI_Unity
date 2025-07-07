using UnityEngine;
using System.Collections.Generic;

namespace BioIK {

	//[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public class BioIK : MonoBehaviour {

		//public bool SolveInEditMode = false;

		[SerializeField] protected bool UseThreading = true;

		[SerializeField] protected int Generations = 2;
		[SerializeField] protected int PopulationSize = 50;
		[SerializeField] protected int Elites = 2;

		public float Smoothing = 0.5f;
		public float AnimationWeight = 0f;
		public float AnimationBlend = 0f;
		public MotionType MotionType = MotionType.Instantaneous;
		public float MaximumVelocity = 3f;
		public float MaximumAcceleration = 3f;

		public List<BioSegment> Segments = new List<BioSegment>();

		public BioSegment Root = null;
		public Evolution Evolution = null;
		public double[] Solution = null;

		protected bool Destroyed = false;

		//Custom Inspector Helpers
		public BioSegment SelectedSegment = null;
		public Vector2 Scroll = Vector2.zero;

		protected virtual void Awake() {
			Refresh();
		}

		void Start() {
			
		}

		void OnDestroy() {
			Destroyed = true;
			DeInitialise();
			Utility.Cleanup(transform);
		}

		protected virtual void OnEnable() {
			Initialise();	
		}

        protected virtual void OnDisable() {
			DeInitialise();
		}

		protected virtual void Initialise() {
			if(Evolution == null) {
				Evolution = new Evolution(new Model(this), PopulationSize, Elites, UseThreading);
			}
		}

		protected virtual void DeInitialise() {
			if(Evolution != null) {
				Evolution.Kill();
				Evolution = null;
			}
		}

		protected virtual void Update() {
			PrecaptureAnimation(Root);
		}

		protected virtual void LateUpdate() {
			PostcaptureAnimation(Root);

			UpdateData(Root);

			for(int i=0; i<Solution.Length; i++) {
				Solution[i] = Evolution.GetModel().MotionPtrs[i].Motion.GetTargetValue(true);
			}
			Solution = Evolution.Optimise(Generations, Solution);
			
			for(int i=0; i<Solution.Length; i++) {
				BioJoint.Motion motion = Evolution.GetModel().MotionPtrs[i].Motion;
				motion.SetTargetValue(Solution[i], true);
				/*
				if(motion.Joint.GetJointType() == JointType.Revolute) {
					motion.SetTargetValue((float)Solution[i]);
				} else if(motion.Joint.GetJointType() == JointType.Continuous) {
					motion.SetTargetValue(motion.GetTargetValue() + Mathf.Deg2Rad*Mathf.DeltaAngle(Mathf.Rad2Deg*motion.GetTargetValue(), Mathf.Rad2Deg*(float)Solution[i]));
				} else if(motion.Joint.GetJointType() == JointType.Prismatic) {
					motion.SetTargetValue((float)Solution[i]);
				} else if(motion.Joint.GetJointType() == JointType.Floating) {
					motion.SetTargetValue((float)Solution[i]);
				}
				*/
			}

			ProcessMotion(Root);
		}

		public virtual void SetThreading(bool enabled) {
			if(UseThreading != enabled) {
				UseThreading = enabled;
				if(Application.isPlaying) {
					Refresh();
				}
			}
		}

		public virtual bool GetThreading() {
			return UseThreading;
		}

		public void SetGenerations(int generations) {
			Generations = generations;
		}

		public int GetGenerations() {
			return Generations;
		}

		public void SetPopulationSize(int populationSize) {
			if(PopulationSize != populationSize) {
				PopulationSize = System.Math.Max(1, populationSize);
				Elites = System.Math.Min(populationSize, Elites);
				if(Application.isPlaying) {
					Refresh();
				}
			}
		}

		public int GetPopulationSize() {
			return PopulationSize;
		}

		public void SetElites(int elites) {
			if(Elites != elites) {
				Elites = System.Math.Max(1, elites);
				if(Application.isPlaying) {
					Refresh();
				}
			}
		}

		public int GetElites() {
			return Elites;
		}

		public void ResetPosture(BioSegment segment) {
			if(segment.Joint != null) {
				segment.Joint.X.SetTargetValue(0f);
				segment.Joint.Y.SetTargetValue(0f);
				segment.Joint.Z.SetTargetValue(0f);
				if(!Application.isPlaying) {
					segment.Joint.PrecaptureAnimation();
					segment.Joint.PostcaptureAnimation();
					segment.Joint.UpdateData();
					segment.Joint.ProcessMotion();
				}
			}
			for(int i=0; i<segment.Childs.Length; i++) {
				ResetPosture(segment.Childs[i]);
			}
		}

		public BioSegment FindSegment(Transform t) {
			for(int i=0; i<Segments.Count; i++) {
				if(Segments[i].Transform == t) {
					return Segments[i];
				}
			}
			return null;
		}

		public BioSegment FindSegment(string name) {
			for(int i=0; i<Segments.Count; i++) {
				if(Segments[i].Transform.name == name) {
					return Segments[i];
				}
			}
			return null;
		}

		public List<BioSegment> GetChain(Transform start, Transform end) {
			BioSegment a = FindSegment(start);
			BioSegment b = FindSegment(end);
			if(a == null || b == null) {
				Debug.Log("Could not generate chain for given transforms");
				return null;
			}
			return GetChain(a, b);
		}

		public List<BioSegment> GetChain(BioSegment start, BioSegment end) {
			List<BioSegment> chain = new List<BioSegment>();
			BioSegment segment = end;
			while(true) {
				chain.Add(segment);
				if(segment.Transform == transform || segment.Parent == null) {
					break;
				} else {
					segment = segment.Parent;
				}
			}
			chain.Reverse();
			return chain;
		}

		public virtual void UpdateData(BioSegment segment) {
			if(segment.Joint != null) {
				if(segment.Joint.enabled) {
					segment.Joint.UpdateData();
				}
			}
			for(int i=0; i<segment.Objectives.Length; i++) {
				if(segment.Objectives[i].enabled) {
					segment.Objectives[i].UpdateData();
				}
			}
			for(int i=0; i<segment.Childs.Length; i++) {
				UpdateData(segment.Childs[i]);
			}
		}

		public virtual void Refresh(bool evolution = true) {
			if(Destroyed) {
				return;
			}
			
			for(int i=0; i<Segments.Count; i++) {
				if(Segments[i] == null) {
					Segments.RemoveAt(i);
					i--;
				}
			}
			Refresh(transform);
			Root = FindSegment(transform);

			if(evolution && Application.isPlaying) {
				DeInitialise();
				Initialise();
				Solution = new double[Evolution.GetModel().GetDoF()];
			}
		}

		protected virtual void Refresh(Transform t) {
			BioSegment segment = FindSegment(t);
			if(segment == null) {
				segment = Utility.AddBioSegment(this, t);
				Segments.Add(segment);
			}
			segment.Character = this;
			segment.RenewRelations();
			
			for(int i=0; i<t.childCount; i++) {
				Refresh(t.GetChild(i));
			}
		}

		protected void PrecaptureAnimation(BioSegment segment) {
			if(segment.Joint != null) {
				if(segment.Joint.enabled) {
					segment.Joint.PrecaptureAnimation();
				}
			}
			for(int i=0; i<segment.Childs.Length; i++) {
				PrecaptureAnimation(segment.Childs[i]);
			}
		}

		protected void PostcaptureAnimation(BioSegment segment) {
			if(segment.Joint != null) {
				if(segment.Joint.enabled) {
					segment.Joint.PostcaptureAnimation();
				}
			}
			for(int i=0; i<segment.Childs.Length; i++) {
				PostcaptureAnimation(segment.Childs[i]);
			}
		}

		protected void ProcessMotion(BioSegment segment) {
			if(segment.Joint != null) {
				if(segment.Joint.enabled) {
					segment.Joint.ProcessMotion();
				}
			}
			for(int i=0; i<segment.Childs.Length; i++) {
				ProcessMotion(segment.Childs[i]);
			}
		}

	}

}
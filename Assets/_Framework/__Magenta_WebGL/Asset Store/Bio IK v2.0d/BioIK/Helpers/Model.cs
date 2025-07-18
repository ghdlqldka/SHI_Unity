﻿using UnityEngine;
using System.Collections.Generic;

namespace BioIK {
	public class Model {
		//Reference to the character
		protected BioIK Character;

        //Reference to root
        protected BioSegment Root;

		//Offset to world
		private double OPX, OPY, OPZ;								//Offset rosition to world frame
		private double ORX, ORY, ORZ, ORW;							//Offset rotation to world frame
		private double OSX, OSY, OSZ;								//Offset scale to world Frame
		
		//Linked list of nodes in the model
		public Node[] Nodes = new Node[0];

		//Global pointers to the IK setup
		public MotionPtr[] MotionPtrs = new MotionPtr[0];
		public ObjectivePtr[] ObjectivePtrs = new ObjectivePtr[0];

        //Assigned Configuraton
        protected double[] Configuration;
        protected double[] Gradient;
        protected double[] Losses;

        //Simulated Configuration
        protected double[] PX,PY,PZ,RX,RY,RZ,RW;
        protected double[] SimulatedLosses;

        //Degree of Freedom
        protected int DoF;

        public Model(/*BioIK character*/)
        {
#if false //
			Character = character;

            //Set Root
            Root = Character.FindSegment(Character.transform);

            //Create Root
            AddNode(Root);

            //Build Model
            BioObjective[] objectives = CollectObjectives(Root, new List<BioObjective>());
            for (int i = 0; i < objectives.Length; i++)
            {
                List<BioSegment> chain = Character.GetChain(Root, objectives[i].Segment);
                for (int j = 1; j < chain.Count; j++)
                {
                    AddNode(chain[j]);
                }
            }

            //Assign DoF
            DoF = MotionPtrs.Length;

            //Initialise arrays for single transform modifications
            for (int i = 0; i < Nodes.Length; i++)
            {
                Nodes[i].ObjectiveImpacts = new bool[ObjectivePtrs.Length];
            }
            PX = new double[ObjectivePtrs.Length];
            PY = new double[ObjectivePtrs.Length];
            PZ = new double[ObjectivePtrs.Length];
            RX = new double[ObjectivePtrs.Length];
            RY = new double[ObjectivePtrs.Length];
            RZ = new double[ObjectivePtrs.Length];
            RW = new double[ObjectivePtrs.Length];
            Configuration = new double[MotionPtrs.Length];
            Gradient = new double[MotionPtrs.Length];
            Losses = new double[ObjectivePtrs.Length];
            SimulatedLosses = new double[ObjectivePtrs.Length];

            //Assigns references to all objective nodes that are affected by a parenting node
            for (int i = 0; i < ObjectivePtrs.Length; i++)
            {
                Node node = ObjectivePtrs[i].Node;
                while (node != null)
                {
                    node.ObjectiveImpacts[i] = true;
                    node = node.Parent;
                }
            }

            Refresh();

            //DebugSetup();
#endif
        }
        
		public Model(BioIK character) {
			Character = character;

			//Set Root
			Root = Character.FindSegment(Character.transform);

			//Create Root
			AddNode(Root);
			
			//Build Model
			BioObjective[] objectives = CollectObjectives(Root, new List<BioObjective>());
			for(int i=0; i<objectives.Length; i++) {
				List<BioSegment> chain = Character.GetChain(Root, objectives[i].Segment);
				for(int j=1; j<chain.Count; j++) {
					AddNode(chain[j]);
				}
			}

			//Assign DoF
			DoF = MotionPtrs.Length;

			//Initialise arrays for single transform modifications
			for(int i=0; i<Nodes.Length; i++) {
				Nodes[i].ObjectiveImpacts = new bool[ObjectivePtrs.Length];
			}
			PX = new double[ObjectivePtrs.Length];
			PY = new double[ObjectivePtrs.Length];
			PZ = new double[ObjectivePtrs.Length];
			RX = new double[ObjectivePtrs.Length];
			RY = new double[ObjectivePtrs.Length];
			RZ = new double[ObjectivePtrs.Length];
			RW = new double[ObjectivePtrs.Length];
			Configuration = new double[MotionPtrs.Length];
			Gradient = new double[MotionPtrs.Length];
			Losses = new double[ObjectivePtrs.Length];
			SimulatedLosses = new double[ObjectivePtrs.Length];

			//Assigns references to all objective nodes that are affected by a parenting node
			for(int i=0; i<ObjectivePtrs.Length; i++) {
				Node node = ObjectivePtrs[i].Node;
				while(node != null) {
					node.ObjectiveImpacts[i] = true;
					node = node.Parent;
				}
			}

			Refresh();

			//DebugSetup();
		}

		public int GetDoF() {
			return DoF;
		}

		public virtual BioIK GetCharacter() {
			return Character;
		}

		public void Refresh() {
			//Updates configuration
			for(int i=0; i<Configuration.Length; i++) {
				Configuration[i] = MotionPtrs[i].Motion.GetTargetValue(true);
			}

			//Update offset from world to root
			if(Root.Transform.root == Character.transform) {
				OPX = OPY = OPZ = ORX = ORY = ORZ = 0.0;
				ORW = OSX = OSY = OSZ = 1.0;
			} else {
				Vector3 p = Root.Transform.parent.position;
				Quaternion r = Root.Transform.parent.rotation;
				Vector3 s = Root.Transform.parent.lossyScale;
				OPX = p.x; OPY = p.y; OPZ = p.z;
				ORX = r.x; ORY = r.y; ORZ = r.z; ORW = r.w;
				OSX = s.x; OSY = s.y; OSZ = s.z;
			}

			//Updates the nodes
			Nodes[0].Refresh();
		}

		public void CopyFrom(Model model) {
			OPX = model.OPX;
			OPY = model.OPY;
			OPZ = model.OPZ;
			ORX = model.ORX;
			ORY = model.ORY;
			ORZ = model.ORZ;
			ORW = model.ORW;
			OSX = model.OSX;
			OSY = model.OSY;
			OSZ = model.OSZ;
			for(int i=0; i<DoF; i++) {
				Configuration[i] = model.Configuration[i];
				Gradient[i] = model.Gradient[i];
			}
			for(int i=0; i<ObjectivePtrs.Length; i++) {
				PX[i] = model.PX[i];
				PY[i] = model.PY[i];
				PZ[i] = model.PZ[i];
				RX[i] = model.RX[i];
				RY[i] = model.RY[i];
				RZ[i] = model.RZ[i];
				RW[i] = model.RW[i];
				Losses[i] = model.Losses[i];
				SimulatedLosses[i] = model.SimulatedLosses[i];
			}
			for(int i=0; i<Nodes.Length; i++) {
				Nodes[i].WPX = model.Nodes[i].WPX;
				Nodes[i].WPY = model.Nodes[i].WPY;
				Nodes[i].WPZ = model.Nodes[i].WPZ;
				Nodes[i].WRX = model.Nodes[i].WRX;
				Nodes[i].WRY = model.Nodes[i].WRY;
				Nodes[i].WRZ = model.Nodes[i].WRZ;
				Nodes[i].WRW = model.Nodes[i].WRW;
				Nodes[i].WSX = model.Nodes[i].WSX;
				Nodes[i].WSY = model.Nodes[i].WSY;
				Nodes[i].WSZ = model.Nodes[i].WSZ;

				Nodes[i].LPX = model.Nodes[i].LPX;
				Nodes[i].LPY = model.Nodes[i].LPY;
				Nodes[i].LPZ = model.Nodes[i].LPZ;
				Nodes[i].LRX = model.Nodes[i].LRX;
				Nodes[i].LRY = model.Nodes[i].LRY;
				Nodes[i].LRZ = model.Nodes[i].LRZ;
				Nodes[i].LRW = model.Nodes[i].LRW;

				//Nodes[i].RootX = model.Nodes[i].RootX;
				//Nodes[i].RootY = model.Nodes[i].RootY;
				//Nodes[i].RootZ = model.Nodes[i].RootZ;
				Nodes[i].XValue = model.Nodes[i].XValue;
				Nodes[i].YValue = model.Nodes[i].YValue;
				Nodes[i].ZValue = model.Nodes[i].ZValue;
			}
		}

		//Computes the loss as the RMSE over all objectives
		public double ComputeLoss(double[] configuration) {
			FK(configuration);
			double loss = 0.0;
			for(int i=0; i<ObjectivePtrs.Length; i++) {
				Node node = ObjectivePtrs[i].Node;
				Losses[i] = ObjectivePtrs[i].Objective.ComputeLoss(node.WPX, node.WPY, node.WPZ, node.WRX, node.WRY, node.WRZ, node.WRW, node, Configuration);
				loss += Losses[i];
			}
			return System.Math.Sqrt(loss / (double)ObjectivePtrs.Length);
		}

		//Computes the gradient
		public double[] ComputeGradient(double[] configuration, double resolution) {
			double oldLoss = ComputeLoss(configuration);
			for(int j=0; j<DoF; j++) {
				Configuration[j] += resolution;
				MotionPtrs[j].Node.SimulateModification(Configuration);
				Configuration[j] -= resolution;
				double newLoss = 0.0;
				for(int i=0; i<ObjectivePtrs.Length; i++) {
					newLoss += SimulatedLosses[i];
				}
				newLoss = System.Math.Sqrt(newLoss / (double)ObjectivePtrs.Length);
				Gradient[j] = (newLoss - oldLoss) / resolution;
			}
			return Gradient;
		}

		//Returns whether the model converges for a particular configuration
		public bool CheckConvergence(double[] configuration) {
			FK(configuration);
			for(int i=0; i<ObjectivePtrs.Length; i++) {
				Model.Node node = ObjectivePtrs[i].Node;
				if(!ObjectivePtrs[i].Objective.CheckConvergence(node.WPX, node.WPY, node.WPZ, node.WRX, node.WRY, node.WRZ, node.WRW, node, configuration)) {
					return false;
				}
			}
			return true;
		}

		//Applies a forward kinematics pass to the model
		public virtual void FK(double[] configuration) {
			for(int i=0; i<Configuration.Length; i++) {
				Configuration[i] = configuration[i];
			}
			Nodes[0].FeedForwardConfiguration(configuration);
		}

        //Adds a segment node into the model
        protected virtual void AddNode(BioSegment segment) {
			if(FindNode(segment.Transform) == null) {
				Node node = new Node(this, FindNode(segment.Transform.parent), segment);

				if(node.Joint != null) {
					if(node.Joint.GetDoF() == 0 || !node.Joint.enabled) {
						node.Joint = null;
					} else {
						if(node.Joint.X.IsEnabled()) {
							MotionPtr motionPtr = new MotionPtr(node.Joint.X, node, MotionPtrs.Length);
							System.Array.Resize(ref MotionPtrs, MotionPtrs.Length+1);
							MotionPtrs[MotionPtrs.Length-1] = motionPtr;
							node.XEnabled = true;
							node.XIndex = motionPtr.Index;
						}
						if(node.Joint.Y.IsEnabled()) {
							MotionPtr motionPtr = new MotionPtr(node.Joint.Y, node, MotionPtrs.Length);
							System.Array.Resize(ref MotionPtrs, MotionPtrs.Length+1);
							MotionPtrs[MotionPtrs.Length-1] = motionPtr;
							node.YEnabled = true;
							node.YIndex = motionPtr.Index;
						}
						if(node.Joint.Z.IsEnabled()) {
							MotionPtr motionPtr = new MotionPtr(node.Joint.Z, node, MotionPtrs.Length);
							System.Array.Resize(ref MotionPtrs, MotionPtrs.Length+1);
							MotionPtrs[MotionPtrs.Length-1] = motionPtr;
							node.ZEnabled = true;
							node.ZIndex = motionPtr.Index;
						}
					}
				}

				BioObjective[] objectives = segment.Objectives;
				for(int i=0; i<objectives.Length; i++) {
					if(objectives[i].enabled) {
						System.Array.Resize(ref ObjectivePtrs, ObjectivePtrs.Length+1);
						ObjectivePtrs[ObjectivePtrs.Length-1] = new ObjectivePtr(objectives[i], node, ObjectivePtrs.Length);
					}
				}

				System.Array.Resize(ref Nodes, Nodes.Length+1);
				Nodes[Nodes.Length-1] = node;
			}
		}

        //Returns all objectives which are childs in the hierarcy, beginning from the root
        protected BioObjective[] CollectObjectives(BioSegment segment, List<BioObjective> objectives) {
			for(int i=0; i<segment.Objectives.Length; i++) {
				if(segment.Objectives[i].enabled) {
					objectives.Add(segment.Objectives[i]);
				}
			}
			for(int i=0; i<segment.Childs.Length; i++) {
				CollectObjectives(segment.Childs[i], objectives);
			}
			return objectives.ToArray();
		}

		//Returns a node in the model
		public Node FindNode(Transform t) {
			for(int i=0; i<Nodes.Length; i++) {
				if(Nodes[i].Transform == t) {
					return Nodes[i];
				}
			}
			return null;
		}

		//Returns the pointer to the motion
		public MotionPtr FindMotionPtr(BioJoint.Motion motion) {
			for(int i=0; i<MotionPtrs.Length; i++) {
				if(MotionPtrs[i].Motion == motion) {
					return MotionPtrs[i];
				}
			}
			return null;
		}

		//Returns the pointer to the objective
		public ObjectivePtr FindObjectivePtr(BioObjective objective) {
			for(int i=0; i<ObjectivePtrs.Length; i++) {
				if(ObjectivePtrs[i].Objective == objective) {
					return ObjectivePtrs[i];
				}
			}
			return null;
		}

		//Subclass representing the single nodes for the OFKT data structure.
		//Values are stored using primitive data types for faster access and efficient computation.
		public class Node {
			public Model Model;							//Reference to the kinematic model
			public Node Parent;							//Reference to the parent of this node
			public Node[] Childs = new Node[0];			//Reference to all child nodes
			public Transform Transform;					//Reference to the transform
			public BioJoint Joint;						//Reference to the joint
			public Transform[] Chain;

			public double WPX, WPY, WPZ;				//World position
			public double WRX, WRY, WRZ, WRW;			//World rotation
			public double WSX, WSY, WSZ;				//World scale
			public double LPX, LPY, LPZ;				//Local position
			public double LRX, LRY, LRZ, LRW;			//Local rotation
			//public double RootX, RootY, RootZ;		//World position of root joint

			public bool XEnabled = false;
			public bool YEnabled = false;
			public bool ZEnabled = false;
			public int XIndex = -1;
			public int YIndex = -1;
			public int ZIndex = -1;
			public double XValue = 0.0;					//
			public double YValue = 0.0;					//
			public double ZValue = 0.0;					//
		
			public bool[] ObjectiveImpacts;				//Boolean values to represent which objective indices in the whole kinematic tree are affected (TODO: Refactor this)

			//Setup for the node
			public Node(Model model, Node parent, BioSegment segment) {
				Model = model;
				Parent = parent;
				if(Parent != null) {
					Parent.AddChild(this);
				}
				Transform = segment.Transform;
				Joint = segment.Joint;

				List<Transform> reverseChain = new List<Transform>();
				reverseChain.Add(Transform);
				Node p = parent;
				while(p != null) {
					reverseChain.Add(p.Transform);
					p = p.Parent;
				}
				reverseChain.Reverse();
				Chain = reverseChain.ToArray();
			}

			//Adds a child to this node
			public void AddChild(Node child) {
				System.Array.Resize(ref Childs, Childs.Length+1);
				Childs[Childs.Length-1] = child;
			}

			//Recursively refreshes the current transform data
			public void Refresh() {
				//Local
				if(Joint == null) {
					Vector3 lp = Transform.localPosition;
					Quaternion lr = Transform.localRotation;
					LPX = lp.x;
					LPY = lp.y;
					LPZ = lp.z;
					LRX = lr.x;
					LRY = lr.y;
					LRZ = lr.z;
					LRW = lr.w;
				} else {
					XValue = Joint.X.GetTargetValue(true);
					YValue = Joint.Y.GetTargetValue(true);
					ZValue = Joint.Z.GetTargetValue(true);
					Joint.ComputeLocalTransformation(XValue, YValue, ZValue, out LPX, out LPY, out LPZ, out LRX, out LRY, out LRZ, out LRW);
				}
				Vector3 ws = Transform.lossyScale;
				WSX = ws.x;
				WSY = ws.y;
				WSZ = ws.z;

				//World
				ComputeWorldTransformation();

				//Feed Forward
				foreach(Node child in Childs) {
					child.Refresh();
				}
			}

			//Updates local and world transform, and feeds the joint variable configuration forward to all childs
			public void FeedForwardConfiguration(double[] configuration, bool updateWorld = false) {
				//Assume no local update is required
				bool updateLocal = false;

				if(XEnabled && configuration[XIndex] != XValue) {
					XValue = configuration[XIndex];
					updateLocal = true;
				}
				if(YEnabled && configuration[YIndex] != YValue) {
					YValue = configuration[YIndex];
					updateLocal = true;
				}
				if(ZEnabled && configuration[ZIndex] != ZValue) {
					ZValue = configuration[ZIndex];
					updateLocal = true;
				}
				
				//Only update local transformation if a joint value has changed
				if(updateLocal) {
					Joint.ComputeLocalTransformation(XValue, YValue, ZValue, out LPX, out LPY, out LPZ, out LRX, out LRY, out LRZ, out LRW);
					updateWorld = true;
				}

				//Only update world transformation if local transformation (in this or parent node) has changed
				if(updateWorld) {
					ComputeWorldTransformation();
				}

				//Feed forward the joint variable configuration
				foreach(Node child in Childs) {
					child.FeedForwardConfiguration(configuration, updateWorld);
				}
			}

			//Simulates a single transform modification while leaving the whole data structure unchanged
			//Returns the resulting Cartesian posture transformations in the out values
			public void SimulateModification(
				double[] configuration
			) {
				double[] px=Model.PX; double[] py=Model.PY; double[] pz=Model.PZ;
				double[] rx=Model.RX; double[] ry=Model.RY; double[] rz=Model.RZ; double[] rw=Model.RW;
				for(int i=0; i<Model.ObjectivePtrs.Length; i++) {
					Node node = Model.ObjectivePtrs[i].Node;
					if(ObjectiveImpacts[i]) {
						//WorldPosition = ParentPosition + ParentRotation * (LocalPosition . ParentScale) + ParentRotation * LocalRotation * WorldRotation^-1 * (ObjectivePosition - WorldPosition)
						//WorldRotation = ParentRotation * LocalRotation * WorldRotation^-1 * ObjectiveRotation
						double lpX, lpY, lpZ, lrX, lrY, lrZ, lrW;
						Joint.ComputeLocalTransformation(
							XEnabled ? configuration[XIndex] : XValue,
							YEnabled ? configuration[YIndex] : YValue, 
							ZEnabled ? configuration[ZIndex] : ZValue, 
							out lpX, out lpY, out lpZ, out lrX, out lrY, out lrZ, out lrW
						);
						double Rx, Ry, Rz, Rw, X, Y, Z;
						if(Parent == null) {
							px[i] = Model.OPX;
							py[i] = Model.OPY;
							pz[i] = Model.OPZ;
							Rx = Model.ORX;
							Ry = Model.ORY;
							Rz = Model.ORZ;
							Rw = Model.ORW;
							X = Model.OSX*lpX;
							Y = Model.OSY*lpY;
							Z = Model.OSZ*lpZ;
						} else {
							px[i] = Parent.WPX;
							py[i] = Parent.WPY;
							pz[i] = Parent.WPZ;
							Rx = Parent.WRX;
							Ry = Parent.WRY;
							Rz = Parent.WRZ;
							Rw = Parent.WRW;
							X = Parent.WSX*lpX;
							Y = Parent.WSY*lpY;
							Z = Parent.WSZ*lpZ;
						}
						double qx = Rx * lrW + Ry * lrZ - Rz * lrY + Rw * lrX;
						double qy = -Rx * lrZ + Ry * lrW + Rz * lrX + Rw * lrY;
						double qz = Rx * lrY - Ry * lrX + Rz * lrW + Rw * lrZ;
						double qw = -Rx * lrX - Ry * lrY - Rz * lrZ + Rw * lrW;
						double dot = WRX*WRX + WRY*WRY + WRZ*WRZ + WRW*WRW;
						double x = qx/dot; double y = qy/dot; double z = qz/dot; double w = qw/dot;
						qx = x * WRW + y * -WRZ - z * -WRY + w * -WRX;
						qy = -x * -WRZ + y * WRW + z * -WRX + w * -WRY;
						qz = x * -WRY - y * -WRX + z * WRW + w * -WRZ;
						qw = -x * -WRX - y * -WRY - z * -WRZ + w * WRW;
						px[i] +=
								+ 2.0 * ((0.5 - Ry * Ry - Rz * Rz) * X + (Rx * Ry - Rw * Rz) * Y + (Rx * Rz + Rw * Ry) * Z)
								+ 2.0 * ((0.5 - qy * qy - qz * qz) * (node.WPX-WPX) + (qx * qy - qw * qz) * (node.WPY-WPY) + (qx * qz + qw * qy) * (node.WPZ-WPZ));
						py[i] += 
								+ 2.0 * ((Rx * Ry + Rw * Rz) * X + (0.5 - Rx * Rx - Rz * Rz) * Y + (Ry * Rz - Rw * Rx) * Z)
								+ 2.0 * ((qx * qy + qw * qz) * (node.WPX-WPX) + (0.5 - qx * qx - qz * qz) * (node.WPY-WPY) + (qy * qz - qw * qx) * (node.WPZ-WPZ));
						pz[i] += 
								+ 2.0 * ((Rx * Rz - Rw * Ry) * X + (Ry * Rz + Rw * Rx) * Y + (0.5 - (Rx * Rx + Ry * Ry)) * Z)
								+ 2.0 * ((qx * qz - qw * qy) * (node.WPX-WPX) + (qy * qz + qw * qx) * (node.WPY-WPY) + (0.5 - qx * qx - qy * qy) * (node.WPZ-WPZ));
						rx[i] = qx * node.WRW + qy * node.WRZ - qz * node.WRY + qw * node.WRX;
						ry[i] = -qx * node.WRZ + qy * node.WRW + qz * node.WRX + qw * node.WRY;
						rz[i] = qx * node.WRY - qy * node.WRX + qz * node.WRW + qw * node.WRZ;
						rw[i] = -qx * node.WRX - qy * node.WRY - qz * node.WRZ + qw * node.WRW;
						Model.SimulatedLosses[i] = Model.ObjectivePtrs[i].Objective.ComputeLoss(px[i], py[i], pz[i], rx[i], ry[i], rz[i], rw[i], node, configuration);
					} else {
						px[i] = node.WPX;
						py[i] = node.WPY;
						pz[i] = node.WPZ;
						rx[i] = node.WRX;
						ry[i] = node.WRY;
						rz[i] = node.WRZ;
						rw[i] = node.WRW;
						Model.SimulatedLosses[i] = Model.Losses[i];
					}
				}
			}

			//Computes the world transformation using the current joint variable configuration
			private void ComputeWorldTransformation() {
				//WorldPosition = ParentPosition + ParentRotation*LocalPosition;
				//WorldRotation = ParentRotation*LocalRotation;
				double RX,RY,RZ,RW,X,Y,Z;
				if(Parent == null) {
					WPX = Model.OPX;
					WPY = Model.OPY;
					WPZ = Model.OPZ;
					RX = Model.ORX;
					RY = Model.ORY;
					RZ = Model.ORZ;
					RW = Model.ORW;
					X = Model.OSX*LPX;
					Y = Model.OSY*LPY;
					Z = Model.OSZ*LPZ;
				} else {
					WPX = Parent.WPX;
					WPY = Parent.WPY;
					WPZ = Parent.WPZ;
					RX = Parent.WRX;
					RY = Parent.WRY;
					RZ = Parent.WRZ;
					RW = Parent.WRW;
					X = Parent.WSX*LPX;
					Y = Parent.WSY*LPY;
					Z = Parent.WSZ*LPZ;
				}
				WPX += 2.0 * ((0.5 - RY * RY - RZ * RZ) * X + (RX * RY - RW * RZ) * Y + (RX * RZ + RW * RY) * Z);
				WPY += 2.0 * ((RX * RY + RW * RZ) * X + (0.5 - RX * RX - RZ * RZ) * Y + (RY * RZ - RW * RX) * Z);
				WPZ += 2.0 * ((RX * RZ - RW * RY) * X + (RY * RZ + RW * RX) * Y + (0.5 - RX * RX - RY * RY) * Z);
				WRX = RX * LRW + RY * LRZ - RZ * LRY + RW * LRX;
				WRY = -RX * LRZ + RY * LRW + RZ * LRX + RW * LRY;
				WRZ = RX * LRY - RY * LRX + RZ * LRW + RW * LRZ;
				WRW = -RX * LRX - RY * LRY - RZ * LRZ + RW * LRW;
			}
		}

		//Data class to store pointers to the objectives
		public class ObjectivePtr {
			public BioObjective Objective;
			public Node Node;
			public int Index;
			public ObjectivePtr(BioObjective objective, Node node, int index) {
				Objective = objective;
				Node = node;
				Index = index;
			}
		}

		//Data class to store pointers to the joint motions
		public class MotionPtr {
			public BioJoint.Motion Motion;
			public Node Node;
			public int Index;
			public MotionPtr(BioJoint.Motion motion, Node node, int index) {
				Motion = motion;
				Node = node;
				Index = index;
			}
		}
	}
}
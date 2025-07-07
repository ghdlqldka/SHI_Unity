using UnityEngine;
using System.Collections.Generic;

namespace BioIK 
{
	public class _Model : Model
    {
        private static string LOG_FORMAT = "<color=#D0F804><b>[_Model]</b></color> {0}";

        // protected BioIK Character;
        protected _BioIK _character
        {
            get 
            { 
                return Character as _BioIK;
            }
        }

        // protected BioSegment Root;
        protected _BioSegment _root
        {
            get
            {
                return Root as _BioSegment;
            }
        }

        public _Model(/*_BioIK character*/)/* : base()*/
        {
#if false //
            Character = character;

            //Set Root
            Root = _character.FindSegment(_character.transform);

            //Create Root
            AddNode(_root);

            //Build Model
            BioObjective[] objectives = CollectObjectives(_root, new List<BioObjective>());
            for (int i = 0; i < objectives.Length; i++)
            {
                List<BioSegment> chain = _character.GetChain(_root, objectives[i].Segment);
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

        public _Model(_BioIK character) : base()
		{
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!!!!!!");

			Character = character;

			//Set Root
			Root = _character.FindSegment(_character.transform);

			//Create Root
			AddNode(_root);
			
			//Build Model
			BioObjective[] objectives = CollectObjectives(_root, new List<BioObjective>());
			for(int i=0; i<objectives.Length; i++) 
            {
				List<BioSegment> chain = _character.GetChain(_root, objectives[i].Segment);
				for(int j=1; j<chain.Count; j++) 
                {
					AddNode(chain[j]);
				}
			}

			//Assign DoF
			DoF = MotionPtrs.Length;

			//Initialise arrays for single transform modifications
			for(int i=0; i<Nodes.Length; i++)
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
			for(int i=0; i<ObjectivePtrs.Length; i++) 
            {
				Node node = ObjectivePtrs[i].Node;
				while(node != null) 
                {
					node.ObjectiveImpacts[i] = true;
					node = node.Parent;
				}
			}

			Refresh();

			//DebugSetup();
		}

        public override BioIK GetCharacter()
        {
            Debug.Assert(Character is _BioIK);

            return _character;
        }

        public override void FK(double[] configuration)
        {
            for (int i = 0; i < Configuration.Length; i++)
            {
                Configuration[i] = configuration[i];
            }
            Nodes[0].FeedForwardConfiguration(configuration);
        }

        protected override void AddNode(BioSegment segment)
        {
            if (FindNode(segment.Transform) == null)
            {
                Node node = new Node(this, FindNode(segment.Transform.parent), segment);

                if (node.Joint != null)
                {
                    if (node.Joint.GetDoF() == 0 || !node.Joint.enabled)
                    {
                        node.Joint = null;
                    }
                    else
                    {
                        if (node.Joint.X.IsEnabled())
                        {
                            MotionPtr motionPtr = new MotionPtr(node.Joint.X, node, MotionPtrs.Length);
                            System.Array.Resize(ref MotionPtrs, MotionPtrs.Length + 1);
                            MotionPtrs[MotionPtrs.Length - 1] = motionPtr;
                            node.XEnabled = true;
                            node.XIndex = motionPtr.Index;
                        }
                        if (node.Joint.Y.IsEnabled())
                        {
                            MotionPtr motionPtr = new MotionPtr(node.Joint.Y, node, MotionPtrs.Length);
                            System.Array.Resize(ref MotionPtrs, MotionPtrs.Length + 1);
                            MotionPtrs[MotionPtrs.Length - 1] = motionPtr;
                            node.YEnabled = true;
                            node.YIndex = motionPtr.Index;
                        }
                        if (node.Joint.Z.IsEnabled())
                        {
                            MotionPtr motionPtr = new MotionPtr(node.Joint.Z, node, MotionPtrs.Length);
                            System.Array.Resize(ref MotionPtrs, MotionPtrs.Length + 1);
                            MotionPtrs[MotionPtrs.Length - 1] = motionPtr;
                            node.ZEnabled = true;
                            node.ZIndex = motionPtr.Index;
                        }
                    }
                }

                BioObjective[] objectives = segment.Objectives;
                for (int i = 0; i < objectives.Length; i++)
                {
                    if (objectives[i].enabled)
                    {
                        System.Array.Resize(ref ObjectivePtrs, ObjectivePtrs.Length + 1);
                        ObjectivePtrs[ObjectivePtrs.Length - 1] = new ObjectivePtr(objectives[i], node, ObjectivePtrs.Length);
                    }
                }

                System.Array.Resize(ref Nodes, Nodes.Length + 1);
                Nodes[Nodes.Length - 1] = node;
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using BioIK;


namespace _SHI_BA
{
	public class BA_Model : BioIK._Model
    {
        protected new BA_BioIK _character
        {
            get
            {
                return Character as BA_BioIK;
            }
        }

        // protected BioSegment Root;
        protected new BA_BioSegment _root
        {
            get
            {
                return Root as BA_BioSegment;
            }
        }

        public BA_Model(BA_BioIK character) : base()
		{
            // Debug.LogError("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@=> ");
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

        }

    }
}
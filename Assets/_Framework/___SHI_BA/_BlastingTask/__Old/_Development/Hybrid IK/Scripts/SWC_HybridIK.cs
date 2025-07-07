using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AshqarApps.DynamicJoint;

namespace _SHI_BA
{
    public class SWC_HybridIK : _Magenta_WebGL.MWGL_HybridIK
    {
        private static string LOG_FORMAT = "<color=#F85AA7><b>[SWC_HybridIK]</b></color> {0}";

        protected override void Awake()
        {
            nodes = null; // forcelly set!!!!!

            Debug.LogFormat(LOG_FORMAT, "Awake(), rootNode : <b>" + rootNode + "</b>, endNode : <b>" + endNode.name + "</b>");
#if DEBUG
            _debug = this.GetComponent<_DEBUG_SWC_HybridIK>();
            Debug.Assert(_debug != null);
#endif
            Debug.Assert(endNode != null);
        }

        public override void ProcessChain()
        {
            Debug.LogFormat(LOG_FORMAT, "ProcessChain()");

            if (endNode == null)
            {
                Debug.Assert(false);
                return;
            }

            if (nodes != null && nodes.Count > 0)
            {
                Debug.LogFormat(LOG_FORMAT, "1");

                ReprocessJoints();
                return;
            }

            Debug.LogFormat(LOG_FORMAT, "2");
            constraints = new List<HybridIKConstraint>();

            SWC_HybridIKJoint child = null;
            nodes = new List<HybridIKJoint>();

            Transform transformNode = endNode;

            while (transformNode != null)
            {
                SWC_HybridIKJoint ikNode = new SWC_HybridIKJoint(transformNode, child, nodeRadius);

                child = ikNode;

                //ikNode.constraint = null;
                if (constraints != null && constraints.Count > 0)
                {
                    HybridIKConstraint constraint = constraints.Find(c => c.jointTransform == ikNode.jointTransform);
                    if (constraint != null)
                    {
                        ikNode.constraint = constraint;
                    }
                }

                nodes.Add(ikNode);

                if (transformNode == rootNode)
                {
                    break;
                }

                transformNode = transformNode.parent;
                while (transformNode.tag.Contains("ExcludeFromIK"))
                {
                    transformNode = transformNode.parent;
                    if (transformNode == null || transformNode == rootNode)
                    {
                        break;
                    }
                }
            } // end-of-while (transformNode != null)

            nodes.Reverse();

            Debug.LogWarningFormat(LOG_FORMAT, "nodes.Count : <b>" + nodes.Count + "</b>");

            for (int i = 0; i < nodes.Count; ++i)
            {
                HybridIKJoint n = nodes[i];

                n.zeroRotation = n.jointTransform.localRotation;
                n.zeroPosition = n.jointTransform.localPosition;

                if (i < nodes.Count - 1)
                {
                    n.child = nodes[i + 1];
                    n.childIndex = i + 1;
                    n.IsEndNode = false;
                }
                else
                {
                    n.child = null;
                    n.childIndex = -1;
                    n.IsEndNode = true;
                }
                if (i > 0)
                {
                    n.parent = nodes[i - 1];
                    n.parentIndex = i - 1;
                    n.boneLength = (n.jointTransform.position - nodes[i - 1].jointTransform.position).magnitude;
                }
                else
                {
                    n.parent = null;
                    n.parentIndex = -1;
                }
            }


            // isInitialized = true;
        }
    }
}
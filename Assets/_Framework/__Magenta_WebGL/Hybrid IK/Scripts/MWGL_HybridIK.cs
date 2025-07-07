using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_HybridIK : _HybridIK
    {
        private static string LOG_FORMAT = "<color=#F85AA7><b>[MWGL_HybridIK]</b></color> {0}";

        protected override void Awake()
        {
            nodes = null; // forcelly set!!!!!

            Debug.LogFormat(LOG_FORMAT, "Awake(), rootNode : <b>" + rootNode + "</b>, endNode : <b>" + endNode.name + "</b>");
#if DEBUG
            _debug = this.GetComponent<_DEBUG_MWGL_HybridIK>();
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

            MWGL_HybridIKJoint child = null;
            nodes = new List<HybridIKJoint>();
#if DEBUG
            if (_debug == null)
            {
                _debug = this.GetComponent<_DEBUG_MWGL_HybridIK>();
            }
            _debug.DEBUG_nodeList = new List<_HybridIKJoint>();
#endif
            Transform transformNode = endNode;

            while (transformNode != null)
            {
                MWGL_HybridIKJoint ikNode = new MWGL_HybridIKJoint(transformNode, child, nodeRadius);

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
#if DEBUG
                _debug.DEBUG_nodeList.Add(ikNode);
#endif
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
#if DEBUG
            if (_debug == null)
            {
                _debug = this.GetComponent<_DEBUG_HybridIK>();
            }
            _debug.DEBUG_nodeList.Reverse();
#endif
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
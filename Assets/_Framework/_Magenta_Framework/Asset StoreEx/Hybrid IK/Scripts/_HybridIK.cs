using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AshqarApps.DynamicJoint;

public class _HybridIK : HybridInverseKinematicsNode
{
    private static string LOG_FORMAT = "<color=#F85AA7><b>[_HybridIK]</b></color> {0}";

#if DEBUG
    protected _DEBUG_HybridIK _debug;
#endif

    // public List<HybridIKJoint> nodes;
    public List<HybridIKJoint> nodeList
    {
        get
        {
            return nodes;
        }
        private set
        {
            nodes = value;
        }
    }

    protected virtual void Awake()
    {
        nodeList = null; // forcelly set!!!!!

        Debug.LogFormat(LOG_FORMAT, "Awake(), rootNode : <b><color=yellow>" + rootNode.name + 
            "</color></b>, endNode : <b><color=cyan>" + endNode.name + "</color></b>");
#if DEBUG
        _debug = this.GetComponent<_DEBUG_HybridIK>();
        Debug.Assert(_debug != null);
        _debug.DEBUG_rootNode = rootNode;
        _debug.DEBUG_endNode = endNode;

        rootNode.name = rootNode.name + "(Root Node)";
        endNode.name = endNode.name + "(End Node)";

        this.gameObject.name = this.gameObject.name + "(" + mode + ")";
#endif
        Debug.Assert(endNode != null);
    }

    protected override void Start()
    {
        Debug.LogFormat(LOG_FORMAT, "Start()");

        ProcessChain();

        foreach (HybridIKJoint n in nodeList)
        {
            _HybridIKJoint _n = n as _HybridIKJoint;

            _n.lastFrameRotation = _n.jointTransform.localRotation;
            _n.lastFramePosition = _n.jointTransform.localPosition;
        }
    }

    protected override void LateUpdate()
    {
        if (IsInitialized() == false)
        {
            return;
        }

        if (enableKeyframeConstraints == true)
        {
            UpdateKeyframeConstraintKeys();
        }

        targetPosition = targetTransform.position;

        _HybridIKJoint rootIKNode = GetRootIKNode() as _HybridIKJoint;
        _HybridIKJoint endIKNode = GetEndIKNode() as _HybridIKJoint;

        rootIKNode.targetPosition = rootIKNode.jointTransform.position;

        // ReInitialization
        ReinitializeTransforms();
        ReinitializeStretch();

        // IK Steps
        FABRIK_Step(numIterations, true, jointLimitStrength);
        CCD_Step(numIterations, true);
        StretchStep();

        SnapParticlesToActualJointPositions();

        bool cachedEnableKeyframeConstraints = enableKeyframeConstraints;

        // Apply second pass with no constraints, focusing on end target only
        if (enableKeyframeConstraints == false || strictlyProjectToKeyframeSpace == false)
        {
            enableKeyframeConstraints = false;
            FABRIK_Step(numIterations, false, jointLimitStrength);
            CCD_Step(numIterations, false);
        }

        // With soft limits, apply a final pass to achieve target with joint limits disabled for the delta
        if (useStrictLimits == false)
        {
            SnapParticlesToActualJointPositions();
            FABRIK_Step();
        }

        SnapParticlesToActualJointPositions();

        if (useAngularMotionMotorConstraints == true)
        {
            ApplyAngularMotionConstraints();
        }

        if (useStretchMotionMotorConstraints == true)
        {
            ApplyStretchMotionConstraints();
        }

        foreach (HybridIKJoint n in nodeList)
        {
            _HybridIKJoint _n = n as _HybridIKJoint;
            _n.lastFrameRotation = _n.jointTransform.localRotation;
            _n.lastFramePosition = _n.jointTransform.localPosition;
        }

        enableKeyframeConstraints = cachedEnableKeyframeConstraints;
    }

    public override void ApplyJointLimits(HybridIKJoint parentNode, float jointLimitStrength = 1)
    {
        if (parentNode.jointLimit == null)
        {
            return;
        }

        if (parentNode.jointLimit is _DynamicJointLimitHinge)
        {
            ((_DynamicJointLimitHinge)parentNode.jointLimit).Apply(jointLimitStrength);
        }
        else if (parentNode.jointLimit is _DynamicJointLimitSwingTwist)
        {
            ((_DynamicJointLimitSwingTwist)parentNode.jointLimit).Apply(jointLimitStrength);
        }
        else
        {
            Debug.Assert(false);
        }
    }
    
    public override void ResetAll()
    {
        Debug.LogFormat(LOG_FORMAT, "ResetAll()");

        if (nodeList != null)
        {
            nodeList.Clear();
            nodeList = null;
        }
    }

    public override void ReprocessJoints()
    {
        Debug.LogFormat(LOG_FORMAT, "ReprocessJoints()");

        _HybridIKJoint endIKNode = GetEndIKNode() as _HybridIKJoint;
        _HybridIKJoint rootIKNode = GetRootIKNode() as _HybridIKJoint;
        Debug.Assert(rootIKNode != null);
        Transform lastIKSpace = rootIKNode.jointTransform.parent;
        Transform endIKSpace = rootIKNode.jointTransform.parent;

        // find endNodeSpace
        foreach (HybridIKJoint n in nodeList)
        {
            _HybridIKJoint _n = n as _HybridIKJoint;
            if (_n != endIKNode && !_n.enableKeyframeConstraints)
            {
                endIKSpace = _n.jointTransform;
            }

            _n.cachedRotation = _n.jointTransform.localRotation;
            _n.cachedPosition = _n.jointTransform.localPosition;
            _n.jointTransform.localRotation = _n.initialRotation;
            _n.jointTransform.localPosition = _n.initialPosition;
        }

        //already pre-processed - so cache child/parent nodes and return
        foreach (HybridIKJoint n in nodeList)
        {
            _HybridIKJoint _n = n as _HybridIKJoint;

            if (_n.parentIndex > -1)
                _n.parent = nodeList[_n.parentIndex];
            if (_n.childIndex > -1)
                _n.child = nodeList[_n.childIndex];

            _n.jointLimit = _n.jointTransform.GetComponent<DynamicJointLimit>();

            // Re-cache constraints
            if (_n.overrideConstraint == false && constraints != null && constraints.Count > 0)
            {
                HybridIKConstraint constraint = constraints.Find(c => c.jointTransform == _n.jointTransform);
                constraint.constrainRotation = true;
                if (constraint != null)
                {
                    _n.constraint = constraint;
                }

                // change keyframe relative to target
                for (int i = 0; i < _n.constraint.positionKeys.Count; ++i)
                {
                    ConstraintPositionKey k = _n.constraint.positionKeys[i];

                    // change keyedPos frame
                    /*
                    Vector3 wPos = n.targetIKSpace == null ? rootIKNode.jointTransform.parent.TransformPoint(n.keyedPositions[i])
                        : n.targetIKSpace.TransformPoint(n.keyedPositions[i]);
                    */
                    Vector3 wPos;
                    if (_n.targetIKSpace == null)
                    {
                        wPos = rootIKNode.jointTransform.parent.TransformPoint(_n.keyedPositions[i]);
                    }
                    else
                    {
                        wPos = _n.targetIKSpace.TransformPoint(_n.keyedPositions[i]);
                    }
                    _n.keyedPositions[i] = lastIKSpace.InverseTransformPoint(wPos);
                    wPos = k.GetEndTargetPosition(k.targetSpace ?? rootIKNode.jointTransform.parent);
                    k.SetEndTargetPosition(wPos, endIKSpace);
                }

                _n.targetIKSpace = lastIKSpace;
                if (_n.enableKeyframeConstraints == false)
                {
                    lastIKSpace = _n.jointTransform;
                }

                if (_n == endIKNode)
                {
                    _n.enableKeyframeConstraints = false;
                    _n.overrideConstraint = false;
                }
            }

            if (_n.overrideConstraint && _n.constraint != null && _n.constraint.positionKeys != null)
            {
                _n.constraint.positionKeys.Clear();
            }

            _n.jointLimit = _n.jointTransform.GetComponent<DynamicJointLimit>();
        }

        foreach (HybridIKJoint n in nodeList)
        {
            n.jointTransform.localRotation = n.cachedRotation;
            n.jointTransform.localPosition = n.cachedPosition;
        }
    }

    public override void ProcessChain()
    {
        Debug.LogFormat(LOG_FORMAT, "ProcessChain()");

        if (endNode == null)
        {
            Debug.Assert(false);
            return;
        }

        if (nodeList != null && nodeList.Count > 0)
        {
            Debug.LogFormat(LOG_FORMAT, "1");

            ReprocessJoints();
            return;
        }

        Debug.LogFormat(LOG_FORMAT, "2");
        constraints = new List<HybridIKConstraint>();

        _HybridIKJoint child = null;
        nodeList = new List<HybridIKJoint>();
#if DEBUG
        if (_debug == null)
        {
            _debug = this.GetComponent<_DEBUG_HybridIK>();
        }
        _debug.DEBUG_nodeList = new List<_HybridIKJoint>();
#endif
        Transform transformNode = endNode;

        while (transformNode != null)
        {
            _HybridIKJoint ikNode = new _HybridIKJoint(transformNode, child, nodeRadius);

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

            nodeList.Add(ikNode);
#if DEBUG
            _debug.DEBUG_nodeList.Add(ikNode);
#endif
            if (transformNode == rootNode)
            {
                break;
            }

            transformNode = transformNode.parent;
            while (transformNode.tag.Contains("ExcludeFromIK") == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
                transformNode = transformNode.parent;
                if (transformNode == null || transformNode == rootNode)
                {
                    break;
                }
            }
        } // end-of-while (transformNode != null)

        nodeList.Reverse();
#if DEBUG
        if (_debug == null)
        {
            _debug = this.GetComponent<_DEBUG_HybridIK>();
        }
        _debug.DEBUG_nodeList.Reverse();
#endif
        Debug.LogWarningFormat(LOG_FORMAT, "nodeList.Count : <b>" + nodeList.Count + "</b>");

        for (int i = 0; i < nodeList.Count; ++i)
        {
            _HybridIKJoint _node = nodeList[i] as _HybridIKJoint;

            _node.zeroRotation = _node.jointTransform.localRotation;
            _node.zeroPosition = _node.jointTransform.localPosition;

            if (i < nodeList.Count - 1)
            {
                _node.child = nodeList[i + 1];
                _node.childIndex = i + 1;
                _node.IsEndNode = false;
            }
            else
            {
                _node.child = null;
                _node.childIndex = -1;
                _node.IsEndNode = true;
            }

            if (i > 0)
            {
                _node.parent = nodeList[i - 1];
                _node.parentIndex = i - 1;
                _node.boneLength = (_node.jointTransform.position - nodeList[i - 1].jointTransform.position).magnitude;
            }
            else
            {
                _node.parent = null;
                _node.parentIndex = -1;
            }
        }

        // isInitialized = true;
    }

    public override void CCD_ForwardStep(bool useConstraints)
    {
        // Debug.LogFormat(LOG_FORMAT, "CCD_ForwardStep(), useConstraints : " + useConstraints);

        _HybridIKJoint rootIKNode = nodeList[0] as _HybridIKJoint;
        _HybridIKJoint endIKNode = nodeList[nodeList.Count - 1] as _HybridIKJoint;

        endIKNode.isPositionConstrained = true;
        endIKNode.targetPosition = targetTransform.position;

        List<HybridIKJoint> ikTargetNodes = nodeList.FindAll(n => (useConstraints && HasPositionConstraint(n)) || n.IsEndNode);

        foreach (HybridIKJoint _targetIKNode in ikTargetNodes)
        {
            _HybridIKJoint targetIKNode = _targetIKNode as _HybridIKJoint;
            Vector3 targetPosition = targetTransform.position;
            Vector3 targetLocalPosition = targetIKNode.targetLocalPosition;
            if (HasPositionConstraint(targetIKNode))
            {
                targetPosition = targetIKNode.constraint.GetPosition();
                targetLocalPosition = targetIKNode.constraint.GetLocalPosition();
            }
            bool pastTarget = false;

            for (int i = 0; i < nodeList.Count; i++)
            {
                bool solvingEndNode = targetIKNode.IsEndNode || pastTarget;
                if (useConstraints == false)
                    solvingEndNode = true;
                // Vector3 originalPosition = solvingEndNode ? endIKNode.GetCurrentPositionWorld() : targetIKNode.GetCurrentPositionWorld();
                Vector3 originalPosition;
                if (solvingEndNode == true)
                {
                    originalPosition = endIKNode.GetCurrentPositionWorld();
                }
                else
                {
                    originalPosition = targetIKNode.GetCurrentPositionWorld();
                }

                _HybridIKJoint node = nodeList[i] as _HybridIKJoint;

                if (node.parent != null)
                {
                    SwingAlignToParticle(node.parent, originalPosition, targetPosition, jointLimitStrength, true, solvingEndNode);
                }

                if (targetIKNode.IsEndNode == false && node == targetIKNode)
                {
                    targetPosition = targetTransform.position;
                    pastTarget = true;
                }
            }
        }
    }

    public override void CCD_BackwardStep(bool useConstraints)
    {
        // Debug.LogFormat(LOG_FORMAT, "CCD_BackwardStep(), useConstraints : " + useConstraints);

        _HybridIKJoint rootIKNode = nodeList[0] as _HybridIKJoint;
        _HybridIKJoint endIKNode = nodeList[nodeList.Count - 1] as _HybridIKJoint;

        endIKNode.isPositionConstrained = true;
        endIKNode.targetPosition = targetTransform.position;

        List<HybridIKJoint> ikTargetNodes = nodeList.FindAll(n => (useConstraints && HasPositionConstraint(n)) || n.IsEndNode);

        foreach (HybridIKJoint _targetIKNode in ikTargetNodes)
        {
            _HybridIKJoint targetIKNode = _targetIKNode as _HybridIKJoint;
            Vector3 targetPosition = targetTransform.position;
            Vector3 targetLocalPosition = targetIKNode.targetLocalPosition;

            if (HasPositionConstraint(targetIKNode))
            {
                targetPosition = targetIKNode.constraint.GetPosition();
                targetLocalPosition = targetIKNode.constraint.GetLocalPosition();
            }

            bool pastTarget = false;

            targetPosition = targetTransform.position;
            for (int i = nodeList.Count - 1; i > 0; i--)
            {
                bool solvingEndNode = !(!targetIKNode.IsEndNode && pastTarget);

                solvingEndNode &= targetIKNode.parent == null || !HasIKConstraint(targetIKNode.parent);
                if (useConstraints == false)
                    solvingEndNode = true;

                // Vector3 originalPosition = !targetIKNode.IsEndNode && pastTarget ? targetIKNode.GetCurrentPositionWorld() : endIKNode.GetCurrentPositionWorld();
                Vector3 originalPosition;
                if (!targetIKNode.IsEndNode && pastTarget)
                {
                    originalPosition = targetIKNode.GetCurrentPositionWorld();
                }
                else
                {
                    originalPosition = endIKNode.GetCurrentPositionWorld();
                }

                HybridIKJoint node = nodeList[i];

                if (node.parent != null)
                {
                    SwingAlignToParticle(node.parent, originalPosition, targetPosition, jointLimitStrength, true, solvingEndNode);
                }

                if (!targetIKNode.IsEndNode && node == targetIKNode)
                {
                    pastTarget = true;
                    targetPosition = targetIKNode.constraint.GetPosition();
                }
            }

        }
    }
}

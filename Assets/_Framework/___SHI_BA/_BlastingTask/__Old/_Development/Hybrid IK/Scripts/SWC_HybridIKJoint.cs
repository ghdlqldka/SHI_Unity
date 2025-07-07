using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AshqarApps.DynamicJoint;

namespace _SHI_BA
{
    [System.Serializable]
    public class SWC_HybridIKJoint : _Magenta_WebGL.MWGL_HybridIKJoint
    {
        private static string LOG_FORMAT = "<color=#F6C130><b>[SWC_HybridIKJoint]</b></color> {0}";

        //
        public SWC_HybridIKJoint(Transform t, SWC_HybridIKJoint child, float radius = 0.1F) : base(/*t, child, radius*/)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!!!, _transform : <b>" + t.name + "</b>");
#if DEBUG
            _DEBUG_SWC_HybridIKJoint _debug = t.gameObject.GetComponent<_DEBUG_SWC_HybridIKJoint>();
            if (_debug != null)
            {
                _debug._Destroy();
            }

            _debug = t.gameObject.AddComponent<_DEBUG_SWC_HybridIKJoint>();
            _debug.Joint = this;
#endif

            if (child != null)
            {
                child.boneLength = (t.position - child.jointTransform.position).magnitude;
                child.parent = this;
                this.child = child;
            }

            lastFrameRotation = t.localRotation;
            initialPosition = t.localPosition;
            initialRotation = t.localRotation;
            initialPositionWorld = t.position;
            targetPosition = t.position;

            previousPositionWorld = initialPositionWorld;

            jointTransform = t;

            jointRadius = radius;

            oldVec = Vector3.up;

            keyedRotations = new List<Quaternion>();
            keyedPositions = new List<Vector3>();
            keyedLocalPositions = new List<Vector3>();

            this.jointLimit = t.gameObject.GetComponent<DynamicJointLimit>();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    [System.Serializable]
    public class MWGL_HybridIKJoint : _HybridIKJoint
    {
        private static string LOG_FORMAT = "<color=#F6C130><b>[MWGL_HybridIKJoint]</b></color> {0}";

        public MWGL_HybridIKJoint()
        {
            //
        }

        public MWGL_HybridIKJoint(Transform t, MWGL_HybridIKJoint child, float radius = 0.1F) : base(/*t, child, radius*/)
        {
            Debug.LogFormat(LOG_FORMAT, "constructor!!!!!!!!, _transform : <b>" + t.name + "</b>");
#if DEBUG
            _DEBUG_MWGL_HybridIKJoint _debug = t.gameObject.GetComponent<_DEBUG_MWGL_HybridIKJoint>();
            if (_debug != null)
            {
                _debug._Destroy();
            }

            _debug = t.gameObject.AddComponent<_DEBUG_MWGL_HybridIKJoint>();
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

            this.jointLimit = t.gameObject.GetComponent<AshqarApps.DynamicJoint.DynamicJointLimit>();
        }
    }
}
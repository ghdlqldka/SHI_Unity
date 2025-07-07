using System.Collections.Generic;
using UnityEngine;

public class _DEBUG_HybridIKJoint : MonoBehaviour
{
    // private static string LOG_FORMAT = "<color=#00FF0E><b>[_DEBUG_HybridIKJoint]</b></color> {0}";

    [ReadOnly]
    [SerializeField]
    protected _HybridIKJoint _joint;
    public _HybridIKJoint Joint
    {
        set 
        { 
            _joint = value;
        }
    }

    protected virtual void Awake()
    {
        // Debug.LogFormat(LOG_FORMAT, "Awake()");
    }

    public void _Destroy()
    {
        DestroyImmediate(this);
    }
}

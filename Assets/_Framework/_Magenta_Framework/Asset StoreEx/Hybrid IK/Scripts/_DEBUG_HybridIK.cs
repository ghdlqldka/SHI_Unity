using System.Collections.Generic;
using UnityEngine;

public class _DEBUG_HybridIK : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=#00FF0E><b>[_DEBUG_HybridIK]</b></color> {0}";

    [ReadOnly]
    public Transform DEBUG_rootNode;
    [ReadOnly]
    public Transform DEBUG_endNode;

    [ReadOnly]
    public List<_HybridIKJoint> DEBUG_nodeList;

    protected virtual void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");
    }
}

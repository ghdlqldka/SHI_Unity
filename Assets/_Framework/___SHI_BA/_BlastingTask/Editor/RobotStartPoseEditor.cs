using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(RobotStartPose))]
public class RobotStartPoseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RobotStartPose script = (RobotStartPose)target;
        if (GUILayout.Button("포즈 적용 (입력값 사용)"))
        {
            script.ApplyPoseFromInspector();
        }
    }
}
#endif

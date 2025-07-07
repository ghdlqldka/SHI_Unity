using UnityEngine;
using UnityEditor;
using BioIK;

public class FKButtonTester : MonoBehaviour
{
    public BioIK.BioIK bioIKSystem;
    public BioSegment baseSegment;
    public BioSegment endEffectorSegment;
    public double[] jointValues;

    public Vector3 fkPosition;
    public Quaternion fkRotation;
    public Vector3 fkLocalPosition;
    public Quaternion fkLocalRotation;

    // 새 구조체로 로컬 포즈(위치+오일러) 저장
    public Vector3 fkLocalEulerZYX;

    public bool isAutoFK = false;

    void Update()
    {
        if (isAutoFK)
        {
            CalculateFK();
        }
    }

    public void CalculateFK()
    {
        // 월드 좌표 FK
        if (BlastingKinematics.GetWorldPoseCurrent(bioIKSystem, endEffectorSegment, out fkPosition, out fkRotation))
        {
            Vector3 euler = fkRotation.eulerAngles;
            float roll = euler.z;
            float pitch = euler.x;
            float yaw = euler.y;
            Debug.Log($"[FK-World] 엔드 이펙터 위치: {fkPosition}, 회전(오일러): Roll={roll:F2}, Pitch={pitch:F2}, Yaw={yaw:F2}");
        }
        else
        {
            Debug.LogError("FK (World) 계산 실패");
        }

        // 로컬 좌표 FK (base 기준, Pose6D 구조체 사용)
        if (baseSegment != null)
        {
            BlastingKinematics.Pose6D pose6d;
            if (BlastingKinematics.GetLocalPoseCurrent(
                bioIKSystem,
                baseSegment,
                endEffectorSegment,
                out pose6d))
            {
                fkLocalPosition = pose6d.position;
                fkLocalEulerZYX = pose6d.eulerZYX;
                float lroll = fkLocalEulerZYX.x;
                float lpitch = fkLocalEulerZYX.y;
                float lyaw = fkLocalEulerZYX.z;
                Debug.Log($"[FK-Local] base 기준 TCP 위치: {fkLocalPosition}, 회전(오일러 ZYX): Roll={lroll:F2}, Pitch={lpitch:F2}, Yaw={lyaw:F2}");
            }
            else
            {
                Debug.LogError("FK (Local) 계산 실패");
            }
        }
        else
        {
            Debug.LogWarning("baseSegment가 할당되지 않았습니다. Local FK는 생략.");
        }
    }

    [ContextMenu("모든 Segment 이름 출력")]
    public void DebugPrintAllSegments()
    {
        if (bioIKSystem == null || bioIKSystem.Segments == null)
        {
            Debug.LogWarning("bioIKSystem 또는 Segments가 null입니다.");
            return;
        }

        Debug.Log($"[FKButtonTester] bioIKSystem.Segments.Count = {bioIKSystem.Segments.Count}");
        foreach (var seg in bioIKSystem.Segments)
        {
            if (seg != null && seg.Transform != null)
                Debug.Log($"[FKButtonTester] Segment Name: {seg.name}, Transform Name: {seg.Transform.name}");
            else if (seg != null)
                Debug.Log($"[FKButtonTester] Segment Name: {seg.name}, Transform is null");
        }
    }

    [ContextMenu("모든 Joint 값(이름, 값) 디버그 출력")]
    public void DebugPrintAllJointValuesBySegment()
    {
        if (bioIKSystem == null || bioIKSystem.Segments == null)
        {
            Debug.LogWarning("bioIKSystem 또는 Segments가 null입니다.");
            return;
        }

        Debug.Log("[FKButtonTester] 모든 Joint 값(이름, 값) 디버그 출력 시작");
        foreach (var segment in bioIKSystem.Segments)
        {
            if (segment == null || segment.Joint == null) continue;

            string jointName = segment.Joint.gameObject.name;
            string msg = $"Joint GameObject Name: {jointName}";

            // X, Y, Z 축 값
            if (segment.Joint.X != null)
                msg += $" | X: {segment.Joint.X.GetCurrentValue():F4}";
            if (segment.Joint.Y != null)
                msg += $" | Y: {segment.Joint.Y.GetCurrentValue():F4}";
            if (segment.Joint.Z != null)
                msg += $" | Z: {segment.Joint.Z.GetCurrentValue():F4}";

            // upbox, A1~A6 등 커스텀 축 값 (BioJoint.Motion 타입으로 캐스팅)
            var upboxField = segment.Joint.GetType().GetField("upbox");
            if (upboxField != null)
            {
                var upbox = upboxField.GetValue(segment.Joint) as BioIK.BioJoint.Motion;
                if (upbox != null)
                    msg += $" | upbox: {upbox.GetCurrentValue():F4}";
            }
            for (int i = 1; i <= 6; i++)
            {
                var field = segment.Joint.GetType().GetField($"A{i}");
                if (field != null)
                {
                    var dof = field.GetValue(segment.Joint) as BioIK.BioJoint.Motion;
                    if (dof != null)
                        msg += $" | A{i}: {dof.GetCurrentValue():F4}";
                }
            }

            Debug.Log(msg);
        }
        Debug.Log("[FKButtonTester] 모든 Joint 값(이름, 값) 디버그 출력 끝");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FKButtonTester))]
public class FKButtonTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FKButtonTester tester = (FKButtonTester)target;
        if (GUILayout.Button(tester.isAutoFK ? "자동 FK 중지" : "자동 FK 시작"))
        {
            tester.isAutoFK = !tester.isAutoFK;
        }
        if (GUILayout.Button("한 번만 FK 계산"))
        {
            tester.CalculateFK();
        }
        if (GUILayout.Button("모든 Segment 이름 출력"))
        {
            tester.DebugPrintAllSegments();
        }
        if (GUILayout.Button("모든 Joint 값(이름, 값) 디버그 출력"))
        {
            tester.DebugPrintAllJointValuesBySegment();
        }
    }
}
#endif
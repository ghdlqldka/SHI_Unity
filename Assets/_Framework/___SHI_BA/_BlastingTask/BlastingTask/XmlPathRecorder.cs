using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [V2 수정] 모든 Pose 데이터를 하나의 리스트에 기록만 하는 단순화된 클래스.
/// [V3 수정] 중복된 Pose 데이터 저장을 방지하는 기능 추가.
/// </summary>
public class XmlPathRecorder
{
    public List<Make_XML.PoseData> AllPoses { get; private set; }
    private int currentPoseId;

    // 마지막으로 저장된 Pose를 기억하기 위한 변수
    private Make_XML.PoseData lastRecordedPose = null;

    public XmlPathRecorder()
    {
        this.AllPoses = new List<Make_XML.PoseData>();
        this.currentPoseId = 1;
    }

    /// <summary>
    /// 새로운 경로 세그먼트(면) 기록을 시작합니다. Pose ID를 1로 리셋합니다.
    /// </summary>
    public void StartNewPathSegment()
    {
        currentPoseId = 1;
        lastRecordedPose = null; // 새로운 세그먼트 시작 시 마지막 Pose 정보도 초기화
        AllPoses.Clear(); // <-- 이 라인을 추가하여 각 면 시작 시 리스트를 비웁니다!
        Debug.Log("[XmlPathRecorder] 새로운 경로 세그먼트 기록 시작.");
    }

    public void RecordCurrentPose_weaving_fixed(Make_XML makeXml, float weavingAngle, float spd, float acc, float onoff) // 파라미터 추가
    {
        if (makeXml == null) return;

        var newPose = new Make_XML.PoseData();
        if (weavingAngle > 180.0f) // 0 도 기준 +/- 로 정규화
        {
            weavingAngle -= 360.0f;
        }
        // 위빙 전용 메서드를 호출하여 Pose 데이터를 채웁니다. 추가된 파라미터 전달
        makeXml.FillPoseFromBioIK_weaving_fixed(makeXml.bioIK, makeXml.baseSegment, makeXml.tcpSegment, newPose, weavingAngle, spd, acc, onoff); // 파라미터 전달

        if (IsPoseDuplicated(newPose))
        {
            Debug.LogWarning($"<color=orange>[XML 중복 방지] 현재 Pose는 이전 데이터와 동일하여 저장하지 않습니다.</color>");
            return;
        }

        newPose.id = currentPoseId++;
        AllPoses.Add(newPose);
        lastRecordedPose = newPose;

#if UNITY_EDITOR || UNITY_STANDALONE
        var stackTrace = new System.Diagnostics.StackTrace();
        var callingMethodName = stackTrace.GetFrame(1).GetMethod().Name;
        Debug.Log($"<color=yellow>[XML 저장 - Weaving] Pose ID: {newPose.id}</color> - (호출: {callingMethodName})");
#else
        Debug.Log($"<color=yellow>[XML 저장 - Weaving] Pose ID: {newPose.id}</color>");
#endif
    }

  
    public void RecordCurrentPose_weaving(Make_XML makeXml, float weavingAngle, float spd, float acc) // 파라미터 추가
    {
        if (makeXml == null) return;

        var newPose = new Make_XML.PoseData();
        if (weavingAngle > 180.0f) // 0 도 기준 +/- 로 정규화
        {
            weavingAngle -= 360.0f;
        }
        // 위빙 전용 메서드를 호출하여 Pose 데이터를 채웁니다. 추가된 파라미터 전달
        makeXml.FillPoseFromBioIK_weaving(makeXml.bioIK, makeXml.baseSegment, makeXml.tcpSegment, newPose, weavingAngle, spd, acc); // 파라미터 전달

        if (IsPoseDuplicated(newPose))
        {
            Debug.LogWarning($"<color=orange>[XML 중복 방지] 현재 Pose는 이전 데이터와 동일하여 저장하지 않습니다.</color>");
            return;
        }

        newPose.id = currentPoseId++;
        AllPoses.Add(newPose);
        lastRecordedPose = newPose;

#if UNITY_EDITOR || UNITY_STANDALONE
        var stackTrace = new System.Diagnostics.StackTrace();
        var callingMethodName = stackTrace.GetFrame(1).GetMethod().Name;
        Debug.Log($"<color=yellow>[XML 저장 - Weaving] Pose ID: {newPose.id}</color> - (호출: {callingMethodName})");
#else
        Debug.Log($"<color=yellow>[XML 저장 - Weaving] Pose ID: {newPose.id}</color>");
#endif
    }
    /// <summary>
    /// 현재 로봇의 Pose를 캡처하여 전체 리스트에 추가합니다.
    /// 단, 마지막으로 저장된 Pose와 중복되면 저장하지 않습니다.
    /// </summary>
    public void RecordCurrentPose(Make_XML makeXml)
    {
        if (makeXml == null) return;

        var newPose = new Make_XML.PoseData(); // ID는 저장 직전에 할당
        makeXml.FillPoseFromBioIK(makeXml.bioIK, makeXml.baseSegment, makeXml.tcpSegment, newPose);

        // 마지막으로 저장된 Pose와 비교
        if (IsPoseDuplicated(newPose))
        {
            // 중복된 경우, 디버그 로그를 남기고 저장을 건너뜁니다.
            Debug.LogWarning($"<color=orange>[XML 중복 방지] 현재 Pose는 이전 데이터와 동일하여 저장하지 않습니다.</color>");
            return;
        }

        // 중복이 아닐 경우에만 ID를 부여하고 리스트에 추가합니다.
        newPose.id = currentPoseId++;
        AllPoses.Add(newPose);
        lastRecordedPose = newPose; // 마지막으로 저장된 Pose를 현재 것으로 업데이트

        // 어떤 메서드가 호출했는지 확인하는 디버깅 로그
        // WebGL 빌드 시 System.Diagnostics.StackTrace 사용을 피하기 위한 조건부 컴파일
#if UNITY_EDITOR || UNITY_STANDALONE // 에디터 또는 PC 스탠드얼론 빌드에서만 사용
        var stackTrace = new System.Diagnostics.StackTrace();
        var callingMethodName = stackTrace.GetFrame(1).GetMethod().Name;
        Debug.Log($"<color=yellow>[XML 저장] Pose ID: {newPose.id}</color> - (호출: {callingMethodName})");
#else // WebGL 또는 기타 빌드에서는 간단한 로그 사용
        Debug.Log($"<color=yellow>[XML 저장] Pose ID: {newPose.id}</color>");
#endif
    }

    /// <summary>
    /// 두 Pose 데이터가 사실상 동일한지 확인합니다.
    /// 위치와 각도의 미세한 차이를 허용하기 위해 허용 오차(Threshold)를 사용합니다.
    /// </summary>
    /// <param name="newPose">새로 기록할 Pose 데이터</param>
    /// <returns>중복이면 true, 아니면 false</returns>
    private bool IsPoseDuplicated(Make_XML.PoseData newPose)
    {
        if (lastRecordedPose == null) return false;

        // 위치 및 회전 값의 허용 오차 설정
        const float posThreshold = 0.001f; // 1mm
        const float rotThreshold = 0.1f;   // 0.1도

        // KUKA 로봇의 6축 관절 값 비교
        bool areAxesSimilar =
            Mathf.Abs(lastRecordedPose.kukaAxis.A1 - newPose.kukaAxis.A1) < rotThreshold &&
            Mathf.Abs(lastRecordedPose.kukaAxis.A2 - newPose.kukaAxis.A2) < rotThreshold &&
            Mathf.Abs(lastRecordedPose.kukaAxis.A3 - newPose.kukaAxis.A3) < rotThreshold &&
            Mathf.Abs(lastRecordedPose.kukaAxis.A4 - newPose.kukaAxis.A4) < rotThreshold &&
            Mathf.Abs(lastRecordedPose.kukaAxis.A5 - newPose.kukaAxis.A5) < rotThreshold &&
            Mathf.Abs(lastRecordedPose.kukaAxis.A6 - newPose.kukaAxis.A6) < rotThreshold;

        // Gantry의 외부 축(E1, E2, E3) 값 비교
        bool areGantrySimilar =
            Mathf.Abs(lastRecordedPose.gantry.E1 - newPose.gantry.E1) < posThreshold &&
            Mathf.Abs(lastRecordedPose.gantry.E2 - newPose.gantry.E2) < posThreshold &&
            Mathf.Abs(lastRecordedPose.gantry.E3 - newPose.gantry.E3) < posThreshold;

        // 모든 조건이 참일 경우에만 중복으로 판단
        return areAxesSimilar && areGantrySimilar;
    }


    /// <summary>
    /// 모든 기록을 초기화합니다.
    /// </summary>
    public void ClearAll()
    {
        AllPoses.Clear();
        currentPoseId = 1;
        lastRecordedPose = null; // 초기화 시 마지막 Pose 정보도 삭제
        Debug.Log("[XmlPathRecorder] 모든 Pose 기록이 초기화되었습니다.");
    }
}
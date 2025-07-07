using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using BioIK;
using TMPro;
using _SHI_BA;

public class Trans_XML : MonoBehaviour
{
    [Header("BioIK 참조")]
    public BioIK.BioIK bioIK;
    private BA_BioIK _bioIK
    {
        get
        {
            return bioIK as BA_BioIK;
        }
    }
    public BioSegment baseSegment;
    public BioSegment tcpSegment;

    // [Header("UI 참조")]
    // public TMP_InputField Input_XMLCode; // XML 코드 입력 필드
    // public TMP_Text statusText; // 상태 표시용 텍스트 (선택사항)

    // [Header("Motion Controller 참조")]
    // public BioIKMotionController motionController;

    [Header("RobotStartPose 참조")]
    public RobotStartPose robotStartPose;

    [System.Serializable]
    public class XMLPoseData
    {
        public int id;
        public float A1, A2, A3, A4, A5, A6; // KUKA 축
        public float E1, E2, E3; // Gantry 축
        public float E4, E5; // ROTI 축
    }

    /// <summary>
    /// 테스트용 함수 - 자유자세(FreePose)로 변경 + Auto IK 끄기
    /// </summary>
    [ContextMenu("TransTest - 자유자세로 변경")]
    public void TransTest()
    {
        if (robotStartPose == null)
        {
            Debug.LogError("[Trans_XML] RobotStartPose가 할당되지 않았습니다!");
            return;
        }

        if (bioIK == null)
        {
            Debug.LogError("[Trans_XML] BioIK가 할당되지 않았습니다!");
            return;
        }

        /*
        if (motionController == null)
        {
            Debug.LogError("[Trans_XML] BioIKMotionController가 할당되지 않았습니다!");
            return;
        }
        */

        Debug.Log("[Trans_XML] TransTest 실행 - 자유자세(FreePose)로 변경 중...");

        // 1. Auto IK 끄기
        bool originalAutoIK = _bioIK.autoIK;
        _bioIK.autoIK = false;
        Debug.Log($"[Trans_XML] Auto IK 변경: {originalAutoIK} → {_bioIK.autoIK}");

        // 2. 자유자세로 변경
        robotStartPose.ApplyPoseByName("FreePose");

        /*
        // 3. Motion Type을 Instantaneous로 변경
        motionController.SetInstantaneousMode();
        Debug.Log("[Trans_XML] Motion Type을 Instantaneous로 변경");
        */

        // 4. X Motion의 Target Value를 -2로 설정
        SetXMotionTargetValue(-2f);

        // 5. A1의 Target Value를 -60도로 설정
        SetA1TargetValue(-60f);

        Debug.Log("[Trans_XML] TransTest 완료 - Auto IK 끄기 + 자유자세 적용 + Instantaneous 모드 적용 + X Motion & A1 Target Value 설정됨");
    }

    /// <summary>
    /// XML 코드를 파싱하여 실행하는 메인 함수
    /// </summary>
    [ContextMenu("StartXMLFromCode - XML 실행")]
    public void StartXMLFromCode()
    {
#if false //
        if (Input_XMLCode == null)
        {
            Debug.LogError("[Trans_XML] Input_XMLCode가 할당되지 않았습니다!");
            return;
        }

        string xmlContent = Input_XMLCode.text;
        if (string.IsNullOrEmpty(xmlContent))
        {
            Debug.LogError("[Trans_XML] XML 내용이 비어있습니다!");
            return;
        }

        StartCoroutine(ExecuteXMLSequence(xmlContent));
#endif
    }

    /// <summary>
    /// XML 시퀀스를 실행하는 코루틴
    /// </summary>
    private IEnumerator ExecuteXMLSequence(string xmlContent)
    {
        Debug.Log("[Trans_XML] XML 시퀀스 실행 시작");

        // 1. 초기 설정
        if (!PrepareForXMLExecution())
        {
            yield break;
        }

        // 2. XML 파싱
        List<XMLPoseData> poses = ParseXMLToPoses(xmlContent);
        if (poses.Count == 0)
        {
            Debug.LogError("[Trans_XML] 파싱된 포즈가 없습니다!");
            yield break;
        }

        Debug.Log($"[Trans_XML] {poses.Count}개의 포즈를 순차 실행합니다");

        // 3. 포즈 순차 실행
        for (int i = 0; i < poses.Count; i++)
        {
            Debug.Log($"[Trans_XML] Pose {poses[i].id} 실행 중... ({i + 1}/{poses.Count})");

            ApplyPoseToRobot(poses[i]);

            // 마지막 포즈가 아니면 0.5초 대기
            if (i < poses.Count - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        Debug.Log("[Trans_XML] XML 시퀀스 실행 완료!");
    }

    /// <summary>
    /// XML 실행을 위한 초기 준비
    /// </summary>
    private bool PrepareForXMLExecution()
    {
        // 필수 참조 확인
        if (robotStartPose == null || bioIK == null /*|| motionController == null*/)
        {
            Debug.LogError("[Trans_XML] 필수 참조가 할당되지 않았습니다!");
            return false;
        }

        Debug.Log("[Trans_XML] 초기 설정 적용 중...");

        // 1. Auto IK 끄기
        _bioIK.autoIK = false;
        Debug.Log("[Trans_XML] Auto IK 비활성화");

        // 2. 자유자세 적용
        robotStartPose.ApplyPoseByName("FreePose");
        Debug.Log("[Trans_XML] 자유자세 적용");

        /*
        // 3. Motion Type을 Instantaneous로 변경
        motionController.SetInstantaneousMode();
        Debug.Log("[Trans_XML] Instantaneous 모드 적용");
        */

        // 4. upbox의 Limit을 -180에서 180까지로 변경
        SetUpboxLimits(-180f, 180f);
        Debug.Log("[Trans_XML] upbox Limit 설정 완료");

        return true;
    }

    /// <summary>
    /// XML 내용을 파싱하여 포즈 리스트로 변환
    /// </summary>
    private List<XMLPoseData> ParseXMLToPoses(string xmlContent)
    {
        List<XMLPoseData> poses = new List<XMLPoseData>();

        try
        {
            Debug.Log($"[Trans_XML] XML 파싱 시작, 길이: {xmlContent.Length} 문자");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNodeList poseNodes = doc.SelectNodes("//Pose");
            Debug.Log($"[Trans_XML] 찾은 Pose 노드 수: {poseNodes.Count}");

            foreach (XmlNode poseNode in poseNodes)
            {
                XMLPoseData pose = new XMLPoseData();

                // Pose ID 읽기
                if (int.TryParse(poseNode.Attributes["ID"]?.Value, out int poseId))
                    pose.id = poseId;

                // Kuka_AxisSpecific 데이터 읽기
                XmlNode axisNode = poseNode.SelectSingleNode(".//Kuka_AxisSpecific");
                if (axisNode != null)
                {
                    pose.A1 = ParseFloat(axisNode.Attributes["A1"]?.Value);
                    pose.A2 = ParseFloat(axisNode.Attributes["A2"]?.Value);
                    pose.A3 = ParseFloat(axisNode.Attributes["A3"]?.Value);
                    pose.A4 = ParseFloat(axisNode.Attributes["A4"]?.Value);
                    pose.A5 = ParseFloat(axisNode.Attributes["A5"]?.Value);
                    pose.A6 = ParseFloat(axisNode.Attributes["A6"]?.Value);
                }

                // Gantry_ExternalAxis 데이터 읽기
                XmlNode gantryNode = poseNode.SelectSingleNode(".//Gantry_ExternalAxis");
                if (gantryNode != null)
                {
                    pose.E1 = ParseFloat(gantryNode.Attributes["E1"]?.Value);
                    pose.E2 = ParseFloat(gantryNode.Attributes["E2"]?.Value);
                    pose.E3 = ParseFloat(gantryNode.Attributes["E3"]?.Value);
                }

                // ROTI_ExternalAxis 데이터 읽기
                XmlNode rotiNode = poseNode.SelectSingleNode(".//ROTI_ExternalAxis");
                if (rotiNode != null)
                {
                    pose.E4 = ParseFloat(rotiNode.Attributes["E4"]?.Value);
                    pose.E5 = ParseFloat(rotiNode.Attributes["E5"]?.Value);
                }

                poses.Add(pose);
                Debug.Log($"[Trans_XML] Pose {pose.id} 파싱 완료 - A1: {pose.A1}, E1: {pose.E1}");
            }

            Debug.Log($"[Trans_XML] XML 파싱 완료: {poses.Count}개의 포즈 데이터");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Trans_XML] XML 파싱 실패: {e.Message}");
        }

        return poses;
    }

    /// <summary>
    /// 파싱된 포즈를 로봇에 적용
    /// </summary>
    private void ApplyPoseToRobot(XMLPoseData pose)
    {
        Debug.Log($"[Trans_XML] Pose {pose.id} 적용 중...");

        foreach (var segment in bioIK.Segments)
        {
            if (segment.Joint == null) continue;
            string jointName = segment.Transform.name;

            switch (jointName)
            {
                // Gantry 축 (Make_XML의 변환식 역변환)
                case "x":
                    if (segment.Joint.X != null && segment.Joint.X.Enabled)
                    {
                        float xValue = pose.E1 / -10000f; // E1 = -10000f * x값 의 역변환
                        segment.Joint.X.SetTargetValue(xValue);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] x축 설정: {xValue} (E1: {pose.E1})");
                    }
                    break;

                case "y":
                    if (segment.Joint.Y != null && segment.Joint.Y.Enabled)
                    {
                        float yValue = pose.E2 / 1000f; // E2 = 1000f * y값 의 역변환
                        segment.Joint.Y.SetTargetValue(yValue);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] y축 설정: {yValue} (E2: {pose.E2})");
                    }
                    break;

                case "z":
                    if (segment.Joint.Z != null && segment.Joint.Z.Enabled)
                    {
                        float zValue = (pose.E3 - 2800f) / 1000f; // E3 = 1000f * z값 + 2800f 의 역변환
                        segment.Joint.Z.SetTargetValue(zValue);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] z축 설정: {zValue} (E3: {pose.E3})");
                    }
                    break;

                // ROTI (upbox)
                case "upbox":
                    SetUpboxValue(-pose.E4); // E4 = -upbox각도 의 역변환
                    Debug.Log($"[Trans_XML] upbox 설정: {-pose.E4} (E4: {pose.E4})");
                    break;

                // KUKA 축들
                case "A1":
                    SetJointAxisValue(segment, pose.A1, "A1");
                    break;

                case "A2":
                    if (segment.Joint.Y != null && segment.Joint.Y.Enabled)
                    {
                        float a2Value = (pose.A2 / -1f) - 90f; // A2 = (값 + 90f) * -1f 의 역변환
                        segment.Joint.Y.SetTargetValue(a2Value);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] A2 설정: {a2Value} (XML A2: {pose.A2})");
                    }
                    break;

                case "A3":
                    if (segment.Joint.Y != null && segment.Joint.Y.Enabled)
                    {
                        float a3Value = (pose.A3 / -1f) + 90f; // A3 = (값 - 90f) * -1f 의 역변환
                        segment.Joint.Y.SetTargetValue(a3Value);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] A3 설정: {a3Value} (XML A3: {pose.A3})");
                    }
                    break;

                case "A4":
                    if (segment.Joint.X != null && segment.Joint.X.Enabled)
                    {
                        float a4Value = pose.A4 / -1f; // A4 = -1f * 값 의 역변환
                        segment.Joint.X.SetTargetValue(a4Value);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] A4 설정: {a4Value} (XML A4: {pose.A4})");
                    }
                    break;

                case "A5":
                    if (segment.Joint.Y != null && segment.Joint.Y.Enabled)
                    {
                        float a5Value = pose.A5 / -1f; // A5 = -1f * 값 의 역변환
                        segment.Joint.Y.SetTargetValue(a5Value);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] A5 설정: {a5Value} (XML A5: {pose.A5})");
                    }
                    break;

                case "A6":
                    if (segment.Joint.X != null && segment.Joint.X.Enabled)
                    {
                        float a6Value = pose.A6 / -1f; // A6 = -1f * 값 의 역변환
                        segment.Joint.X.SetTargetValue(a6Value);
                        segment.Joint.ProcessMotion();
                        Debug.Log($"[Trans_XML] A6 설정: {a6Value} (XML A6: {pose.A6})");
                    }
                    break;
            }
        }

        Debug.Log($"[Trans_XML] Pose {pose.id} 적용 완료");
    }

    /// <summary>
    /// upbox 값 설정 (Transform 직접 조작)
    /// </summary>
    private void SetUpboxValue(float angle)
    {
        GameObject upboxObj = GameObject.Find("upbox");
        if (upboxObj != null)
        {
            // Transform의 Z축 회전을 직접 설정
            Vector3 currentEuler = upboxObj.transform.localEulerAngles;
            upboxObj.transform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, angle);

            // BioJoint도 있다면 같이 설정
            var upboxSegment = bioIK.FindSegment("upbox");
            if (upboxSegment?.Joint != null)
            {
                if (upboxSegment.Joint.Z != null && upboxSegment.Joint.Z.Enabled)
                {
                    upboxSegment.Joint.Z.SetTargetValue(angle);
                    upboxSegment.Joint.ProcessMotion();
                }
            }
        }
    }

    /// <summary>
    /// 조인트 축 값 설정 (A1 등)
    /// </summary>
    private void SetJointAxisValue(BioSegment segment, float value, string axisName)
    {
        BioJoint.Motion targetMotion = null;
        string motionAxis = "";

        // 활성화된 축 찾기 (우선순위: Z → Y → X)
        if (segment.Joint.Z != null && segment.Joint.Z.Enabled)
        {
            targetMotion = segment.Joint.Z;
            motionAxis = "Z";
        }
        else if (segment.Joint.Y != null && segment.Joint.Y.Enabled)
        {
            targetMotion = segment.Joint.Y;
            motionAxis = "Y";
        }
        else if (segment.Joint.X != null && segment.Joint.X.Enabled)
        {
            targetMotion = segment.Joint.X;
            motionAxis = "X";
        }

        if (targetMotion != null)
        {
            targetMotion.SetTargetValue(value);
            segment.Joint.ProcessMotion();
            Debug.Log($"[Trans_XML] {axisName} {motionAxis}축 설정: {value}");
        }
        else
        {
            Debug.LogWarning($"[Trans_XML] {axisName}에 활성화된 축이 없습니다!");
        }
    }

    /// <summary>
    /// X Motion의 Target Value를 설정하는 함수
    /// </summary>
    private void SetXMotionTargetValue(float targetValue)
    {
        if (bioIK == null)
        {
            Debug.LogError("[Trans_XML] BioIK가 할당되지 않았습니다!");
            return;
        }

        Debug.Log($"[Trans_XML] X Motion Target Value를 {targetValue}로 설정 중...");

        // BioIK의 모든 세그먼트를 검사하여 X Motion이 있는 조인트 찾기
        foreach (var segment in bioIK.Segments)
        {
            if (segment.Joint != null && segment.Joint.X != null && segment.Joint.X.Enabled)
            {
                Debug.Log($"[Trans_XML] 조인트 '{segment.Transform.name}'의 X Motion 발견");

                // 현재 Target Value 확인
                float currentTarget = (float)segment.Joint.X.GetTargetValue();
                Debug.Log($"[Trans_XML] 현재 X Motion Target Value: {currentTarget}");

                // Target Value 설정
                segment.Joint.X.SetTargetValue(targetValue);

                // ProcessMotion 호출 (Auto IK가 꺼져있을 때 필요)
                segment.Joint.ProcessMotion();

                // 설정 후 값 확인
                float afterTarget = (float)segment.Joint.X.GetTargetValue();
                Debug.Log($"[Trans_XML] 설정 후 X Motion Target Value: {afterTarget}");

                if (Mathf.Approximately(afterTarget, targetValue))
                {
                    Debug.Log($"[Trans_XML] ✅ 조인트 '{segment.Transform.name}' X Motion Target Value 설정 성공!");
                }
                else
                {
                    Debug.LogWarning($"[Trans_XML] ⚠️ 조인트 '{segment.Transform.name}' X Motion Target Value 설정 실패!");
                }

                break; // 첫 번째 X Motion만 설정하고 종료
            }
        }
    }

    /// <summary>
    /// A1 조인트의 Target Value를 설정하는 함수
    /// </summary>
    private void SetA1TargetValue(float targetAngleDegrees)
    {
        if (bioIK == null)
        {
            Debug.LogError("[Trans_XML] BioIK가 할당되지 않았습니다!");
            return;
        }

        Debug.Log($"[Trans_XML] A1 Target Value를 {targetAngleDegrees}도로 설정 중...");

        // 모든 조인트를 검사하여 정확히 "A1" 이름의 조인트 찾기
        bool foundA1 = false;
        for (int i = 0; i < bioIK.Segments.Count; i++)
        {
            var segment = bioIK.Segments[i];
            if (segment.Joint != null)
            {
                string jointName = segment.Transform.name;
                Debug.Log($"[Trans_XML] 조인트 검사 [{i}]: '{jointName}'");

                // 정확히 "A1" 이름인 조인트 찾기
                if (jointName == "A1")
                {
                    Debug.Log($"[Trans_XML] A1 조인트 발견: '{jointName}'");

                    // 어떤 축이 활성화되어 있는지 확인
                    Debug.Log($"[Trans_XML] A1 조인트 축 상태:");
                    Debug.Log($"[Trans_XML]   X축: {(segment.Joint.X != null && segment.Joint.X.Enabled ? "활성화" : "비활성화")}");
                    Debug.Log($"[Trans_XML]   Y축: {(segment.Joint.Y != null && segment.Joint.Y.Enabled ? "활성화" : "비활성화")}");
                    Debug.Log($"[Trans_XML]   Z축: {(segment.Joint.Z != null && segment.Joint.Z.Enabled ? "활성화" : "비활성화")}");

                    // 활성화된 축에 값 설정 (일반적으로 Z축이 회전축)
                    BioJoint.Motion targetMotion = null;
                    string axisName = "";

                    if (segment.Joint.Z != null && segment.Joint.Z.Enabled)
                    {
                        targetMotion = segment.Joint.Z;
                        axisName = "Z";
                    }
                    else if (segment.Joint.Y != null && segment.Joint.Y.Enabled)
                    {
                        targetMotion = segment.Joint.Y;
                        axisName = "Y";
                    }
                    else if (segment.Joint.X != null && segment.Joint.X.Enabled)
                    {
                        targetMotion = segment.Joint.X;
                        axisName = "X";
                    }

                    if (targetMotion != null)
                    {
                        Debug.Log($"[Trans_XML] A1 조인트의 {axisName}축에 값 설정");

                        // 현재 Target Value 확인
                        float currentTarget = (float)targetMotion.GetTargetValue();
                        Debug.Log($"[Trans_XML] 현재 A1 Target Value: {currentTarget}");

                        // Target Value 설정 (라디안 변환 없이 그대로)
                        Debug.Log($"[Trans_XML] 설정할 값: {targetAngleDegrees}");

                        targetMotion.SetTargetValue(targetAngleDegrees);

                        // ProcessMotion 호출 (Auto IK가 꺼져있을 때 필요)
                        segment.Joint.ProcessMotion();

                        // 설정 후 값 확인
                        float afterTarget = (float)targetMotion.GetTargetValue();
                        Debug.Log($"[Trans_XML] 설정 후 A1 Target Value: {afterTarget}");

                        if (Mathf.Approximately(afterTarget, targetAngleDegrees))
                        {
                            Debug.Log($"[Trans_XML] ✅ A1 조인트 {axisName}축 Target Value 설정 성공!");
                        }
                        else
                        {
                            Debug.LogWarning($"[Trans_XML] ⚠️ A1 조인트 {axisName}축 Target Value 설정 실패!");
                            Debug.LogWarning($"[Trans_XML] 예상값: {targetAngleDegrees}, 실제값: {afterTarget}");
                        }

                        foundA1 = true;
                    }
                    else
                    {
                        Debug.LogError("[Trans_XML] A1 조인트에 활성화된 축이 없습니다!");
                    }

                    break; // A1 조인트 처리 완료
                }
            }
        }

        if (!foundA1)
        {
            Debug.LogError("[Trans_XML] 'A1' 이름의 조인트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// upbox 조인트의 Limit을 설정하는 함수
    /// </summary>
    private void SetUpboxLimits(float lowerLimit, float upperLimit)
    {
        if (bioIK == null)
        {
            Debug.LogError("[Trans_XML] BioIK가 할당되지 않았습니다!");
            return;
        }

        Debug.Log($"[Trans_XML] upbox Limit을 {lowerLimit}~{upperLimit}로 설정 중...");

        // upbox 조인트 찾기
        var upboxSegment = bioIK.FindSegment("upbox");
        if (upboxSegment?.Joint != null)
        {
            Debug.Log($"[Trans_XML] upbox 조인트 발견: '{upboxSegment.Transform.name}'");

            // 활성화된 축 찾기 및 Limit 설정
            bool limitSet = false;

            if (upboxSegment.Joint.Z != null && upboxSegment.Joint.Z.Enabled)
            {
                upboxSegment.Joint.Z.SetLowerLimit(lowerLimit);
                upboxSegment.Joint.Z.SetUpperLimit(upperLimit);
                Debug.Log($"[Trans_XML] upbox Z축 Limit 설정: {lowerLimit} ~ {upperLimit}");
                limitSet = true;
            }

            if (upboxSegment.Joint.Y != null && upboxSegment.Joint.Y.Enabled)
            {
                upboxSegment.Joint.Y.SetLowerLimit(lowerLimit);
                upboxSegment.Joint.Y.SetUpperLimit(upperLimit);
                Debug.Log($"[Trans_XML] upbox Y축 Limit 설정: {lowerLimit} ~ {upperLimit}");
                limitSet = true;
            }

            if (upboxSegment.Joint.X != null && upboxSegment.Joint.X.Enabled)
            {
                upboxSegment.Joint.X.SetLowerLimit(lowerLimit);
                upboxSegment.Joint.X.SetUpperLimit(upperLimit);
                Debug.Log($"[Trans_XML] upbox X축 Limit 설정: {lowerLimit} ~ {upperLimit}");
                limitSet = true;
            }

            if (limitSet)
            {
                Debug.Log($"[Trans_XML] ✅ upbox Limit 설정 성공!");
            }
            else
            {
                Debug.LogWarning($"[Trans_XML] ⚠️ upbox에 활성화된 축이 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[Trans_XML] upbox 조인트를 찾을 수 없습니다!");
        }
    }

    /// <summary>
    /// 문자열을 float로 안전하게 변환
    /// </summary>
    private float ParseFloat(string value, float defaultValue = 0.0f)
    {
        if (string.IsNullOrEmpty(value)) return defaultValue;
        return float.TryParse(value, out float result) ? result : defaultValue;
    }
}
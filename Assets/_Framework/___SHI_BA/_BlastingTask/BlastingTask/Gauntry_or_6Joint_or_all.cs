using UnityEngine;
using UnityEditor;
using BioIK;
using System.Collections.Generic;
using _SHI_BA;

// 파일명: Gauntry_or_6Joint_or_all.cs
public class Gauntry_or_6Joint_or_all : MonoBehaviour
{
    [Header("IK System References")]
    [Tooltip("로봇팔을 제어하는 주 BioIK 컴포넌트입니다.")]
    public BioIK.BioIK bioIK;
    public BA_BioIK _bioIK
    {
        get { return bioIK as BA_BioIK; }
    }
    [Header("IK Target Segments")]
    [Tooltip("로봇의 TCP에 해당하는 BioSegment를 할당하세요.")]
    public BioSegment tcpSegment;
    [Tooltip("갠트리의 IKMover 컴포넌트들을 관리하는 GantryController 입니다.")]
    public GantryController gantryController;
    public Transform yMoverTransform;
    private float savedYMoverX;

    /// <summary>
    /// BioIK를 활성화하고 6축 로봇 관절(A1-A6)만 제어합니다.
    /// IKMover 갠트리 제어는 비활성화됩니다.
    /// </summary>
    public void EnableOnlyA1toA6()
    {
        // Debug.Log("[IkKuka6JointController] BioIK 제어 모드로 전환: 6축 관절(A1-A6)만 활성화.");
        
        // 1. IKMover를 끄고, BioIK 컴포넌트를 켭니다.
        SetIKMode(true); 
        if (bioIK == null) return;

        // --- [핵심 수정] ---
        // BioIK Solver의 내부 상태를 완전히 리셋하기 위해 컴포넌트를 껐다 켭니다.
        // IKMover -> BioIK 전환 시 발생할 수 있는 잠재적 상태 충돌을 해결하기 위함입니다.
        // Debug.LogWarning("[IkKuka6JointController] BioIK 시스템의 완전한 상태 동기화를 위해 강제 재초기화를 시도합니다.");
        bioIK.enabled = false;
        bioIK.enabled = true;
        // ---------------------------------

        // 2. A1-A6 관절만 활성화합니다.
        HashSet<string> enabledNames = new HashSet<string> { "A1", "A2", "A3", "A4", "A5", "A6" };
        ToggleBioJoints(enabledNames);
    }
    public void EnableGantryAndA5Orientation()
    {
        Debug.Log("▶ [IK 제어 모드 전환] 갠트리 이동과 A5 회전 제어를 동시에 활성화합니다.");

        // ▼▼▼ [수정된 로직] SetIKMode 대신, IKMover와 BioIK를 모두 활성화합니다. ▼▼▼
        if (gantryController != null)
        {
            if (gantryController.x_mover != null) gantryController.x_mover.AutoUpdate = true;
            if (gantryController.y_mover != null) gantryController.y_mover.AutoUpdate = true;
            if (gantryController.z != null) gantryController.z.AutoUpdate = true;
        }
        else
        {
            Debug.LogError("GantryController가 할당되지 않았습니다!");
        }

        if (_bioIK != null)
        {
            _bioIK.autoIK = true;
        }
        else
        {
             Debug.LogError("BioIK 컴포넌트가 할당되지 않았습니다!");
        }
        // ▲▲▲

        // A5 관절만 움직이도록 설정
        HashSet<string> enabledJoints = new HashSet<string> { "A5", "A6" };
        ToggleBioJoints(enabledJoints);

        // TCP의 Objective를 설정 (회전만, 가중치 10)
        if (tcpSegment != null)
        {
            foreach (var objective in tcpSegment.Objectives)
            {
                // [오류 수정] IsEnabled 대신 MonoBehaviour.enabled 속성을 사용
                if (objective is Orientation)
                {
                    objective.enabled = true; 
                    objective.SetWeight(10.0);
                    Debug.Log($"[Gauntry_or_6Joint_or_all] TCP Orientation Objective 활성화, 가중치 10으로 설정.");
                }
                else if (objective is Position)
                {
                    objective.enabled = false; 
                    Debug.Log($"[Gauntry_or_6Joint_or_all] TCP Position Objective 비활성화.");
                }
            }
        }
        else
        {
            Debug.LogError("TCP Segment가 할당되지 않았습니다! TCP Objective를 설정할 수 없습니다.");
        }
        
        // 변경사항을 BioIK 시스템에 적용
        if(bioIK != null) bioIK.Refresh();
    }
    public void EnableOnlyA5A6()
    {        
        // 1. IKMover를 끄고, BioIK 컴포넌트를 켭니다.
        SetIKMode(true); 
        if (bioIK == null) return;

        // --- [핵심 수정] ---
        // BioIK Solver의 내부 상태를 완전히 리셋하기 위해 컴포넌트를 껐다 켭니다.
        // IKMover -> BioIK 전환 시 발생할 수 있는 잠재적 상태 충돌을 해결하기 위함입니다.
        // Debug.LogWarning("[IkKuka6JointController] BioIK 시스템의 완전한 상태 동기화를 위해 강제 재초기화를 시도합니다.");
        bioIK.enabled = false;
        bioIK.enabled = true;
        // ---------------------------------

        // 2. A1-A6 관절만 활성화합니다.
        HashSet<string> enabledNames = new HashSet<string> { "A5", "A6" };
        ToggleBioJoints(enabledNames);
    }

    /// <summary>
    /// IKConstructor의 IKMover를 활성화하여 갠트리(x, y, z)만 제어합니다.
    /// BioIK 제어는 비활성화됩니다.
    /// </summary>
    public void EnableOnlyGauntry()
    {
        Debug.Log("▶ [IK 제어 모드 전환] IKMover 갠트리 제어를 활성화합니다.");

        // 1. IK 제어 모드를 BioIK에서 IKMover(Gantry)로 전환합니다.
        SetIKMode(false);

        // 2. GantryController의 참조가 있는지 확인합니다.
        if (gantryController == null)
        {
            Debug.LogError("GantryController 참조가 설정되지 않았습니다! 시퀀스를 시작할 수 없습니다.");
            return;
        }

        // 3. GantryController에 정의된 y축 우선 이동 시퀀스를 시작합니다.
        //    이것이 GantryController의 isYAxisPrioritySequenceActive 플래그를 true로 만듭니다.
        gantryController.EnableOnlyYaxis();

        // 4. (선택적) BioIK 관절을 모두 비활성화하여 충돌을 방지합니다.
        if (bioIK != null)
        {
            ToggleBioJoints(new HashSet<string>());
        }
    }

    public void ReinitializeBioIK()
    {
        if (bioIK != null)
        {
            Debug.LogWarning("[GauntryController] BioIK 시스템을 강제로 재초기화합니다.");

            // BioIK 컴포넌트를 비활성화했다가 즉시 다시 활성화하여
            // OnDisable() -> OnEnable() 생명주기 메서드를 실행, 시스템을 리셋합니다.
            bioIK.enabled = false;
            bioIK.enabled = true;
        }
    }

    /// <summary>
    /// BioIK를 활성화하고 Upbox 관절만 제어하며, 지정된 각도로 고정합니다.
    /// IKMover 갠트리 제어는 비활성화됩니다.
    /// </summary>
    public void EnableOnlyUpbox(float panAngle)
    {
        // Debug.Log("[IkKuka6JointController] BioIK 제어 모드로 전환: Upbox 관절만 활성화.");
        SetIKMode(true);
        if (bioIK == null) return;

        HashSet<string> enabledNames = new HashSet<string> { "upbox" };
        ToggleBioJoints(enabledNames);

        foreach (var segment in bioIK.Segments)
        {
            if (segment.Joint != null && segment.Joint.gameObject.name == "upbox")
            {
                if (segment.Joint.Y != null) // Y축이 Pan 각도를 제어한다고 가정
                {
                    segment.Joint.Y.SetLowerLimit(panAngle);
                    segment.Joint.Y.SetUpperLimit(panAngle);
                    // Debug.Log($"[IkKuka6JointController] Upbox Y축 회전 제한을 {panAngle}도로 설정했습니다.");
                }
                break; // upbox를 찾았으므로 루프 종료
            }
        }
    }

    /// <summary>
    /// IK 제어 모드를 BioIK와 IKMover 간에 전환하는 핵심 헬퍼 메서드입니다.
    /// </summary>
    /// <param name="useBioIK">BioIK를 사용하려면 true, IKConstructor(IKMover)를 사용하려면 false.</param>
    private void SetIKMode(bool useBioIK)
    {
        // 1. BioIK 컴포넌트 자체의 활성화 상태와 autoIK 속성을 제어합니다.
        if (_bioIK != null)
        {
            // bioIK.enabled = useBioIK;
            _bioIK.autoIK = useBioIK;
            // Debug.Log($"BioIK 컴포넌트 enabled: {bioIK.enabled}, autoIK: {bioIK.autoIK}");
        }
        else
        {
            Debug.LogWarning("IkKuka6JointController에 BioIK 참조가 없습니다.");
        }

        // 2. Gantry IKMover 컴포넌트들의 AutoUpdate 속성을 제어합니다.
        if (gantryController != null)
        {
            bool ikMoverAutoUpdate = !useBioIK;
            if (gantryController.x_mover != null) 
            {
                gantryController.x_mover.AutoUpdate = ikMoverAutoUpdate;
            }
            if (gantryController.y_mover != null)
            {
                gantryController.y_mover.AutoUpdate = ikMoverAutoUpdate;
            }
            if (gantryController.z != null)
            {
                gantryController.z.AutoUpdate = ikMoverAutoUpdate;
            }
            // Debug.Log($"갠트리 IKMover 자동 업데이트(AutoUpdate)가 '{ikMoverAutoUpdate}'로 설정되었습니다.");
        }
        else
        {
            Debug.LogWarning("IkKuka6JointController에 GantryController 참조가 없습니다.");
        }
    }

    /// <summary>
    /// BioJoint 컴포넌트들의 활성화 상태를 일괄적으로 제어하고, BioIK를 Refresh합니다.
    /// </summary>
    /// <param name="enabledNames">활성화할 관절 이름 목록 (Whitelist). null이면 모든 관절을 대상으로 합니다.</param>
    /// <param name="excludedNames">비활성화할 관절 이름 목록 (Blacklist).</param>
    private void ToggleBioJoints(HashSet<string> enabledNames, HashSet<string> excludedNames = null)
    {
        if (bioIK == null || bioIK.Segments == null) return;

        // Debug.Log($"BioJoint 상태 변경 시작... (활성화 대상: {enabledNames.Count}개)");
        foreach (var segment in bioIK.Segments)
        {
            if (segment.Joint != null)
            {
                bool shouldBeEnabled;
                if (enabledNames != null)
                {
                    // Whitelist 모드: enabledNames에 포함된 경우에만 활성화
                    shouldBeEnabled = enabledNames.Contains(segment.Joint.gameObject.name);
                }
                else
                {
                    // Blacklist 모드: 기본적으로 활성화
                    shouldBeEnabled = true;
                }

                if (excludedNames != null && excludedNames.Contains(segment.Joint.gameObject.name))
                {
                    shouldBeEnabled = false;
                }
                
                // 현재 상태와 다를 경우에만 변경하여 불필요한 연산을 줄입니다.
                if(segment.Joint.enabled != shouldBeEnabled)
                {
                    segment.Joint.enabled = shouldBeEnabled;
                }

                // 관련된 JointValue Objective도 함께 상태를 변경하여 충돌을 방지합니다.
                JointValue jv = segment.GetComponent<JointValue>();
                if (jv != null && jv.enabled != shouldBeEnabled)
                {
                    jv.enabled = shouldBeEnabled;
                }
            }
        }
        
        // 모든 Joint의 활성화 상태 변경 후 Refresh()를 호출하여 BioIK 시스템에 변경사항을 즉시 적용합니다.
        // Debug.Log("BioIK.Refresh()를 호출하여 관절 변경사항을 시스템에 적용합니다.");
        bioIK.Refresh();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Gauntry_or_6Joint_or_all))]
public class IkKuka6JointControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Gauntry_or_6Joint_or_all ctrl = (Gauntry_or_6Joint_or_all)target;
        if (GUILayout.Button("Enable BioIK (A1~A6 Only)"))
        {
            ctrl.EnableOnlyA1toA6();
        }
        if (GUILayout.Button("Enable IKMover (Gantry Only)"))
        {
            ctrl.EnableOnlyGauntry();
        }
        // "모든 Joint Enable" 버튼 제거
    }
}
#endif
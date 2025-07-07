using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RobotPoseDropdown : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public TMP_Dropdown poseDropdown;

    [Header("로봇 제어 스크립트 참조")]
    public RobotStartPose robotStartPose;

    // [Header("조인트 모드 컨트롤러 참조")]
    // public JointModeController jointModeController;
    [SerializeField]
    protected UI_MoveMode ui_MoveMode;

    [Header("Go to One Point UI 참조")]
    public Go_to_One_Point_UI goToOnePointUI;

    // 포즈 적용 상태 관리
    private enum PoseApplyState
    {
        None,
        RobotMode,
        ApplyPose,
        AllMode,
        ExecuteGoTo,
        FinalRobotMode,  // 추가: 마지막 Robot 모드
        FinalAllMode,    // 추가: 마지막 All 모드
        Complete
    }

    private PoseApplyState currentState = PoseApplyState.None;
    private float stateTimer = 0f;
    private string pendingPoseName = "";
    private int pendingPoseIndex = -1;

    // 포즈 이름들 (FreePose 추가)
    private List<string> poseNames = new List<string>
    {
        "home",
        "ready_front",
        "ready_diag",
        "ready_top",
        "ready_deep",
        "FreePose"  // 새로 추가
    };

    // 드롭다운 표시될 한글 이름들 (사용자용)
    private List<string> poseDisplayNames = new List<string>
    {
        "기본 자세",
        "정면 준비",
        "대각선 준비",
        "상단 준비",
        "바닥 준비",
        "자유자세"  // 새로 추가
    };
    public void SetPoseToFreePose()
    {
        // "FreePose"의 인덱스 찾기 (마지막 인덱스)
        int freePoseIndex = poseNames.IndexOf("FreePose");
        if (freePoseIndex >= 0)
        {
            // 드롭다운 값만 변경하고 OnPoseSelected는 호출하지 않음 (무한루프 방지)
            poseDropdown.SetValueWithoutNotify(freePoseIndex);
            Debug.Log("드롭다운이 자유자세로 설정됨 (이벤트 호출 없음)");
        }
        else
        {
            Debug.LogError("FreePose를 찾을 수 없습니다!");
        }
    }
    void Start()
    {
        SetupDropdown();
    }

    void Update()
    {
        // 포즈 적용 상태 머신 처리
        ProcessPoseApplyStateMachine();
    }

    void SetupDropdown()
    {
        if (poseDropdown == null)
        {
            Debug.LogError("Dropdown이 할당되지 않았습니다!");
            return;
        }

        // 드롭다운 옵션 초기화
        poseDropdown.ClearOptions();

        // 한글 이름들로 옵션 추가 (실제 이름은 저장하려면 poseNames 사용)
        poseDropdown.AddOptions(poseDisplayNames);

        // 드롭다운 값 변경 이벤트 등록
        poseDropdown.onValueChanged.AddListener(OnPoseSelected);

        // 기본적으로 첫 번째 항목 선택
        OnPoseSelected(0);
    }

    // 드롭다운에서 포즈 선택됐을 때 호출
    public void OnPoseSelected(int index)
    {
        if (robotStartPose == null)
        {
            Debug.LogError("RobotStartPose가 할당되지 않았습니다!");
            return;
        }

        /*
        if (jointModeController == null)
        {
            Debug.LogError("JointModeController가 할당되지 않았습니다!");
            return;
        }
        */

        if (index < 0 || index >= poseNames.Count)
        {
            Debug.LogError("잘못된 포즈 인덱스입니다!");
            return;
        }

        // 이미 진행 중이면 무시
        if (currentState != PoseApplyState.None)
        {
            Debug.Log("포즈 적용이 이미 진행 중입니다.");
            return;
        }

        string selectedPose = poseNames[index];
        Debug.Log($"===== 포즈 변경 시작: {poseDisplayNames[index]} ({selectedPose}) =====");

        // 포즈 적용 시작
        pendingPoseName = selectedPose;
        pendingPoseIndex = index;
        currentState = PoseApplyState.RobotMode;
        stateTimer = 0f;
    }

    // 포즈 적용 상태 머신 처리
    private void ProcessPoseApplyStateMachine()
    {
        if (currentState == PoseApplyState.None) return;

        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case PoseApplyState.RobotMode:
                if (stateTimer >= 0.05f) // 0.05초 대기
                {
                    Debug.Log("1단계: Robot 모드로 전환");
                    // jointModeController.SetMode(JointModeController.JointMode.Robot);
                    ui_MoveMode.SetMode(UI_MoveMode.JointMode.Robot);
                    currentState = PoseApplyState.ApplyPose;
                    stateTimer = 0f;
                }
                break;

            case PoseApplyState.ApplyPose:
                if (stateTimer >= 0.05f) // 0.05초 대기
                {
                    Debug.Log($"2단계: 포즈 적용 - {poseDisplayNames[pendingPoseIndex]} ({pendingPoseName})");
                    robotStartPose.ApplyPoseByName(pendingPoseName);
                    currentState = PoseApplyState.AllMode;
                    stateTimer = 0f;
                }
                break;

            case PoseApplyState.AllMode:
                if (stateTimer >= 0.05f) // 0.05초 대기
                {
                    Debug.Log("3단계: All 모드로 변경");
                    // jointModeController.SetMode(JointModeController.JointMode.All);
                    ui_MoveMode.SetMode(UI_MoveMode.JointMode.All);
                    currentState = PoseApplyState.ExecuteGoTo;
                    stateTimer = 0f;
                }
                break;

            case PoseApplyState.ExecuteGoTo:
                if (stateTimer >= 0.05f) // 0.05초 대기
                {
                    Debug.Log("4단계: GoToOnePointAll_UI 실행");
                    ExecuteGoToOnePointAll();
                    currentState = PoseApplyState.FinalRobotMode;
                    stateTimer = 0f;
                }
                break;

            case PoseApplyState.FinalRobotMode:
                if (stateTimer >= 0.05f) // 0.05초 대기
                {
                    Debug.Log("5단계: 최종 Robot 모드로 전환");
                    // jointModeController.SetMode(JointModeController.JointMode.Robot);
                    ui_MoveMode.SetMode(UI_MoveMode.JointMode.Robot);
                    currentState = PoseApplyState.FinalAllMode;
                    stateTimer = 0f;
                }
                break;

            case PoseApplyState.FinalAllMode:
                if (stateTimer >= 0.05f) // 0.05초 대기
                {
                    Debug.Log("6단계: 최종 All 모드로 변경");
                    // jointModeController.SetMode(JointModeController.JointMode.All);
                    ui_MoveMode.SetMode(UI_MoveMode.JointMode.All);
                    currentState = PoseApplyState.Complete;
                    stateTimer = 0f;
                }
                break;

            case PoseApplyState.Complete:
                Debug.Log($"===== 포즈 변경 완료: {poseDisplayNames[pendingPoseIndex]} =====");
                // 상태 초기화
                currentState = PoseApplyState.None;
                pendingPoseName = "";
                pendingPoseIndex = -1;
                stateTimer = 0f;
                break;
        }
    }

    // GoToOnePointAll_UI 실행 함수
    private void ExecuteGoToOnePointAll()
    {
        if (goToOnePointUI != null)
        {
            goToOnePointUI.GoToOnePointAll_UI();
            Debug.Log("GoToOnePointAll_UI 실행 완료");
        }
        else
        {
            Debug.LogWarning("Go_to_One_Point_UI가 할당되지 않았습니다!");
        }
    }

    // 코드에서 포즈 직접 설정하고 싶을 때 사용
    public void SetPoseByName(string poseName)
    {
        int index = poseNames.IndexOf(poseName);
        if (index >= 0)
        {
            poseDropdown.value = index;
            OnPoseSelected(index);
        }
        else
        {
            Debug.LogError($"포즈 '{poseName}'을 찾을 수 없습니다!");
        }
    }

    // 현재 선택된 포즈 이름 반환
    public string GetCurrentPoseName()
    {
        if (poseDropdown.value >= 0 && poseDropdown.value < poseNames.Count)
        {
            return poseNames[poseDropdown.value];
        }
        return "";
    }

    // 현재 선택된 포즈의 한글 이름 반환
    public string GetCurrentPoseDisplayName()
    {
        if (poseDropdown.value >= 0 && poseDropdown.value < poseDisplayNames.Count)
        {
            return poseDisplayNames[poseDropdown.value];
        }
        return "";
    }
}
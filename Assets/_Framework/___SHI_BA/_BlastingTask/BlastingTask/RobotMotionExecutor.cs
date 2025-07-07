using UnityEngine;
using System.Collections;
using BioIK;
using System.Linq; // OrderBy 사용을 위해 추가
using System.Collections.Generic;
using _SHI_BA;
using Unity.VisualScripting;
using System.Data;


public class RobotMotionExecutor
{
    private static string LOG_FORMAT = "<color=#C811AD><b>[RobotMotionExecutor]</b></color> {0}";

    // --- 외부 참조 컴포넌트 ---
    private readonly MonoBehaviour coroutineRunner;
    private readonly BA_BioIK bioIK;

    private readonly Transform eef;
    private readonly GameObject targetObj;
    private readonly Gauntry_or_6Joint_or_all jointController;
    private readonly RobotStartPose robotStartPose;
    private readonly XmlPathRecorder xmlRecorder;
    private readonly Toggle_Obstacle_Avoiding toggleObstacleAvoiding;

    private LayerMask currentDetectionLayer;
    private HashSet<GameObject> collidedObstaclesThisRow = new HashSet<GameObject>();

    // --- 상태 관리 ---
    private enum MotionState { Idle, Preparing, PositioningGantry, AligningTCP, ExecutingPath, Completed }
    private MotionState currentState;
    private BA_MotionPath currentPath;
    private string currentPoseName;
    private int currentTargetIndex;
    private bool isPoseCorrectionActive;
    private bool isCurrentlyAvoiding = false;
    public bool IsExecuting { get; private set; }
    private Coroutine currentMotionCoroutine = null; // 현재 실행 중인 코루틴 저장
    private float currentFace7_AngleBps = 120f;     // 7번 면의 현재 각도 상태
    private Dictionary<GameObject, int> processedParentObstacles = new Dictionary<GameObject, int>();

    // private Dictionary<GameObject, bool> processedParentObstacles = new Dictionary<GameObject, bool>();
    private bool wasMovingHorizontally = true;
    private bool isChangingRow = false; // <<< 이 라인을 추가합니다.


    // private bool canCorrectPosture = true; // [신규 추가] 자세 교정 가능 여부 플래그 (자물쇠 역할)
    private int completedHorizontalRows = 0; // [신규 추가] 완료된 가로줄 수를 세는 변수
    // private bool postureCorrectionJustCompleted = false; // [신규 추가] 자세 교정 완료 플래그

    /// <summary>
    /// 생성자: 8개의 인자를 모두 받도록 수정되었습니다.
    /// </summary>
    public RobotMotionExecutor(MonoBehaviour runner, BA_BioIK bioIK, GameObject targetObj, Transform eef, Gauntry_or_6Joint_or_all jointController, RobotStartPose robotStartPose, XmlPathRecorder xmlRecorder, Toggle_Obstacle_Avoiding toggleObstacleAvoiding)
    {
        this.coroutineRunner = runner;
        this.bioIK = bioIK;
        this.targetObj = targetObj;
        this.eef = eef;
        this.jointController = jointController;
        this.robotStartPose = robotStartPose;
        this.xmlRecorder = xmlRecorder;
        this.toggleObstacleAvoiding = toggleObstacleAvoiding; // toggleObstacleAvoiding 할당 추가
        this.currentState = MotionState.Idle;
    }
    /// <summary>
    /// [신규 추가] BioIK의 최대 가속도 값을 설정하여 모션 타입을 제어합니다.
    /// 이 메서드는 RobotMotionExecutor 내부에서만 사용됩니다.
    /// </summary>
    /// <param name="maxAcceleration">새로운 최대 가속도 값. 0으로 설정 시 등속 모드로 동작합니다.</param>
    private void SetMotionMode(float maxAcceleration)
    {
        if (this.bioIK == null) return;

        // BioIK 컴포넌트의 MaximumAcceleration 값을 직접 변경합니다.
        this.bioIK.MaximumAcceleration = maxAcceleration;

        // if (maxAcceleration > 0)
        // {
        //     Debug.Log($"[Executor] 가속 모드 설정. MaxAcceleration: {maxAcceleration}");
        // }
        // else
        // {
        //     Debug.Log($"[Executor] 등속 모드 설정. MaxAcceleration: 0");
        // }
    }
    /// <summary>
    /// 새로운 경로로 모션 실행을 시작합니다.
    /// </summary>
    public void StartMotion(BA_MotionPath path, string poseName)
    {
        if (IsExecuting) return;
        Debug.Log($"[Executor.StartMotion] 모션 시작. FaceIndex: {path.FaceIndex}");
        this.currentPath = path;
        this.currentPoseName = poseName;
        this.currentTargetIndex = 0;
        this.IsExecuting = true;
        this.isCurrentlyAvoiding = false; // <<< 이 라인을 추가하여 상태를 초기화합니다.

        // 모든 상태 변수 초기화
        collidedObstaclesThisRow.Clear();
        processedParentObstacles.Clear();
        completedHorizontalRows = 0;
        wasMovingHorizontally = true; // 첫 시작은 수평으로 간주

        if (path.FaceIndex == 8)
        {
            this.currentFace7_AngleBps = 120f; // 7번 면의 시작 각도는 항상 120도
        }

        // jointController를 통해 GantryController의 x_mover에 접근합니다.
        if (jointController != null && jointController.gantryController != null)
        {
            var gantry = jointController.gantryController;
            if (gantry.x_mover != null)
            {
                // [수정] IKMover의 속성 이름 'MaximumVelocity'를 'Speed'로 변경합니다.
                if (path.FaceIndex == 8)
                {
                    // 7번 면일 경우 속도를 500 mm/s (0.5 m/s)로 설정
                    gantry.x_mover.Speed = 10f; // MaximumVelocity -> Speed
                    gantry.x_mover.AccelerationTime = 5f; // MaximumVelocity -> Speed
                    gantry.y_mover.Speed = 1000f; // MaximumVelocity -> Speed
                    gantry.y_mover.AccelerationTime = 5f; // MaximumVelocity -> Speed
                    gantry.z.Speed = 1000f; // MaximumVelocity -> Speed
                    gantry.z.AccelerationTime = 5f; // MaximumVelocity -> Speed

                    Debug.Log($"[Executor] 7번 면 감지. X_Mover 속도를 {gantry.x_mover.Speed} m/s (500 mm/s)로 설정합니다.");
                }
                else
                {
                    // 그 외의 모든 면에서는 속도를 10000 mm/s (10 m/s)로 설정
                    gantry.x_mover.Speed = 10000f; // MaximumV+elocity -> Speed
                    gantry.x_mover.AccelerationTime = 500f; // MaximumVelocity -> Speed
                    gantry.y_mover.Speed = 10000f; // MaximumVelocity -> Speed
                    gantry.y_mover.AccelerationTime = 500f; // MaximumVelocity -> Speed
                    gantry.z.Speed = 10000f; // MaximumVelocity -> Speed
                    gantry.z.AccelerationTime = 500f; // MaximumVelocity -> Speed
                    Debug.Log($"[Executor] 일반 면 감지. X_Mover 속도를 {gantry.x_mover.Speed} m/s (10000 mm/s)로 설정합니다.");
                }
            }
            else
            {
                Debug.LogWarning("[Executor] GantryController의 x_mover가 할당되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[Executor] JointController 또는 GantryController가 할당되지 않았습니다.");
        }
        currentMotionCoroutine = coroutineRunner.StartCoroutine(MotionStateMachine());
    }


    public void OnObstacleDetected(GameObject hitObject)
    {
        // 이미 회피 동작이 진행 중인 경우, 중복 감지 및 처리 방지
        if (isCurrentlyAvoiding) return;

        // 현재 모션이 실행 중이 아니거나 7번 면 작업이 아닌 경우, 처리를 건너뜁니다.
        if (!IsExecuting || currentPath == null || currentPath.FaceIndex != 8) return;

        // 충돌한 오브젝트에서 ObstacleIdentifier 컴포넌트를 가져옵니다.
        ObstacleIdentifier identifier = hitObject.GetComponent<ObstacleIdentifier>();
        // ObstacleIdentifier가 없는 경우, 이 오브젝트는 장애물로 처리하지 않습니다.
        if (identifier == null)
        {
            return;
        }

        GameObject parentObstacle; // 이 장애물의 부모 오브젝트 (논리적 그룹의 대표)
        bool isChildObject;        // 이 장애물이 부모 그룹 내의 자식인지 여부

        // 장애물 타입에 따라 부모 오브젝트를 결정합니다.
        if (identifier.type == ObstacleIdentifier.ObstacleType.Child)
        {
            isChildObject = true;
            parentObstacle = identifier.parentObstacle; // Inspector에서 할당된 부모를 사용

            // 자식 장애물이지만 부모가 할당되지 않은 경우 오류를 기록하고 건너뜁니다.
            if (parentObstacle == null)
            {
                Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}'에 부모(parentObstacle)가 할당되지 않았습니다. 회피를 건너뜁니다.", hitObject);
                return;
            }
        }
        else // ObstacleIdentifier.ObstacleType.Parent
        {
            isChildObject = false;
            parentObstacle = hitObject; // 부모 장애물인 경우 자신이 그룹의 대표입니다.
        }

        // processedParentObstacles 딕셔너리에서 현재 부모 장애물 그룹의 트리거 횟수를 가져옵니다.
        // 아직 딕셔너리에 없으면 기본값 0을 사용합니다.
        int currentTriggerCount = processedParentObstacles.GetValueOrDefault(parentObstacle, 0);


        if (isChildObject)
        {
            if (currentTriggerCount == 2 && processedParentObstacles.GetValueOrDefault(parentObstacle, 0) != 3)
            {
                Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 부모 '{parentObstacle.name}'가 2번 트리거됨({currentTriggerCount}회). 자식 회피 트리거.");
                // 자식 트리거는 '3' 단계로 전달합니다.
                PerformAngleSwitchAndPathModification(parentObstacle, "자식", 3);
                processedParentObstacles[parentObstacle] = 3; // 자식 트리거 완료로 표시 (총 3회 트리거)
            }
            // 부모가 아직 2번 트리거되지 않았거나 (0 또는 1),
            // 이미 자식까지 모두 트리거된 상태 (3)라면, 현재 자식 장애물 감지는 무시됩니다.
            else if (currentTriggerCount < 2)
            {
                Debug.Log($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 하지만 부모 '{parentObstacle.name}'는 {currentTriggerCount}회만 트리거됨 (2회 필요). 현재 자식 트리거 무시.");
            }
            else if (currentTriggerCount >= 3)
            {
                Debug.Log($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 이미 회피가 트리거되었습니다. 무시.");
            }
        }
        else // 부모 장애물인 경우:
        {
            // 부모 장애물은 최대 2번까지 'PerformAngleSwitchAndPathModification'를 트리거합니다.
            // (이전에 부모 2회차에서 PerformAngleSwitchAndPathModification 호출을 건너뛰던 로직이 변경됨)
            if (currentTriggerCount < 2)
            {
                Debug.LogWarning($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지 ({currentTriggerCount + 1}회 트리거). 부모 회피 트리거.");
                // 부모의 현재 트리거 횟수를 'triggerStage'로 전달 (1회차는 1, 2회차는 2)
                PerformAngleSwitchAndPathModification(parentObstacle, "부모", currentTriggerCount + 1);
                processedParentObstacles[parentObstacle] = currentTriggerCount + 1; // 부모의 트리거 횟수 1 증가
            }
            else // 부모 장애물이 이미 2번 트리거된 경우
            {
                Debug.Log($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지. 이미 2번 트리거되었습니다. 무시.");
            }
        }
    }

    private void PerformAngleSwitchAndPathModification(GameObject parentObstacle, string type, int triggerStage)
    {
        isCurrentlyAvoiding = true;

        StopCurrentMotion();

        // triggerStage를 CollisionResponseSequence_Face7 코루틴에 전달합니다.
        currentMotionCoroutine = coroutineRunner.StartCoroutine(CollisionResponseSequence_Face7(triggerStage));
    }
    private IEnumerator CorrectPostureAtNewRow(int newRowStartIndex)
    {
        Debug.Log("<color=cyan>[Coroutine Start] CorrectPostureAtNewRow 시작됨.</color>");

        collidedObstaclesThisRow.Clear();
        processedParentObstacles.Clear();
        // 이 코루틴은 이제 카운터를 직접 증가시키지 않습니다.
        // Debug.LogWarning($"[Executor] {completedHorizontalRows + 1}번째 줄 진입. 자세 교정을 시작합니다.");

        // 줄 바꿈 전용 각도 결정 메서드를 호출합니다.
        if (currentPath.FaceIndex == 8)
        {
            SwitchPathAngleAndApplyToFutureRotations(newRowStartIndex);
        }

        BioIK.BioSegment tcpSegment = bioIK.FindSegment(this.eef);
        double originalPositionWeight = 1.0;
        double originalOrientationWeight = 1.0;

        try
        {
            if (tcpSegment != null)
            {
                // Debug.Log("[Executor] TCP 가중치 조절: Position(0), Orientation(10)");
                foreach (var objective in tcpSegment.Objectives)
                {
                    if (objective is BioIK.Position p) { originalPositionWeight = p.Weight; p.Weight = 0.0; }
                    else if (objective is BioIK.Orientation o) { originalOrientationWeight = o.Weight; o.Weight = 10.0; }
                }
            }

            targetObj.transform.rotation = Quaternion.Euler(currentPath.PointList[newRowStartIndex].eulerAngles);
            jointController.EnableOnlyA5A6();

            yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 3.0f));
            Debug.LogWarning("[Executor] 줄 바꿈 자세 교정 완료. 현재 상태를 XML로 기록합니다.");
            xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<BA_Main>().makeXmlInstance);
            // xmlRecorder.RecordCurrentPose();
        }
        finally
        {
            if (tcpSegment != null)
            {
                // Debug.Log("[Executor] TCP 가중치를 원래대로 복원합니다.");
                foreach (var objective in tcpSegment.Objectives)
                {
                    if (objective is BioIK.Position p) { p.Weight = originalPositionWeight; }
                    else if (objective is BioIK.Orientation o) { o.Weight = originalOrientationWeight; }
                }
            }
        }

        jointController.EnableOnlyGauntry();
        // Debug.Log("[Executor] 자세 교정 완료. 갠트리 이동을 재개합니다.");
        Debug.Log("<color=red>[Coroutine End] CorrectPostureAtNewRow 정상 종료.</color>");

    }

    private IEnumerator ResumeMotionFrom(int startIndex)
    {
        // 코루틴 시작 시, 현재 상태를 기준으로 wasMovingHorizontally 플래그를 정확히 초기화합니다.
        if (startIndex > 0 && startIndex < currentPath.PointList.Count)
        {
            Vector3 initialDirection = (currentPath.PointList[startIndex].position - currentPath.PointList[startIndex - 1].position).normalized;
            wasMovingHorizontally = Mathf.Abs(Vector3.Dot(initialDirection, currentPath.WidthDir)) > 0.9f;
        }
        else
        {
            wasMovingHorizontally = true;
        }

        for (int i = startIndex; i < currentPath.PointList.Count; i++)
        {
            currentTargetIndex = i;

            if (i > 0)
            {
                Vector3 currentDirection = (currentPath.PointList[i].position - currentPath.PointList[i - 1].position).normalized;
                bool isNowMovingHorizontally = Mathf.Abs(Vector3.Dot(currentDirection, currentPath.WidthDir)) > 0.9f;

                // 이벤트 1: 수평 이동이 끝나고, 수직 이동(줄 바꿈)이 시작될 때
                if (wasMovingHorizontally && !isNowMovingHorizontally)
                {
                    completedHorizontalRows++;
                    Debug.Log($"<color=lime>--- {completedHorizontalRows}번째 가로줄 완료 ---</color>");
                    collidedObstaclesThisRow.Clear();
                    processedParentObstacles.Clear();

                    // "줄 바꾸는 중" 상태로 전환 (잠금 설정)
                    if (!isChangingRow) // <<< 이미 바꾸는 중이 아닐 때만 true로 설정
                    {
                        isChangingRow = true;
                    }
                }

                // 이벤트 2: 수직 이동이 끝나고, 새로운 수평 이동이 시작될 때
                if (!wasMovingHorizontally && isNowMovingHorizontally)
                {
                    // "줄 바꾸는 중" 상태일 때만 자세 교정 코루틴을 실행합니다.
                    if (isChangingRow && currentPath.FaceIndex == 8 && completedHorizontalRows >= 1)
                    {
                        yield return coroutineRunner.StartCoroutine(CorrectPostureAtNewRow(i));

                        // <<< 핵심 수정: 자세 교정 코루틴이 완전히 끝난 후에 잠금을 해제합니다. >>>
                        isChangingRow = false;
                    }
                }
                wasMovingHorizontally = isNowMovingHorizontally;
            }

            // XML 저장 및 경로 이동 로직
            Quaternion rotation = Quaternion.Euler(currentPath.PointList[i].eulerAngles);
            targetObj.transform.SetPositionAndRotation(currentPath.PointList[i].position, rotation);
            yield return coroutineRunner.StartCoroutine(WaitUntilStagnationOrTimeout(150.0f));
            xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<BA_Main>().makeXmlInstance);
            Debug.Log("<color=red>[Coroutine End] MotionStateMachine 정상 종료.</color>");

            if (coroutineRunner is BA_Main mainScript && mainScript.simulationStatus != null)
            {
                mainScript.simulationStatus.OnPointCompleted();
            }
            yield return null;
        }

        CompleteMotion();

    }
 
    private void CompleteMotion()
    {
        Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex} 작업 완료");

        SetMotionMode(5.0f);
        IsExecuting = false;
        isCurrentlyAvoiding = false; // <<< 이 라인을 추가하여 상태를 리셋합니다.
        bioIK.autoIK = true;
        if (toggleObstacleAvoiding != null) toggleObstacleAvoiding.SetShapeDistanceObjectives(false);
    }

    private IEnumerator CollisionResponseSequence_Face7(int triggerStage)
    {
        Debug.Log("<color=cyan>[Coroutine Start] CollisionResponseSequence_Face7 시작됨.</color>");
        IsExecuting = true;

        // --- 요청 1, 2, 3: 부모 1회차, 부모 2회차, 자식 1회차 충돌 '순간' RecordCurrentPose 호출 ---
        // 이 부분은 모든 트리거 단계에서 항상 실행됩니다.
        xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<BA_Main>().makeXmlInstance);
        Debug.Log($"[Face7 Response] RecordCurrentPose 호출됨 (충돌 순간). (트리거 단계: {triggerStage})");

        // --- ToggleAngleForCollision() 및 로봇 회전 동작 조건부 실행 ---
        // 요청 1: 부모 1회차 -> ToggleAngleForCollision() 호출
        // 요청 3: 자식 1회차 -> ToggleAngleForCollision() 호출
        if (triggerStage == 1 || triggerStage == 3)
        {
            ToggleAngleForCollision();
            Debug.Log($"[Face7 Response] ToggleAngleForCollision 호출됨. (트리거 단계: {triggerStage})");

            BioIK.BioSegment tcpSegment = bioIK.FindSegment(this.eef);
            double originalPositionWeight = 1.0;
            double originalOrientationWeight = 1.0;

            try
            {
                // TCP Objective 가중치 조절 (회전 집중)
                if (tcpSegment != null)
                {
                    foreach (var objective in tcpSegment.Objectives)
                    {
                        if (objective is BioIK.Position p) { originalPositionWeight = p.Weight; p.Weight = 0.0; }
                        else if (objective is BioIK.Orientation o) { originalOrientationWeight = o.Weight; o.Weight = 10.0; }
                    }
                }

                // 요청 4: 자식 1회차 충돌 시 EnableOnlyA5A6 호출 (부모 1회차에도 동일 적용)
                jointController.EnableOnlyA5A6();

                targetObj.transform.rotation = Quaternion.Euler(currentPath.PointList[currentTargetIndex].eulerAngles);
                // 회전 완료까지 대기
                yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 2.0f));
                Debug.LogWarning($"[Executor] 충돌 회피 회전 완료. (트리거 단계: {triggerStage})");

                // 요청 4: 자식 1회차 충돌 후 회전이 끝났을 때 RecordCurrentPose 호출
                if (triggerStage == 3) // 자식 1회차인 경우에만 추가 호출
                {
                    // xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
                    Debug.Log($"[Face7 Response] RecordCurrentPose 호출됨 (회전 완료 후). (트리거 단계: {triggerStage})");
                }
            }
            finally
            {
                // TCP Objective 가중치 원래대로 복원
                if (tcpSegment != null)
                {
                    foreach (var objective in tcpSegment.Objectives)
                    {
                        if (objective is BioIK.Position p) { p.Weight = originalPositionWeight; }
                        else if (objective is BioIK.Orientation o) { o.Weight = originalOrientationWeight; }
                    }
                }
            }
        }
        // 요청 2: 부모 2회차 -> ToggleAngleForCollision() 호출 건너뜀
        else if (triggerStage == 2)
        {
            Debug.Log($"[Face7 Response] 부모 2회차 감지. ToggleAngleForCollision 호출 및 회피 동작 건너뜀. (트리거 단계: {triggerStage})");
            // 이 경우, 회피 동작이나 회전 대기 없이 바로 다음 단계로 넘어갑니다.
        }

        isCurrentlyAvoiding = false;
        jointController.EnableOnlyGauntry(); // 갠트리 이동 재개
        // 남은 경로를 계속 진행
        yield return coroutineRunner.StartCoroutine(ResumeMotionFrom(currentTargetIndex));
        Debug.Log("<color=red>[Coroutine End] CollisionResponseSequence_Face7 정상 종료.</color>");
    }

    private void ToggleAngleForCollision()
    {
        // 현재 각도를 기준으로 즉시 반대 각도로 토글합니다.
        currentFace7_AngleBps = Mathf.Approximately(currentFace7_AngleBps, 120f) ? 60f : 120f;
        // Debug.Log($"<color=red>[Executor] 충돌 회피! 새로운 목표 각도는 {currentFace7_AngleBps}도 입니다.</color>");

        Quaternion newTargetRotation = Quaternion.LookRotation(
            Quaternion.AngleAxis(currentFace7_AngleBps, currentPath.HeightDir) * currentPath.FaceNormal,
            currentPath.HeightDir
        );

        // 경로의 남은 부분에 새로운 회전값을 적용합니다.
        for (int i = currentTargetIndex; i < currentPath.PointList.Count; i++)
        {
            currentPath.PointList[i].eulerAngles = newTargetRotation.eulerAngles;
        }
    }
    public void StopCurrentMotion()
    {
        if (currentMotionCoroutine != null)
        {
            coroutineRunner.StopCoroutine(currentMotionCoroutine);
        }
        // IsExecuting 플래그를 false로 설정하여, 현재 어떤 모션도 실행 중이 아님을 명시합니다.
        IsExecuting = false;
        // Debug.Log("[Executor] 현재 진행 중인 모션 코루틴을 중단했습니다.");
    }

    public IEnumerator WaitUntilRotationAligned(float threshold, float timeout)
    {
        float timer = 0f;
        while (timer < timeout)
        {
            float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);

            if (rotErr < threshold)
            {
                // 목표 오차 이내로 들어왔으므로 루프 종료
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Debug.LogWarning($"[Executor] TCP 회전 정렬 시간 초과 (오차: {Quaternion.Angle(eef.rotation, targetObj.transform.rotation):F2}도)");
    }
    private IEnumerator MotionStateMachine()
    {
        Debug.Log("<color=cyan>[Coroutine Start] MotionStateMachine 시작됨.</color>");

        // === 1. 준비 및 자세 잡기 ===
        currentState = MotionState.Preparing;
        // Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex}, 자세 '{currentPoseName}' 준비 시작");

        // 안정적인 자세 확보를 위해 '가속 모드' 활성화
        jointController.EnableOnlyA1toA6();
        bioIK.autoIK = false;

        // SetMainTcpObjectives(false);

        SetMotionMode(2.0f);
        // jointController.EnableOnlyA1toA6();

        // [핵심 수정] RobotStartPose의 코루틴을 직접 받고, 해당 코루틴이 끝날 때까지 기다립니다.
        // Debug.Log("[Executor] RobotStartPose의 자세 잡기 시퀀스를 시작하고 완료될 때까지 대기합니다...");
        yield return coroutineRunner.StartCoroutine(robotStartPose.PosingSequenceCoroutine(this.currentPoseName, currentPath.FaceNormal));
        // Debug.Log("[Executor] RobotStartPose 시퀀스 완료됨. 다음 단계로 진행합니다.");
        // SetMainTcpObjectives(true);

        // IK 재활성화 및 upbox 최종 회전
        bioIK.autoIK = true;
        float panAngle = CalculatePanAngleForNormal(currentPath.FaceNormal);
        Debug.Log(currentPath.FaceNormal + " - PanAngle: " + panAngle);
        if (currentPath.FaceIndex == 8)
        {
            float panAngle1 = -90f;
            float panAngle2 = -60f;
            RotateUpboxDirectly(-panAngle1);
            RotatedownDirectly(-panAngle2);

            // Debug.LogWarning($"[Executor] 면 7번이므로 PanAngle을 -90도로 강제 설정합니다.");
        }
        else
        {
            RotatedownDirectly(0);
            RotateUpboxDirectly(panAngle);
        }

        // === 2. 갠트리 위치 잡기 ===

        currentState = MotionState.PositioningGantry;
        // Debug.Log($"[Executor] State: {currentState} - 갠트리 시작점으로 이동");
        jointController.EnableOnlyGauntry();

        // === [수정] IK 포즈가 실제로 반영될 때까지 충분히 대기 ===
        yield return new WaitForSeconds(0.5f); // 또는 여러 프레임 yield return null;

        // 이제 경로 첫 점으로 이동
        targetObj.transform.position = currentPath.PointList[currentTargetIndex].position;
        targetObj.transform.rotation = Quaternion.Euler(currentPath.PointList[currentTargetIndex].eulerAngles);
        // yield return new WaitUntil(() => Vector3.Distance(eef.position, targetObj.transform.position) < 0.1f);
        while (Vector3.Distance(eef.position, targetObj.transform.position) >= 0.1f)
        {
#if DEBUG
            Debug.LogFormat(LOG_FORMAT, "");
#endif
            yield return null;
        }
        // [핵심 수정] 타임아웃이 있는 대기 루프로 변경
        float waitTimeout = 1.0f;
        float waitTimer = 0f;
        // bool targetReached = false;
        while (waitTimer < waitTimeout)
        {
            float distance = Vector3.Distance(eef.position, targetObj.transform.position);
            Debug.Log("distance : " + distance);
            if (distance < 0.01f)
            {
                // targetReached = true;
                // Debug.Log($"[Executor] 갠트리 위치 잡기 완료. (소요 시간: {waitTimer:F2}초)");
                break;
            }
            waitTimer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 타임아웃 처리
        // if (!targetReached)
        // {
        //     Debug.LogError($"[Executor] 갠트리 위치 잡기 시간 초과! (10초). 목표 지점에 도달할 수 없습니다.");
        //     Debug.LogError($"  - 목표 위치: {targetObj.transform.position.ToString("F4")}");
        //     Debug.LogError($"  - 현재 EEF 위치: {eef.position.ToString("F4")}");
        //     Debug.LogError($"  - 남은 거리: {Vector3.Distance(eef.position, targetObj.transform.position):F4}m");
        //     Debug.LogError($"  - 현재 면: {currentPath.FaceIndex}, 자세: {currentPoseName}. 자세 또는 경로 시작점이 갠트리 이동 범위를 벗어났을 수 있습니다.");

        //     // 에러 발생 시 현재 모션 중단
        //     IsExecuting = false;
        //     yield break; // 코루틴 즉시 종료
        // }

        // Debug.Log("[Executor] 경로 추적 시작 전, 자세 고정용 JointValue Objective를 모두 제거합니다.");
        robotStartPose.RemoveAllJointValueObjectives();
        currentState = MotionState.AligningTCP;
        // Debug.Log($"[Executor] State: {currentState} - 6축 로봇팔 자세 정렬");
        jointController.EnableOnlyA1toA6();

        // 회전 오차가 10도 이내로 만족할 때까지 대기
        float tcpTimeout = 3.0f; // 최대 5초 대기
        float tcpElapsed = 0f;
        float rotThreshold = 2.0f; // 회전 오차 임계값(도)
        Quaternion targetRotation = targetObj.transform.rotation;

        while (tcpElapsed < tcpTimeout)
        {
            float rotErr = Quaternion.Angle(eef.rotation, targetRotation);
            if (rotErr < rotThreshold)
            {
                Debug.Log($"[Executor] TCP 정렬 완료 (오차: {rotErr:F2}도)");
                break;
            }
            tcpElapsed += Time.deltaTime;
            yield return null;
        }
        if (tcpElapsed >= tcpTimeout)
        {
            Debug.LogWarning($"[Executor] TCP 정렬 시간 초과 ({tcpTimeout}초). 마지막 오차: {Quaternion.Angle(eef.rotation, targetRotation):F2}도");
        }

        // === 4. 경로 실행 ===
        currentState = MotionState.ExecutingPath;
        Debug.Log($"[Executor] State: {currentState} - 경로 실행 시작");

        // 부드러운 경로 추종을 위해 '등속 모드'로 전환
        // SetMotionMode(0.0f);
        Vector3 targetPosition;
        jointController.EnableOnlyGauntry();
        float weavingAngle = GetWeavingAngleIfApplicable();
        bool isWeavingPath = weavingAngle > 0f;
        // int _count = currentPath.Positions.Count;
        for (currentTargetIndex = 0; currentTargetIndex < currentPath.PointList.Count; currentTargetIndex++)
        {
            // Debug.Assert(currentPath != null);
            // Debug.Assert(_count == currentPath.Positions.Count);
            // Debug.Assert(currentPath.Positions.Count < currentTargetIndex);

            // 1. 목표 설정
            targetPosition = currentPath.PointList[currentTargetIndex].position;
            targetRotation = Quaternion.Euler(currentPath.PointList[currentTargetIndex].eulerAngles);
            Debug.Assert(targetObj != null);
            targetObj.transform.SetPositionAndRotation(targetPosition, targetRotation);
            jointController.EnableOnlyGauntry();

            // 2. Gantry에 대한 IK 계산 (명령)
            // bioIK.SolveIK();
            // 3. 위치 도달 대기 (실행) - 별도의 코루틴 호출
            yield return coroutineRunner.StartCoroutine(
                WaitUntilStagnationOrTimeout(150.0f)
            );

            // 4. XML 저장
            if (isWeavingPath)
            {
                // 위빙 경로일 경우, 위빙 각도 정보를 함께 전달하여 기록
                xmlRecorder.RecordCurrentPose_weaving(this.coroutineRunner.GetComponent<BA_Main>().makeXmlInstance, weavingAngle, 44.0f, 44.0f);
            }
            else
            {
                // 일반 경로일 경우, 기존 방식으로 기록
                xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<BA_Main>().makeXmlInstance);
            }
            if (coroutineRunner is BA_Main mainScript && mainScript.simulationStatus != null)
            {
                mainScript.simulationStatus.OnPointCompleted();
            }
            // Debug.Log($"[Executor] XML 저장 완료: Point {currentTargetIndex + 1} / {currentPath.Positions.Count}");
            // 5. 다음 지점 전 한 프레임 대기
            yield return null;
        }

        // === 5. 완료 ===
        currentState = MotionState.Completed;
        Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex} 작업 완료");

        // 다음 작업을 위해 기본 '가속 모드'로 복원
        SetMotionMode(5.0f);
        IsExecuting = false;
        // xmlRecorder.FinalizeRecording(); // 남은 pose 저장
        // xmlRecorder.StartNewFile();      // 다음 면을 위해 새 파일 시작
        bioIK.autoIK = true;

        if (toggleObstacleAvoiding != null) toggleObstacleAvoiding.SetShapeDistanceObjectives(false);
        // jointController.EnableAllJoints();
    }

    private float GetWeavingAngleIfApplicable()
    {
        // 1. 현재 경로가 MotionPath_Weaving 타입인지 확인합니다.
        if (currentPath is BA_WeavingPath weavingPath)
        {
            // 2. 타입이 맞으면, 경로에 저장된 WeavingAngle 값을 반환합니다.
            Debug.Log($"<color=cyan>이 경로는 위빙 경로입니다. 저장된 위빙 각도: {weavingPath.WeavingAngle:F2}도</color>");
            return weavingPath.WeavingAngle;
        }

        // 위빙 경로가 아니면 0을 반환합니다.
        return 0f;
    }
    /// <summary>
    /// [수정됨] 위치 오차가 더 이상 개선되지 않거나, 타임아웃이 되면 종료합니다.
    /// 정체 상태로 종료될 때 현재 위치를 XML로 저장합니다.
    /// </summary>
    /// <param name="timeout">최대 대기 시간</param>
    public IEnumerator WaitUntilStagnationOrTimeout(float timeout)
    {
        float mainTimer = 0f;
        float stagnationTimer = 0f;
        float lastPosErr = float.MaxValue;
        const float stagnationThreshold = 0.5f;

        while (mainTimer < timeout)
        {
            Debug.Assert(eef != null);
            Debug.Assert(targetObj != null);
            float currentPosErr = Vector3.Distance(eef.position, targetObj.transform.position);

            // 1. 목표에 도달했으면 즉시 성공 종료
            if (currentPosErr < 0.005f)
            {
                float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
                // Debug.Log($"[Executor] 위치 도달 성공. 최종 회전 오차: {rotErr:F2}도");
                yield break;
            }

            // 2. 오차 정체 감지
            if (Mathf.Abs(lastPosErr - currentPosErr) < 0.0001f)
            {
                stagnationTimer += Time.deltaTime;
            }
            else
            {
                stagnationTimer = 0f;
            }

            lastPosErr = currentPosErr;

            // 3. 정체 시간이 임계값을 넘으면 IK가 멈춘 것으로 간주하고 강제 종료
            if (stagnationTimer > stagnationThreshold)
            {
                float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
                Debug.LogWarning($"[Executor] IK 이동이 정체되어 Point {currentTargetIndex + 1}을 건너뜁니다. (최종 위치 오차: {currentPosErr:F4}m, 회전 오차: {rotErr:F2}도)");

                // ======================= [핵심 수정 코드] =======================
                // 건너뛰기 직전, 현재 로봇의 마지막 상태를 XML로 저장합니다.
                // Debug.LogWarning("[Executor] 정체 상태의 마지막 위치를 XML로 저장합니다.");
                // xmlRecorder.RecordCurrentPose();
                // ===============================================================

                yield break; // 코루틴을 종료하고 다음 경로점으로 넘어갑니다.
            }

            mainTimer += Time.deltaTime;
            yield return null;
        }

        // 4. 전체 타임아웃 발생 시
        Debug.LogError($"[Executor] 위치 도달 전체 시간 초과! (Point {currentTargetIndex + 1})");
    }
    private void SwitchPathAngleAndApplyToFutureRotations(int startIndex)
    {
        // 현재 가로줄 번호가 짝수(0, 2, 4...)이면 120도, 홀수(1, 3, 5...)이면 60도를 목표 각도로 설정합니다.
        float newTargetAngleBps = (completedHorizontalRows % 2 == 0) ? 120f : 60f;

        // 이 메서드가 호출될 때마다, 즉시 전역 변수도 업데이트하여 현재 상태를 동기화합니다.
        currentFace7_AngleBps = newTargetAngleBps;

        // Debug.Log($"<color=cyan>[Executor] 각도 결정 실행: {completedHorizontalRows + 1}번째 줄의 목표 각도는 {currentFace7_AngleBps}도 입니다.</color>");

        Quaternion newTargetRotation = Quaternion.LookRotation(
            Quaternion.AngleAxis(currentFace7_AngleBps, currentPath.HeightDir) * currentPath.FaceNormal,
            currentPath.HeightDir
        );

        for (int i = startIndex; i < currentPath.PointList.Count; i++)
        {
            // currentPath.Rotations[i] = newTargetRotation;
            currentPath.PointList[i].eulerAngles = newTargetRotation.eulerAngles;
        }
    }
    private IEnumerator SolveIKPeriodicallyUntilPositionReached(float timeout)
    {
        float timer = 0f;
        int frameCount = 0; // 프레임 카운터

        while (timer < timeout)
        {
            // 3프레임마다 IK 계산 실행
            // if (frameCount % 10 == 0)
            // {
            //     // Debug.Log($"Frame {Time.frameCount}: Solving IK for point {currentTargetIndex + 1}");
            //     bioIK.SolveIK();
            // }

            // 매 프레임 위치 오차 확인
            float posErr = Vector3.Distance(eef.position, targetObj.transform.position);
            float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);

            if (currentPath.FaceIndex == 8)
            {
                posErr = Vector3.Distance(eef.position, targetObj.transform.position);
                rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
                Debug.Log($"[Executor-Debug] 7번 면 Point {currentTargetIndex + 1} 대기 중... PosErr: {posErr:F4}, RotErr: {rotErr:F2}");
            }
            if (posErr <= 0.01f)
            {
                rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
                Debug.Log($"[Executor] 위치 도달 완료. 최종 회전 오차: {rotErr:F2}도");
                yield break; // 성공적으로 코루틴 종료
            }

            timer += Time.deltaTime;
            frameCount++;
            yield return null; // 다음 프레임까지 대기
        }

        // 타임아웃 발생 시
        Debug.LogWarning($"[Executor] 위치 도달 시간 초과! (Point {currentTargetIndex + 1})");
    }
    // 타임아웃 기능이 있는 대기 코루틴 (헬퍼 함수)
    private IEnumerator WaitUntilCondition(System.Func<bool> condition, float timeout, string taskName)
    {
        float timer = 0f;
        while (!condition())
        {
            if (timer > timeout)
            {
                Debug.LogWarning($"[Executor] {taskName} 시간 초과! 다음 단계로 강제 진행합니다.");
                yield break; // Timeout 발생 시 코루틴 종료
            }
            timer += Time.deltaTime;
            yield return null; // 조건 만족까지 매 프레임 대기
        }
        Debug.Log($"[Executor] {taskName} 완료.");
    }
    /// <summary>
    /// (추가된 헬퍼 메소드) 면의 법선 벡터를 기반으로 최적의 Pan(Upbox) 각도를 계산합니다.
    /// </summary>
    private float CalculatePanAngleForNormal(Vector3 normal)
    {
        float[] angles = { 0f, 90f, 180f, -90f };
        Vector3[] xDirs = { Vector3.right, Vector3.back, Vector3.left, Vector3.forward };
        Vector3 normalXZ = new Vector3(normal.x, 0, normal.z).normalized;
        float minDot = float.PositiveInfinity;
        int bestIdx = 0;
        for (int i = 0; i < 4; i++)
        {
            float dot = Vector3.Dot(normalXZ, xDirs[i]);
            if (dot < minDot) { minDot = dot; bestIdx = i; }
        }
        return angles[bestIdx];
    }
    public void RotateUpboxDirectly(float angle)
    {

        GameObject upboxObj = GetUpboxGameObject();
        if (upboxObj != null)
        {
            upboxObj.transform.localRotation = Quaternion.Euler(0, 0, angle);

        }
        else
        {
            Debug.LogWarning("[Executor] upbox GameObject를 찾지 못했습니다.");
        }
    }

    public void RotatedownDirectly(float angle)
    {
        // down joint 오브젝트를 찾습니다.
        GameObject downObj = GetDownGameObject();
        if (downObj != null)
        {
            // 찾은 오브젝트를 x축 기준으로 회전시킵니다.
            downObj.transform.localRotation = Quaternion.Euler(angle, 0, 0);
            Debug.Log($"[Executor] down joint를 x축 기준으로 {angle}도 회전시켰습니다.");
        }
        else
        {
            Debug.LogWarning("[Executor] 'down'이라는 이름의 GameObject를 찾지 못했습니다.");
        }
    }
    public GameObject GetUpboxGameObject()
    {
        if (bioIK == null || bioIK.Segments == null)
            return null;

        foreach (var segment in bioIK.Segments)
        {
            if (segment.Joint != null && segment.Joint.gameObject.name == "upbox")
            {
                return segment.Joint.gameObject;
            }
        }
        return null;
    }
    public GameObject GetDownGameObject()
    {
        if (bioIK == null || bioIK.Segments == null)
            return null;

        foreach (var segment in bioIK.Segments)
        {
            // "down" 이라는 이름의 joint를 찾습니다.
            if (segment.Joint != null && segment.Joint.gameObject.name == "down")
            {
                return segment.Joint.gameObject;
            }
        }
        return null;
    }

}


//// // BlastingTask/RobotMotionExecutor.cs
//// using UnityEngine;
//// using System.Collections;
//// using BioIK;
//// using System.Linq;
//// using System.Collections.Generic;

//// /// <summary>
//// /// 로봇 모션 실행을 위한 기본 상태 머신을 관리하는 부모 클래스.
//// /// ICollisionHandler 인터페이스를 구현하며, 자식 클래스에서 로직을 재정의할 수 있습니다.
//// /// </summary>
//// public class RobotMotionExecutor : ICollisionHandler
//// {
////     // --- 자식 클래스에서 접근 가능하도록 protected로 변경 ---
////     protected readonly MonoBehaviour coroutineRunner;
////     protected readonly BioIK.BioIK bioIK;
////     protected readonly Transform eef;
////     protected readonly GameObject targetObj;
////     protected readonly Gauntry_or_6Joint_or_all jointController;
////     protected readonly RobotStartPose robotStartPose;
////     protected readonly XmlPathRecorder xmlRecorder;
////     protected readonly Toggle_Obstacle_Avoiding toggleObstacleAvoiding;

////     protected enum MotionState { Idle, Preparing, ExecutingPath, Completed }
////     protected MotionState currentState;

////     protected MotionPathGenerator.MotionPath currentPath;
////     protected string currentPoseName;
////     protected int currentTargetIndex;

////     public bool IsExecuting { get; protected set; }
////     protected Coroutine currentMotionCoroutine = null;

////     public RobotMotionExecutor(MonoBehaviour runner, BioIK.BioIK bioIK, GameObject targetObj, Transform eef, Gauntry_or_6Joint_or_all jointController, RobotStartPose robotStartPose, XmlPathRecorder xmlRecorder, Toggle_Obstacle_Avoiding toggleObstacleAvoiding)
////     {
////         this.coroutineRunner = runner;
////         this.bioIK = bioIK;
////         this.targetObj = targetObj;
////         this.eef = eef;
////         this.jointController = jointController;
////         this.robotStartPose = robotStartPose;
////         this.xmlRecorder = xmlRecorder;
////         this.toggleObstacleAvoiding = toggleObstacleAvoiding;
////         this.currentState = MotionState.Idle;
////     }

////     public virtual void StartMotion(MotionPathGenerator.MotionPath path, string poseName)
////     {
////         if (IsExecuting) return;
////         IsExecuting = true;
////         Debug.Log($"[Executor.StartMotion] 모션 시작. FaceIndex: {path.FaceIndex}");
////         this.currentPath = path;
////         this.currentPoseName = poseName;
////         currentMotionCoroutine = coroutineRunner.StartCoroutine(MotionStateMachine());
////     }

////     public void StopCurrentMotion()
////     {
////         if (currentMotionCoroutine != null)
////         {
////             coroutineRunner.StopCoroutine(currentMotionCoroutine);
////             currentMotionCoroutine = null;
////         }
////         IsExecuting = false;
////         currentState = MotionState.Idle;
////     }

////     public virtual void OnObstacleDetected(GameObject hitObject)
////     {
////         // 기본 클래스에서는 충돌을 처리하지 않음
////     }

////     protected virtual IEnumerator MotionStateMachine()
////     {
////         Debug.Log("<color=cyan>[Base Coroutine] MotionStateMachine 시작됨.</color>");
////         currentState = MotionState.Preparing;

////         jointController.EnableOnlyA1toA6();
////         bioIK.autoIK = false;
////         yield return coroutineRunner.StartCoroutine(robotStartPose.PosingSequenceCoroutine(this.currentPoseName, currentPath.FaceNormal));
////         bioIK.autoIK = true;
////         float panAngle = CalculatePanAngleForNormal(currentPath.FaceNormal);
////         RotatedownDirectly(0);
////         RotateUpboxDirectly(panAngle);
////         jointController.EnableOnlyGauntry();
////         yield return new WaitForSeconds(0.5f);
////         targetObj.transform.position = currentPath.Positions[0];
////         targetObj.transform.rotation = currentPath.Rotations[0];
////         yield return coroutineRunner.StartCoroutine(WaitUntilStagnationOrTimeout(150.0f));
////         jointController.EnableOnlyA1toA6();
////         yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 3.0f));

////         currentState = MotionState.ExecutingPath;
////         Debug.Log($"[Executor] State: {currentState} - 경로 실행 시작");
////         jointController.EnableOnlyGauntry();
////         for (currentTargetIndex = 0; currentTargetIndex < currentPath.Positions.Count; currentTargetIndex++)
////         {
////             targetObj.transform.SetPositionAndRotation(currentPath.Positions[currentTargetIndex], currentPath.Rotations[currentTargetIndex]);
////             yield return coroutineRunner.StartCoroutine(WaitUntilStagnationOrTimeout(150.0f));
////             xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
////             if (coroutineRunner is main mainScript && mainScript.simulationStatus != null)
////             {
////                 mainScript.simulationStatus.OnPointCompleted();
////             }
////             yield return null;
////         }

////         CompleteMotion();
////     }

////     protected void CompleteMotion()
////     {
////         currentState = MotionState.Completed;
////         Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex} 작업 완료");
////         IsExecuting = false;
////     }

////     protected IEnumerator WaitUntilStagnationOrTimeout(float timeout)
////     {
////         float mainTimer = 0f;
////         while (mainTimer < timeout)
////         {
////             float currentPosErr = Vector3.Distance(eef.position, targetObj.transform.position);
////             if (currentPosErr < 0.005f) yield break;
////             mainTimer += Time.deltaTime;
////             yield return null;
////         }
////     }

////     protected IEnumerator WaitUntilRotationAligned(float threshold, float timeout)
////     {
////         float timer = 0f;
////         while (timer < timeout)
////         {
////             float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
////             if (rotErr < threshold) yield break;
////             timer += Time.deltaTime;
////             yield return null;
////         }
////     }

////     protected float CalculatePanAngleForNormal(Vector3 normal)
////     {
////         float[] angles = { 0f, 90f, 180f, -90f };
////         Vector3[] xDirs = { Vector3.right, Vector3.back, Vector3.left, Vector3.forward };
////         Vector3 normalXZ = new Vector3(normal.x, 0, normal.z).normalized;
////         float minDot = float.PositiveInfinity;
////         int bestIdx = 0;
////         for (int i = 0; i < 4; i++)
////         {
////             float dot = Vector3.Dot(normalXZ, xDirs[i]);
////             if (dot < minDot) { minDot = dot; bestIdx = i; }
////         }
////         return angles[bestIdx];
////     }

////     protected void RotateUpboxDirectly(float angle)
////     {
////         GameObject upboxObj = GetUpboxGameObject(); 해결해줘
////         if (upboxObj != null) upboxObj.transform.localRotation = Quaternion.Euler(0, 0, angle);
////     }

////     protected void RotatedownDirectly(float angle)
////     {
////         GameObject downObj = GetDownGameObject();
////         if (downObj != null) downObj.transform.localRotation = Quaternion.Euler(angle, 0, 0);
////     }

////     protected GameObject GetUpboxGameObject()
////     {
////         return bioIK.Segments.FirstOrDefault(s => s.Joint != null && s.Joint.gameObject.name == "upbox")?.Joint.gameObject;
////     }

////     protected GameObject GetDownGameObject()
////     {
////         return bioIK.Segments.FirstOrDefault(s => s.Joint != null && s.Joint.gameObject.name == "down")?.Joint.gameObject;
////     }
//// }

//using UnityEngine;
//using System.Collections;
//using BioIK;
//using System.Linq; // OrderBy 사용을 위해 추가
//using System.Collections.Generic;
//using _SHI_BA;

///// <summary>
///// 생성된 경로를 따라 로봇을 움직이는 상태 머신을 관리하는 클래스.
///// 기존 Plane_Motion_Planning의 복잡한 모션 실행 및 보정 알고리즘을 그대로 보존합니다.
///// </summary>
//public class RobotMotionExecutor
//{
//    // --- 외부 참조 컴포넌트 ---
//    private readonly MonoBehaviour coroutineRunner;
//    private readonly BioIK.BioIK bioIK;
//    private BA_BioIK _bioIK
//    {
//        get
//        {
//            return bioIK as BA_BioIK;
//        }
//    }
//    private readonly Transform eef;
//    private readonly GameObject targetObj;
//    private readonly Gauntry_or_6Joint_or_all jointController;
//    private readonly RobotStartPose robotStartPose;
//    private readonly XmlPathRecorder xmlRecorder;
//    private readonly Toggle_Obstacle_Avoiding toggleObstacleAvoiding;

//    private LayerMask currentDetectionLayer;
//    private HashSet<GameObject> collidedObstaclesThisRow = new HashSet<GameObject>();

//    // --- 상태 관리 ---
//    private enum MotionState { Idle, Preparing, PositioningGantry, AligningTCP, ExecutingPath, Completed }
//    private MotionState currentState;
//    private MotionPathGenerator.MotionPath currentPath;
//    private string currentPoseName;
//    private int currentTargetIndex;
//    private bool isPoseCorrectionActive;
//    private bool isCurrentlyAvoiding = false; 
//    public bool IsExecuting { get; private set; }
//    private Coroutine currentMotionCoroutine = null; // 현재 실행 중인 코루틴 저장
//    private float currentFace7_AngleBps = 120f;     // 7번 면의 현재 각도 상태
//    private Dictionary<GameObject, int> processedParentObstacles = new Dictionary<GameObject, int>();

//    // private Dictionary<GameObject, bool> processedParentObstacles = new Dictionary<GameObject, bool>();
//    private bool wasMovingHorizontally = true; 
//    private bool isChangingRow = false; // <<< 이 라인을 추가합니다.


//    // private bool canCorrectPosture = true; // [신규 추가] 자세 교정 가능 여부 플래그 (자물쇠 역할)
//    private int completedHorizontalRows = 0; // [신규 추가] 완료된 가로줄 수를 세는 변수
//    // private bool postureCorrectionJustCompleted = false; // [신규 추가] 자세 교정 완료 플래그

//    /// <summary>
//    /// 생성자: 8개의 인자를 모두 받도록 수정되었습니다.
//    /// </summary>
//    public RobotMotionExecutor(MonoBehaviour runner, BioIK.BioIK bioIK, GameObject targetObj, Transform eef, Gauntry_or_6Joint_or_all jointController, RobotStartPose robotStartPose, XmlPathRecorder xmlRecorder, Toggle_Obstacle_Avoiding toggleObstacleAvoiding)
//    {
//        this.coroutineRunner = runner;
//        this.bioIK = bioIK;
//        this.targetObj = targetObj;
//        this.eef = eef;
//        this.jointController = jointController;
//        this.robotStartPose = robotStartPose;
//        this.xmlRecorder = xmlRecorder;
//        this.toggleObstacleAvoiding = toggleObstacleAvoiding; // toggleObstacleAvoiding 할당 추가
//        this.currentState = MotionState.Idle;
//    }
//    /// <summary>
//    /// [신규 추가] BioIK의 최대 가속도 값을 설정하여 모션 타입을 제어합니다.
//    /// 이 메서드는 RobotMotionExecutor 내부에서만 사용됩니다.
//    /// </summary>
//    /// <param name="maxAcceleration">새로운 최대 가속도 값. 0으로 설정 시 등속 모드로 동작합니다.</param>
//    private void SetMotionMode(float maxAcceleration)
//    {
//        if (this.bioIK == null) return;

//        // BioIK 컴포넌트의 MaximumAcceleration 값을 직접 변경합니다.
//        this.bioIK.MaximumAcceleration = maxAcceleration;

//        // if (maxAcceleration > 0)
//        // {
//        //     Debug.Log($"[Executor] 가속 모드 설정. MaxAcceleration: {maxAcceleration}");
//        // }
//        // else
//        // {
//        //     Debug.Log($"[Executor] 등속 모드 설정. MaxAcceleration: 0");
//        // }
//    }
//    /// <summary>
//    /// 새로운 경로로 모션 실행을 시작합니다.
//    /// </summary>
//    public void StartMotion(MotionPathGenerator.MotionPath path, string poseName)
//    {
//        if (IsExecuting) return;
//        Debug.Log($"[Executor.StartMotion] 모션 시작. FaceIndex: {path.FaceIndex}");
//        this.currentPath = path;
//        this.currentPoseName = poseName;
//        this.currentTargetIndex = 0;
//        this.IsExecuting = true;
//        this.isCurrentlyAvoiding = false; // <<< 이 라인을 추가하여 상태를 초기화합니다.

//        // 모든 상태 변수 초기화
//        collidedObstaclesThisRow.Clear();
//        processedParentObstacles.Clear();
//        completedHorizontalRows = 0;
//        wasMovingHorizontally = true; // 첫 시작은 수평으로 간주

//        if (path.FaceIndex == 8)
//        {
//            this.currentFace7_AngleBps = 120f; // 7번 면의 시작 각도는 항상 120도
//        }

//        // jointController를 통해 GantryController의 x_mover에 접근합니다.
//        if (jointController != null && jointController.gantryController != null)
//        {
//            var gantry = jointController.gantryController;
//            if (gantry.x_mover != null)
//            {
//                // [수정] IKMover의 속성 이름 'MaximumVelocity'를 'Speed'로 변경합니다.
//                if (path.FaceIndex == 8)
//                {
//                    // 7번 면일 경우 속도를 500 mm/s (0.5 m/s)로 설정
//                    gantry.x_mover.Speed = 1f; // MaximumVelocity -> Speed
//                    gantry.x_mover.AccelerationTime = 0.5f; // MaximumVelocity -> Speed
//                    gantry.y_mover.Speed = 100f; // MaximumVelocity -> Speed
//                    gantry.y_mover.AccelerationTime = 5f; // MaximumVelocity -> Speed
//                    gantry.z.Speed = 100f; // MaximumVelocity -> Speed
//                    gantry.z.AccelerationTime = 5f; // MaximumVelocity -> Speed

//                    Debug.Log($"[Executor] 7번 면 감지. X_Mover 속도를 {gantry.x_mover.Speed} m/s (500 mm/s)로 설정합니다.");
//                }
//                else
//                {
//                    // 그 외의 모든 면에서는 속도를 10000 mm/s (10 m/s)로 설정
//                    gantry.x_mover.Speed = 10000f; // MaximumV+elocity -> Speed
//                    gantry.x_mover.AccelerationTime = 500f; // MaximumVelocity -> Speed
//                    gantry.y_mover.Speed = 10000f; // MaximumVelocity -> Speed
//                    gantry.y_mover.AccelerationTime = 500f; // MaximumVelocity -> Speed
//                    gantry.z.Speed = 10000f; // MaximumVelocity -> Speed
//                    gantry.z.AccelerationTime = 500f; // MaximumVelocity -> Speed
//                    Debug.Log($"[Executor] 일반 면 감지. X_Mover 속도를 {gantry.x_mover.Speed} m/s (10000 mm/s)로 설정합니다.");
//                }
//            }
//            else
//            {
//                Debug.LogWarning("[Executor] GantryController의 x_mover가 할당되지 않았습니다.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("[Executor] JointController 또는 GantryController가 할당되지 않았습니다.");
//        }
//        currentMotionCoroutine = coroutineRunner.StartCoroutine(MotionStateMachine());
//    }

//    private bool CheckForCollision()
//    {
//        Vector3 origin = this.eef.position;
//        Vector3 direction = this.eef.right;
//        float distance = (float)this.currentPath.NormalOffset + 0.2f;

//        Debug.DrawRay(origin, direction * distance, Color.yellow);
//        Debug.Log($"DrawRay Info: Origin={origin.ToString("F3")}, Direction={direction.ToString("F3")}, Distance={distance:F2}");

//        // 저장해둔 currentDedistancetectionLayer를 사용합니다.
//        if (Physics.Raycast(origin, direction, distance, this.currentDetectionLayer))
//        {
//            return true;
//        }
//        return false;
//    }


//    // public void OnObstacleDetected(GameObject hitObject)
//    // {
//    //     if (isCurrentlyAvoiding) return;
//    //     // 1. 현재 실행 중인 경로가 7번 면이 아니면 무시 (기존 로직 유지)
//    //     if (!IsExecuting || currentPath.FaceIndex != 7) return;

//    //     // 2. 충돌한 오브젝트에서 ObstacleIdentifier 컴포넌트를 가져옵니다.
//    //     ObstacleIdentifier identifier = hitObject.GetComponent<ObstacleIdentifier>();
//    //     if (identifier == null)
//    //     {
//    //         // 식별자가 없는 장애물은 회피 로직을 처리하지 않습니다.
//    //         return;
//    //     }

//    //     // 3. ObstacleIdentifier의 정보를 사용하여 부모/자식 관계와 부모 오브젝트를 명확하게 결정합니다.
//    //     GameObject parentObstacle;
//    //     bool isChildObject;

//    //     if (identifier.type == ObstacleIdentifier.ObstacleType.Child)
//    //     {
//    //         isChildObject = true;
//    //         // 자식인 경우, Inspector에 연결된 부모 참조를 직접 사용합니다. (GameObject.Find 제거)
//    //         parentObstacle = identifier.parentObstacle;

//    //         // 부모가 할당되지 않은 경우에 대한 예외 처리
//    //         if (parentObstacle == null)
//    //         {
//    //             Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}'에 부모(parentObstacle)가 할당되지 않았습니다. 회피를 건너뜁니다.", hitObject);
//    //             return;
//    //         }
//    //     }
//    //     else // type == ObstacleIdentifier.ObstacleType.Parent
//    //     {
//    //         isChildObject = false;
//    //         // 부모인 경우, 자기 자신이 부모 오브젝트입니다.
//    //         parentObstacle = hitObject;
//    //     }

//    //     // 4. 명확하게 식별된 parentObstacle을 사용하여 기존 순서 보장 로직을 그대로 수행합니다.
//    //     if (isChildObject)
//    //     {
//    //         // 자식과 충돌했는데, 부모가 아직 처리되지 않았거나(Dictionary에 없거나), 
//    //         // 부모만 처리되고 자식은 아직일 때 (값이 false)
//    //         if (processedParentObstacles.ContainsKey(parentObstacle) && processedParentObstacles[parentObstacle] == false)
//    //         {
//    //             // 부모가 처리된 상태(false)일 때만 자식을 처리
//    //             if (processedParentObstacles.ContainsKey(parentObstacle))
//    //             {
//    //                 // 두 번째 각도 변경 (자식 충돌 시)
//    //                 PerformAngleSwitchAndPathModification(parentObstacle, "자식");
//    //                 processedParentObstacles[parentObstacle] = true; // 이제 이 부모-자식 쌍은 완전히 처리됨
//    //             }
//    //             // 부모가 아예 처리 안됐으면 자식 충돌은 무시
//    //         }
//    //     }
//    //     else // 부모와 충돌했을 때
//    //     {
//    //         if (!processedParentObstacles.ContainsKey(parentObstacle))
//    //         {
//    //             // 첫 번째 각도 변경 (부모 충돌 시)
//    //             PerformAngleSwitchAndPathModification(parentObstacle, "부모");
//    //             processedParentObstacles.Add(parentObstacle, false); // 부모는 처리됨, 자식은 아직(false)
//    //         }
//    //     }
//    // // }
//    // public void OnObstacleDetected(GameObject hitObject)
//    // {
//    //     // 이미 회피 동작이 진행 중인 경우, 중복 감지 및 처리 방지
//    //     if (isCurrentlyAvoiding) return;

//    //     // 현재 모션이 실행 중이 아니거나 7번 면 작업이 아닌 경우, 처리를 건너뜁니다.
//    //     if (!IsExecuting || currentPath == null || currentPath.FaceIndex != 7) return;

//    //     // 충돌한 오브젝트에서 ObstacleIdentifier 컴포넌트를 가져옵니다.
//    //     ObstacleIdentifier identifier = hitObject.GetComponent<ObstacleIdentifier>();
//    //     // ObstacleIdentifier가 없는 경우, 이 오브젝트는 장애물로 처리하지 않습니다.
//    //     if (identifier == null)
//    //     {
//    //         return;
//    //     }

//    //     GameObject parentObstacle; // 이 장애물의 부모 오브젝트 (논리적 그룹의 대표)
//    //     bool isChildObject;        // 이 장애물이 부모 그룹 내의 자식인지 여부

//    //     // 장애물 타입에 따라 부모 오브젝트를 결정합니다.
//    //     if (identifier.type == ObstacleIdentifier.ObstacleType.Child)
//    //     {
//    //         isChildObject = true;
//    //         parentObstacle = identifier.parentObstacle; // Inspector에서 할당된 부모를 사용

//    //         // 자식 장애물이지만 부모가 할당되지 않은 경우 오류를 기록하고 건너뜁니다.
//    //         if (parentObstacle == null)
//    //         {
//    //             Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}'에 부모(parentObstacle)가 할당되지 않았습니다. 회피를 건너뜁니다.", hitObject);
//    //             return;
//    //         }
//    //     }
//    //     else // ObstacleIdentifier.ObstacleType.Parent
//    //     {
//    //         isChildObject = false;
//    //         parentObstacle = hitObject; // 부모 장애물인 경우 자신이 그룹의 대표입니다.
//    //     }

//    //     // processedParentObstacles 딕셔너리에서 현재 부모 장애물 그룹의 트리거 횟수를 가져옵니다.
//    //     // 아직 딕셔너리에 없으면 기본값 0을 사용합니다.
//    //     int currentTriggerCount = processedParentObstacles.GetValueOrDefault(parentObstacle, 0);

//    //     // --- 사용자의 요청에 따른 핵심 로직 변경 ---
//    //     if (isChildObject)
//    //     {
//    //         // 1. 자식 장애물이 감지되었을 때:
//    //         //    부모 장애물 그룹이 정확히 2번 트리거되었고 (currentTriggerCount == 2),
//    //         //    아직 자식 장애물에 의한 회피가 트리거되지 않았다면 (processedParentObstacles[parentObstacle] != 3),
//    //         //    자식 장애물에 의한 회피 시퀀스를 트리거합니다.
//    //         if (currentTriggerCount == 2 && processedParentObstacles[parentObstacle] != 3)
//    //         {
//    //             Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 부모 '{parentObstacle.name}'가 2번 트리거됨({currentTriggerCount}회). 자식 회피 트리거.");
//    //             xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);

//    //             PerformAngleSwitchAndPathModification(parentObstacle, "자식");
//    //             // 자식 트리거가 완료되었음을 나타내기 위해 횟수를 3으로 설정합니다.
//    //             processedParentObstacles[parentObstacle] = 3; 
//    //         }
//    //         // 부모가 아직 2번 트리거되지 않았거나 (0 또는 1),
//    //         // 이미 자식까지 모두 트리거된 상태 (3)라면, 현재 자식 장애물 감지는 무시됩니다.
//    //         else if (currentTriggerCount < 2)
//    //         {
//    //             Debug.Log($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 하지만 부모 '{parentObstacle.name}'는 {currentTriggerCount}회만 트리거됨 (2회 필요). 현재 자식 트리거 무시.");
//    //         }
//    //         else if (currentTriggerCount >= 3) // 3 이상은 이미 자식 트리거가 발생했거나 그 이후
//    //         {
//    //             Debug.Log($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 이미 회피가 트리거되었습니다. 무시.");
//    //         }
//    //     }
//    //     else // 부모 장애물인 경우:
//    //     {
//    //         // 1. 부모 장애물이 처음 감지되었을 때 (currentTriggerCount == 0):
//    //         //    회피 시퀀스를 트리거하고, 트리거 횟수를 1로 설정합니다.
//    //         if (currentTriggerCount == 0)
//    //         {
//    //             Debug.LogWarning($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지 (1회차). 부모 회피 트리거.");
//    //             xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);

//    //             PerformAngleSwitchAndPathModification(parentObstacle, "부모");
//    //             processedParentObstacles[parentObstacle] = 1; // 호출 횟수 1로 설정
//    //         }
//    //         // 2. 부모 장애물이 두 번째로 감지되었을 때 (currentTriggerCount == 1):
//    //         //    회피 시퀀스를 *트리거하지 않고*, 단순히 인식되었음을 기록하고 횟수만 2로 증가시킵니다.
//    //         //    (이 부분에서 ToggleAngleForCollision()이 호출되지 않게 됩니다.)
//    //         else if (currentTriggerCount == 1)
//    //         {
//    //             Debug.LogWarning($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지 (2회차). 이번에는 회피를 트리거하지 않고 인식만 합니다.");
//    //             xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);

//    //             processedParentObstacles[parentObstacle] = 2; // 호출 횟수 2로만 설정
//    //         }
//    //         // 3. 부모 장애물이 이미 2번 이상 감지된 경우 (자식 트리거 이후 포함):
//    //         //    더 이상 회피 시퀀스를 트리거하지 않고 무시합니다.
//    //         else 
//    //         {
//    //             Debug.Log($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지. 이미 {currentTriggerCount}회 트리거되었습니다. 무시.");
//    //         }
//    //     }
//    // }
//    public void OnObstacleDetected(GameObject hitObject)
//    {
//        // 이미 회피 동작이 진행 중인 경우, 중복 감지 및 처리 방지
//        if (isCurrentlyAvoiding) return;

//        // 현재 모션이 실행 중이 아니거나 7번 면 작업이 아닌 경우, 처리를 건너뜁니다.
//        if (!IsExecuting || currentPath == null || currentPath.FaceIndex != 8) return;

//        // 충돌한 오브젝트에서 ObstacleIdentifier 컴포넌트를 가져옵니다.
//        ObstacleIdentifier identifier = hitObject.GetComponent<ObstacleIdentifier>();
//        // ObstacleIdentifier가 없는 경우, 이 오브젝트는 장애물로 처리하지 않습니다.
//        if (identifier == null)
//        {
//            return;
//        }

//        GameObject parentObstacle; // 이 장애물의 부모 오브젝트 (논리적 그룹의 대표)
//        bool isChildObject;        // 이 장애물이 부모 그룹 내의 자식인지 여부

//        // 장애물 타입에 따라 부모 오브젝트를 결정합니다.
//        if (identifier.type == ObstacleIdentifier.ObstacleType.Child)
//        {
//            isChildObject = true;
//            parentObstacle = identifier.parentObstacle; // Inspector에서 할당된 부모를 사용

//            // 자식 장애물이지만 부모가 할당되지 않은 경우 오류를 기록하고 건너뜁니다.
//            if (parentObstacle == null)
//            {
//                Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}'에 부모(parentObstacle)가 할당되지 않았습니다. 회피를 건너뜁니다.", hitObject);
//                return;
//            }
//        }
//        else // ObstacleIdentifier.ObstacleType.Parent
//        {
//            isChildObject = false;
//            parentObstacle = hitObject; // 부모 장애물인 경우 자신이 그룹의 대표입니다.
//        }

//        // processedParentObstacles 딕셔너리에서 현재 부모 장애물 그룹의 트리거 횟수를 가져옵니다.
//        // 아직 딕셔너리에 없으면 기본값 0을 사용합니다.
//        int currentTriggerCount = processedParentObstacles.GetValueOrDefault(parentObstacle, 0);

//        // --- 핵심 로직 변경 ---
//        if (isChildObject)
//        {
//            // 자식 장애물이 감지되었을 때:
//            // 부모 장애물 그룹이 정확히 2번 트리거되었고 (currentTriggerCount == 2),
//            // 아직 자식 장애물에 의한 회피가 트리거되지 않았다면 (processedParentObstacles[parentObstacle] != 3),
//            // 자식 장애물에 의한 회피 시퀀스를 트리거합니다.
//            if (currentTriggerCount == 2 && processedParentObstacles.GetValueOrDefault(parentObstacle, 0) != 3)
//            {
//                Debug.LogWarning($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 부모 '{parentObstacle.name}'가 2번 트리거됨({currentTriggerCount}회). 자식 회피 트리거.");
//                // 자식 트리거는 '3' 단계로 전달합니다.
//                PerformAngleSwitchAndPathModification(parentObstacle, "자식", 3); 
//                processedParentObstacles[parentObstacle] = 3; // 자식 트리거 완료로 표시 (총 3회 트리거)
//            }
//            // 부모가 아직 2번 트리거되지 않았거나 (0 또는 1),
//            // 이미 자식까지 모두 트리거된 상태 (3)라면, 현재 자식 장애물 감지는 무시됩니다.
//            else if (currentTriggerCount < 2)
//            {
//                Debug.Log($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 하지만 부모 '{parentObstacle.name}'는 {currentTriggerCount}회만 트리거됨 (2회 필요). 현재 자식 트리거 무시.");
//            }
//            else if (currentTriggerCount >= 3)
//            {
//                Debug.Log($"[Executor] 자식 장애물 '{hitObject.name}' 감지. 이미 회피가 트리거되었습니다. 무시.");
//            }
//        }
//        else // 부모 장애물인 경우:
//        {
//            // 부모 장애물은 최대 2번까지 'PerformAngleSwitchAndPathModification'를 트리거합니다.
//            // (이전에 부모 2회차에서 PerformAngleSwitchAndPathModification 호출을 건너뛰던 로직이 변경됨)
//            if (currentTriggerCount < 2)
//            {
//                Debug.LogWarning($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지 ({currentTriggerCount + 1}회 트리거). 부모 회피 트리거.");
//                // 부모의 현재 트리거 횟수를 'triggerStage'로 전달 (1회차는 1, 2회차는 2)
//                PerformAngleSwitchAndPathModification(parentObstacle, "부모", currentTriggerCount + 1); 
//                processedParentObstacles[parentObstacle] = currentTriggerCount + 1; // 부모의 트리거 횟수 1 증가
//            }
//            else // 부모 장애물이 이미 2번 트리거된 경우
//            {
//                Debug.Log($"[Executor] 부모 장애물 '{parentObstacle.name}' 감지. 이미 2번 트리거되었습니다. 무시.");
//            }
//        }
//    }
//    // private void PerformAngleSwitchAndPathModification(GameObject parentObstacle, string type)
//    // {
//    //     isCurrentlyAvoiding = true;

//    //     // [수정] 이 메서드에서는 각도를 직접 변경하거나 계산하지 않습니다.
//    //     // 단순히 충돌 발생 사실만 로그로 남기고, 코루틴 중단 및 시작 역할만 수행합니다.
//    //     // Debug.LogWarning($"[Executor] '{parentObstacle.name}'의 {type} 충돌! 회피 동작을 시작합니다.");

//    //     StopCurrentMotion();

//    //     // [수정] 이제 CollisionResponseSequence_Face7이 스스로 각도를 결정하므로 파라미터 없이 호출합니다.
//    //     currentMotionCoroutine = coroutineRunner.StartCoroutine(CollisionResponseSequence_Face7());
//    // }
//    private void PerformAngleSwitchAndPathModification(GameObject parentObstacle, string type, int triggerStage)
//    {
//        isCurrentlyAvoiding = true;

//        StopCurrentMotion();

//        // triggerStage를 CollisionResponseSequence_Face7 코루틴에 전달합니다.
//        currentMotionCoroutine = coroutineRunner.StartCoroutine(CollisionResponseSequence_Face7(triggerStage));
//    }
//    private IEnumerator CorrectPostureAtNewRow(int newRowStartIndex)
//    {
//        Debug.Log("<color=cyan>[Coroutine Start] CorrectPostureAtNewRow 시작됨.</color>");

//        collidedObstaclesThisRow.Clear();
//        processedParentObstacles.Clear();
//        // 이 코루틴은 이제 카운터를 직접 증가시키지 않습니다.
//        // Debug.LogWarning($"[Executor] {completedHorizontalRows + 1}번째 줄 진입. 자세 교정을 시작합니다.");

//        // 줄 바꿈 전용 각도 결정 메서드를 호출합니다.
//        if (currentPath.FaceIndex == 8)
//        {
//            SwitchPathAngleAndApplyToFutureRotations(newRowStartIndex);
//        }

//        BioIK.BioSegment tcpSegment = bioIK.FindSegment(this.eef);
//        double originalPositionWeight = 1.0;
//        double originalOrientationWeight = 1.0;

//        try
//        {
//            if (tcpSegment != null)
//            {
//                // Debug.Log("[Executor] TCP 가중치 조절: Position(0), Orientation(10)");
//                foreach (var objective in tcpSegment.Objectives)
//                {
//                    if (objective is BioIK.Position p) { originalPositionWeight = p.Weight; p.Weight = 0.0; }
//                    else if (objective is BioIK.Orientation o) { originalOrientationWeight = o.Weight; o.Weight = 10.0; }
//                }
//            }

//            targetObj.transform.rotation = currentPath.Rotations[newRowStartIndex];
//            jointController.EnableOnlyA5A6();

//            yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 3.0f));
//            Debug.LogWarning("[Executor] 줄 바꿈 자세 교정 완료. 현재 상태를 XML로 기록합니다.");
//            xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
//            // xmlRecorder.RecordCurrentPose();
//        }
//        finally
//        {
//            if (tcpSegment != null)
//            {
//                // Debug.Log("[Executor] TCP 가중치를 원래대로 복원합니다.");
//                foreach (var objective in tcpSegment.Objectives)
//                {
//                    if (objective is BioIK.Position p) { p.Weight = originalPositionWeight; }
//                    else if (objective is BioIK.Orientation o) { o.Weight = originalOrientationWeight; }
//                }
//            }
//        }

//        jointController.EnableOnlyGauntry();
//        // Debug.Log("[Executor] 자세 교정 완료. 갠트리 이동을 재개합니다.");
//        Debug.Log("<color=red>[Coroutine End] CorrectPostureAtNewRow 정상 종료.</color>");

//    }

//    private IEnumerator ResumeMotionFrom(int startIndex)
//    {
//        // 코루틴 시작 시, 현재 상태를 기준으로 wasMovingHorizontally 플래그를 정확히 초기화합니다.
//        if (startIndex > 0 && startIndex < currentPath.Positions.Count)
//        {
//            Vector3 initialDirection = (currentPath.Positions[startIndex] - currentPath.Positions[startIndex - 1]).normalized;
//            wasMovingHorizontally = Mathf.Abs(Vector3.Dot(initialDirection, currentPath.WidthDir)) > 0.9f;
//        }
//        else
//        {
//            wasMovingHorizontally = true;
//        }

//        for (int i = startIndex; i < currentPath.Positions.Count; i++)
//        {
//            currentTargetIndex = i;

//            if (i > 0)
//            {
//                Vector3 currentDirection = (currentPath.Positions[i] - currentPath.Positions[i - 1]).normalized;
//                bool isNowMovingHorizontally = Mathf.Abs(Vector3.Dot(currentDirection, currentPath.WidthDir)) > 0.9f;

//                // 이벤트 1: 수평 이동이 끝나고, 수직 이동(줄 바꿈)이 시작될 때
//                if (wasMovingHorizontally && !isNowMovingHorizontally)
//                {
//                    completedHorizontalRows++;
//                    Debug.Log($"<color=lime>--- {completedHorizontalRows}번째 가로줄 완료 ---</color>");
//                    collidedObstaclesThisRow.Clear();
//                    processedParentObstacles.Clear();

//                    // "줄 바꾸는 중" 상태로 전환 (잠금 설정)
//                    if (!isChangingRow) // <<< 이미 바꾸는 중이 아닐 때만 true로 설정
//                    {
//                        isChangingRow = true;
//                    }
//                }

//                // 이벤트 2: 수직 이동이 끝나고, 새로운 수평 이동이 시작될 때
//                if (!wasMovingHorizontally && isNowMovingHorizontally)
//                {
//                    // "줄 바꾸는 중" 상태일 때만 자세 교정 코루틴을 실행합니다.
//                    if (isChangingRow && currentPath.FaceIndex == 8 && completedHorizontalRows >= 1)
//                    {
//                        yield return coroutineRunner.StartCoroutine(CorrectPostureAtNewRow(i));

//                        // <<< 핵심 수정: 자세 교정 코루틴이 완전히 끝난 후에 잠금을 해제합니다. >>>
//                        isChangingRow = false;
//                    }
//                }
//                wasMovingHorizontally = isNowMovingHorizontally;
//            }

//            // XML 저장 및 경로 이동 로직
//            targetObj.transform.SetPositionAndRotation(currentPath.Positions[i], currentPath.Rotations[i]);
//            yield return coroutineRunner.StartCoroutine(WaitUntilStagnationOrTimeout(150.0f));
//            xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
//            Debug.Log("<color=red>[Coroutine End] MotionStateMachine 정상 종료.</color>");

//            if (coroutineRunner is main mainScript && mainScript.simulationStatus != null)
//            {
//                mainScript.simulationStatus.OnPointCompleted();
//            }
//            yield return null;
//        }

//        CompleteMotion();

//    }
//    // private IEnumerator PrepareAndAlign()
//    // {
//    //     currentState = MotionState.Preparing;
//    //     Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex}, 자세 '{currentPoseName}' 준비 시작");
//    //     jointController.EnableOnlyA1toA6();
//    //     bioIK.autoIK = false;
//    //     yield return coroutineRunner.StartCoroutine(robotStartPose.PosingSequenceCoroutine(this.currentPoseName, currentPath.FaceNormal));
//    //     bioIK.autoIK = true;
//    //     float panAngle = CalculatePanAngleForNormal(currentPath.FaceNormal);
//    //     if (currentPath.FaceIndex == 7) panAngle = -90f;
//    //     RotateUpboxDirectly(panAngle);
//    //     currentState = MotionState.PositioningGantry;
//    //     jointController.EnableOnlyGauntry();
//    //     yield return new WaitForSeconds(0.5f);
//    //     targetObj.transform.position = currentPath.Positions[0];
//    //     targetObj.transform.rotation = currentPath.Rotations[0];
//    //     yield return coroutineRunner.StartCoroutine(WaitUntilStagnationOrTimeout(150.0f));
//    //     currentState = MotionState.AligningTCP;
//    //     jointController.EnableOnlyA1toA6();
//    //     yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 3.0f));
//    //     currentState = MotionState.ExecutingPath;
//    // }
//    private void CompleteMotion()
//    {
//        Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex} 작업 완료");

//        SetMotionMode(5.0f);
//        IsExecuting = false;
//        isCurrentlyAvoiding = false; // <<< 이 라인을 추가하여 상태를 리셋합니다.
//        _bioIK.autoIK = true;
//        if (toggleObstacleAvoiding != null) toggleObstacleAvoiding.SetShapeDistanceObjectives(false);
//    }

//    // private IEnumerator CollisionResponseSequence_Face7()
//    // {
//    //     Debug.Log("<color=cyan>[Coroutine Start] CollisionResponseSequence_Face7 시작됨.</color>");

//    //     IsExecuting = true;

//    //     // 충돌 회피 전용 각도 토글 메서드를 호출합니다.
//    //     ToggleAngleForCollision();

//    //     BioIK.BioSegment tcpSegment = bioIK.FindSegment(this.eef);
//    //     double originalPositionWeight = 1.0;
//    //     double originalOrientationWeight = 1.0;

//    //     try
//    //     {
//    //         if (tcpSegment != null)
//    //         {
//    //             // Debug.Log("[Executor_Face7] TCP 가중치 조절: Position(0), Orientation(10)");
//    //             foreach (var objective in tcpSegment.Objectives)
//    //             {
//    //                 if (objective is BioIK.Position p) { originalPositionWeight = p.Weight; p.Weight = 0.0; }
//    //                 else if (objective is BioIK.Orientation o) { originalOrientationWeight = o.Weight; o.Weight = 10.0; }
//    //             }
//    //         }

//    //         targetObj.transform.rotation = currentPath.Rotations[currentTargetIndex];
//    //         jointController.EnableOnlyA5A6();

//    //         // Debug.Log("[Executor_Face7] 갠트리 멈춤. TCP 자세 교정 중...");
//    //         yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 2.0f));
//    //         // Debug.LogWarning("[Executor] 충돌 회피 회전 완료. 현재 상태를 XML로 기록합니다.");
//    //         // xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
//    //     }
//    //     finally
//    //     {
//    //         if (tcpSegment != null)
//    //         {
//    //             // Debug.Log("[Executor_Face7] TCP Objective 가중치를 원래대로 복원합니다.");
//    //             foreach (var objective in tcpSegment.Objectives)
//    //             {
//    //                 if (objective is BioIK.Position p) { p.Weight = originalPositionWeight; }
//    //                 else if (objective is BioIK.Orientation o) { o.Weight = originalOrientationWeight; }
//    //             }
//    //         }
//    //     }
//    //     // <<< 아래 2줄의 코드를 추가합니다. >>>
//    //     isCurrentlyAvoiding = false;
//    //     // Debug.LogWarning("[Executor_Face7] TCP 자세 교정 완료. 이제 다음 충돌 감지가 가능합니다.");

//    //     // xmlRecorder.RecordCurrentPose();
//    //     jointController.EnableOnlyGauntry();
//    //     // Debug.Log("[Executor_Face7] 갠트리 이동 재개. 남은 경로를 계속 진행합니다.");

//    //     // isCurrentlyAvoiding = false;
//    //     // Debug.LogWarning("[Executor_Face7] 충돌 회피 동작 완료. 다음 충돌 감지를 활성화합니다.");

//    //     yield return coroutineRunner.StartCoroutine(ResumeMotionFrom(currentTargetIndex));
//    //     Debug.Log("<color=red>[Coroutine End] CollisionResponseSequence_Face7 정상 종료.</color>");

//    // }
//    private IEnumerator CollisionResponseSequence_Face7(int triggerStage)
//    {
//        Debug.Log("<color=cyan>[Coroutine Start] CollisionResponseSequence_Face7 시작됨.</color>");
//        IsExecuting = true;

//        // --- 요청 1, 2, 3: 부모 1회차, 부모 2회차, 자식 1회차 충돌 '순간' RecordCurrentPose 호출 ---
//        // 이 부분은 모든 트리거 단계에서 항상 실행됩니다.
//        xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
//        Debug.Log($"[Face7 Response] RecordCurrentPose 호출됨 (충돌 순간). (트리거 단계: {triggerStage})");

//        // --- ToggleAngleForCollision() 및 로봇 회전 동작 조건부 실행 ---
//        // 요청 1: 부모 1회차 -> ToggleAngleForCollision() 호출
//        // 요청 3: 자식 1회차 -> ToggleAngleForCollision() 호출
//        if (triggerStage == 1 || triggerStage == 3)
//        {
//            ToggleAngleForCollision();
//            Debug.Log($"[Face7 Response] ToggleAngleForCollision 호출됨. (트리거 단계: {triggerStage})");

//            BioIK.BioSegment tcpSegment = bioIK.FindSegment(this.eef);
//            double originalPositionWeight = 1.0;
//            double originalOrientationWeight = 1.0;

//            try
//            {
//                // TCP Objective 가중치 조절 (회전 집중)
//                if (tcpSegment != null)
//                {
//                    foreach (var objective in tcpSegment.Objectives)
//                    {
//                        if (objective is BioIK.Position p) { originalPositionWeight = p.Weight; p.Weight = 0.0; }
//                        else if (objective is BioIK.Orientation o) { originalOrientationWeight = o.Weight; o.Weight = 10.0; }
//                    }
//                }

//                // 요청 4: 자식 1회차 충돌 시 EnableOnlyA5A6 호출 (부모 1회차에도 동일 적용)
//                jointController.EnableOnlyA5A6();

//                targetObj.transform.rotation = currentPath.Rotations[currentTargetIndex];
//                // 회전 완료까지 대기
//                yield return coroutineRunner.StartCoroutine(WaitUntilRotationAligned(5.0f, 2.0f));
//                Debug.LogWarning($"[Executor] 충돌 회피 회전 완료. (트리거 단계: {triggerStage})");

//                // 요청 4: 자식 1회차 충돌 후 회전이 끝났을 때 RecordCurrentPose 호출
//                if (triggerStage == 3) // 자식 1회차인 경우에만 추가 호출
//                {
//                    // xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
//                    Debug.Log($"[Face7 Response] RecordCurrentPose 호출됨 (회전 완료 후). (트리거 단계: {triggerStage})");
//                }
//            }
//            finally
//            {
//                // TCP Objective 가중치 원래대로 복원
//                if (tcpSegment != null)
//                {
//                    foreach (var objective in tcpSegment.Objectives)
//                    {
//                        if (objective is BioIK.Position p) { p.Weight = originalPositionWeight; }
//                        else if (objective is BioIK.Orientation o) { o.Weight = originalOrientationWeight; }
//                    }
//                }
//            }
//        }
//        // 요청 2: 부모 2회차 -> ToggleAngleForCollision() 호출 건너뜀
//        else if (triggerStage == 2)
//        {
//            Debug.Log($"[Face7 Response] 부모 2회차 감지. ToggleAngleForCollision 호출 및 회피 동작 건너뜀. (트리거 단계: {triggerStage})");
//            // 이 경우, 회피 동작이나 회전 대기 없이 바로 다음 단계로 넘어갑니다.
//        }

//        isCurrentlyAvoiding = false;
//        jointController.EnableOnlyGauntry(); // 갠트리 이동 재개
//        // 남은 경로를 계속 진행
//        yield return coroutineRunner.StartCoroutine(ResumeMotionFrom(currentTargetIndex));
//        Debug.Log("<color=red>[Coroutine End] CollisionResponseSequence_Face7 정상 종료.</color>");
//    }
//    /// <summary>
//    /// [신규 추가] 충돌 회피 시, 현재 각도를 즉시 반대 각도로 토글하고 경로에 적용합니다.
//    /// </summary>
//    private void ToggleAngleForCollision()
//    {
//        // 현재 각도를 기준으로 즉시 반대 각도로 토글합니다.
//        currentFace7_AngleBps = Mathf.Approximately(currentFace7_AngleBps, 120f) ? 60f : 120f;
//        // Debug.Log($"<color=red>[Executor] 충돌 회피! 새로운 목표 각도는 {currentFace7_AngleBps}도 입니다.</color>");

//        Quaternion newTargetRotation = Quaternion.LookRotation(
//            Quaternion.AngleAxis(currentFace7_AngleBps, currentPath.HeightDir) * currentPath.FaceNormal,
//            currentPath.HeightDir
//        );

//        // 경로의 남은 부분에 새로운 회전값을 적용합니다.
//        for (int i = currentTargetIndex; i < currentPath.Rotations.Count; i++)
//        {
//            currentPath.Rotations[i] = newTargetRotation;
//        }
//    }
//    public void StopCurrentMotion()
//    {
//        if (currentMotionCoroutine != null)
//        {
//            coroutineRunner.StopCoroutine(currentMotionCoroutine);
//        }
//        // IsExecuting 플래그를 false로 설정하여, 현재 어떤 모션도 실행 중이 아님을 명시합니다.
//        IsExecuting = false;
//        // Debug.Log("[Executor] 현재 진행 중인 모션 코루틴을 중단했습니다.");
//    }

//    public IEnumerator WaitUntilRotationAligned(float threshold, float timeout)
//    {
//        float timer = 0f;
//        while (timer < timeout)
//        {
//            float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);

//            // ▼▼▼ [ 여기에 디버깅 로그 추가 ] ▼▼▼
//            // 매 프레임 현재 회전 오차를 Console 창에 출력합니다.
//            // Debug.Log($"[TCP 재정렬 중] 현재 회전 오차: {rotErr:F2}도 (목표: {threshold}도 이내)");
//            // ▲▲▲ [ 추가 끝 ] ▲▲▲

//            if (rotErr < threshold)
//            {
//                // 목표 오차 이내로 들어왔으므로 루프 종료
//                yield break;
//            }
//            timer += Time.deltaTime;
//            yield return null;
//        }
//        Debug.LogWarning($"[Executor] TCP 회전 정렬 시간 초과 (오차: {Quaternion.Angle(eef.rotation, targetObj.transform.rotation):F2}도)");
//    }
//    private IEnumerator MotionStateMachine()
//    {
//        Debug.Log("<color=cyan>[Coroutine Start] MotionStateMachine 시작됨.</color>");

//        // === 1. 준비 및 자세 잡기 ===
//        currentState = MotionState.Preparing;
//        // Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex}, 자세 '{currentPoseName}' 준비 시작");

//        // 안정적인 자세 확보를 위해 '가속 모드' 활성화
//        jointController.EnableOnlyA1toA6();
//        _bioIK.autoIK = false;

//        // SetMainTcpObjectives(false);

//        SetMotionMode(2.0f);
//        // jointController.EnableOnlyA1toA6();

//        // [핵심 수정] RobotStartPose의 코루틴을 직접 받고, 해당 코루틴이 끝날 때까지 기다립니다.
//        // Debug.Log("[Executor] RobotStartPose의 자세 잡기 시퀀스를 시작하고 완료될 때까지 대기합니다...");
//        yield return coroutineRunner.StartCoroutine(robotStartPose.PosingSequenceCoroutine(this.currentPoseName, currentPath.FaceNormal));
//        // Debug.Log("[Executor] RobotStartPose 시퀀스 완료됨. 다음 단계로 진행합니다.");
//        // SetMainTcpObjectives(true);

//        // IK 재활성화 및 upbox 최종 회전
//        _bioIK.autoIK = true;
//        float panAngle = CalculatePanAngleForNormal(currentPath.FaceNormal);
//        if (currentPath.FaceIndex == 8)
//        {
//            float panAngle1 = -90f;
//            float panAngle2 = -60f;
//            RotateUpboxDirectly(-panAngle1);
//            RotatedownDirectly(-panAngle2);

//            // Debug.LogWarning($"[Executor] 면 7번이므로 PanAngle을 -90도로 강제 설정합니다.");
//        }
//        else
//        {
//            RotatedownDirectly(0);
//            RotateUpboxDirectly(panAngle);
//        }

//        // === 2. 갠트리 위치 잡기 ===

//        currentState = MotionState.PositioningGantry;
//        // Debug.Log($"[Executor] State: {currentState} - 갠트리 시작점으로 이동");
//        jointController.EnableOnlyGauntry();

//        // === [수정] IK 포즈가 실제로 반영될 때까지 충분히 대기 ===
//        yield return new WaitForSeconds(0.5f); // 또는 여러 프레임 yield return null;

//        // 이제 경로 첫 점으로 이동
//        targetObj.transform.position = currentPath.Positions[currentTargetIndex];
//        targetObj.transform.rotation = currentPath.Rotations[currentTargetIndex];
//        yield return new WaitUntil(() => Vector3.Distance(eef.position, targetObj.transform.position) < 0.1f);
//        // [핵심 수정] 타임아웃이 있는 대기 루프로 변경
//        float waitTimeout = 1.0f;
//        float waitTimer = 0f;
//        // bool targetReached = false;
//        while (waitTimer < waitTimeout)
//        {
//            float distance = Vector3.Distance(eef.position, targetObj.transform.position);
//            if (distance < 0.01f)
//            {
//                // targetReached = true;
//                // Debug.Log($"[Executor] 갠트리 위치 잡기 완료. (소요 시간: {waitTimer:F2}초)");
//                break;
//            }
//            waitTimer += Time.deltaTime;
//            yield return null; // 다음 프레임까지 대기
//        }

//        // 타임아웃 처리
//        // if (!targetReached)
//        // {
//        //     Debug.LogError($"[Executor] 갠트리 위치 잡기 시간 초과! (10초). 목표 지점에 도달할 수 없습니다.");
//        //     Debug.LogError($"  - 목표 위치: {targetObj.transform.position.ToString("F4")}");
//        //     Debug.LogError($"  - 현재 EEF 위치: {eef.position.ToString("F4")}");
//        //     Debug.LogError($"  - 남은 거리: {Vector3.Distance(eef.position, targetObj.transform.position):F4}m");
//        //     Debug.LogError($"  - 현재 면: {currentPath.FaceIndex}, 자세: {currentPoseName}. 자세 또는 경로 시작점이 갠트리 이동 범위를 벗어났을 수 있습니다.");

//        //     // 에러 발생 시 현재 모션 중단
//        //     IsExecuting = false;
//        //     yield break; // 코루틴 즉시 종료
//        // }

//        // Debug.Log("[Executor] 경로 추적 시작 전, 자세 고정용 JointValue Objective를 모두 제거합니다.");
//        robotStartPose.RemoveAllJointValueObjectives();
//        currentState = MotionState.AligningTCP;
//        // Debug.Log($"[Executor] State: {currentState} - 6축 로봇팔 자세 정렬");
//        jointController.EnableOnlyA1toA6();

//        // 회전 오차가 10도 이내로 만족할 때까지 대기
//        float tcpTimeout = 3.0f; // 최대 5초 대기
//        float tcpElapsed = 0f;
//        float rotThreshold = 2.0f; // 회전 오차 임계값(도)
//        Quaternion targetRotation = targetObj.transform.rotation;

//        while (tcpElapsed < tcpTimeout)
//        {
//            float rotErr = Quaternion.Angle(eef.rotation, targetRotation);
//            if (rotErr < rotThreshold)
//            {
//                Debug.Log($"[Executor] TCP 정렬 완료 (오차: {rotErr:F2}도)");
//                break;
//            }
//            tcpElapsed += Time.deltaTime;
//            yield return null;
//        }
//        if (tcpElapsed >= tcpTimeout)
//        {
//            Debug.LogWarning($"[Executor] TCP 정렬 시간 초과 ({tcpTimeout}초). 마지막 오차: {Quaternion.Angle(eef.rotation, targetRotation):F2}도");
//        }

//        // === 4. 경로 실행 ===
//        currentState = MotionState.ExecutingPath;
//        Debug.Log($"[Executor] State: {currentState} - 경로 실행 시작");

//        // 부드러운 경로 추종을 위해 '등속 모드'로 전환
//        // SetMotionMode(0.0f);
//        Vector3 targetPosition;
//        jointController.EnableOnlyGauntry();
//        float weavingAngle = GetWeavingAngleIfApplicable();
//        bool isWeavingPath = weavingAngle > 0f;
//        // int _count = currentPath.Positions.Count;
//        for (currentTargetIndex = 0; currentTargetIndex < currentPath.Positions.Count; currentTargetIndex++)
//        {
//            // Debug.Assert(currentPath != null);
//            // Debug.Assert(_count == currentPath.Positions.Count);
//            // Debug.Assert(currentPath.Positions.Count < currentTargetIndex);

//            // 1. 목표 설정
//            targetPosition = currentPath.Positions[currentTargetIndex];
//            targetRotation = currentPath.Rotations[currentTargetIndex];
//            Debug.Assert(targetObj != null);
//            targetObj.transform.SetPositionAndRotation(targetPosition, targetRotation);

//            // 2. Gantry에 대한 IK 계산 (명령)
//            // bioIK.SolveIK();
//            // 3. 위치 도달 대기 (실행) - 별도의 코루틴 호출
//            yield return coroutineRunner.StartCoroutine(
//                WaitUntilStagnationOrTimeout(150.0f)
//            );

//            // 4. XML 저장
//            if (isWeavingPath)
//            {
//                // 위빙 경로일 경우, 위빙 각도 정보를 함께 전달하여 기록
//                xmlRecorder.RecordCurrentPose_weaving(this.coroutineRunner.GetComponent<main>().makeXmlInstance, weavingAngle, 44.0f, 44.0f);
//            }
//            else
//            {
//                // 일반 경로일 경우, 기존 방식으로 기록
//                xmlRecorder.RecordCurrentPose(this.coroutineRunner.GetComponent<main>().makeXmlInstance);
//            }            if (coroutineRunner is main mainScript && mainScript.simulationStatus != null)
//            {
//                mainScript.simulationStatus.OnPointCompleted();
//            }
//            // Debug.Log($"[Executor] XML 저장 완료: Point {currentTargetIndex + 1} / {currentPath.Positions.Count}");
//            // 5. 다음 지점 전 한 프레임 대기
//            yield return null;
//        }

//        // === 5. 완료 ===
//        currentState = MotionState.Completed;
//        Debug.Log($"[Executor] State: {currentState} - 면 {currentPath.FaceIndex} 작업 완료");

//        // 다음 작업을 위해 기본 '가속 모드'로 복원
//        SetMotionMode(5.0f);
//        IsExecuting = false;
//        // xmlRecorder.FinalizeRecording(); // 남은 pose 저장
//        // xmlRecorder.StartNewFile();      // 다음 면을 위해 새 파일 시작
//        _bioIK.autoIK = true;

//        if (toggleObstacleAvoiding != null) toggleObstacleAvoiding.SetShapeDistanceObjectives(false);
//        // jointController.EnableAllJoints();
//    }

//    private float GetWeavingAngleIfApplicable()
//    {
//        // 1. 현재 경로가 MotionPath_Weaving 타입인지 확인합니다.
//        if (currentPath is MotionPathGenerator.MotionPath_Weaving weavingPath)
//        {
//            // 2. 타입이 맞으면, 경로에 저장된 WeavingAngle 값을 반환합니다.
//            Debug.Log($"<color=cyan>이 경로는 위빙 경로입니다. 저장된 위빙 각도: {weavingPath.WeavingAngle:F2}도</color>");
//            return weavingPath.WeavingAngle;
//        }

//        // 위빙 경로가 아니면 0을 반환합니다.
//        return 0f;
//    }
//    /// <summary>
//    /// [수정됨] 위치 오차가 더 이상 개선되지 않거나, 타임아웃이 되면 종료합니다.
//    /// 정체 상태로 종료될 때 현재 위치를 XML로 저장합니다.
//    /// </summary>
//    /// <param name="timeout">최대 대기 시간</param>
//    public IEnumerator WaitUntilStagnationOrTimeout(float timeout)
//    {
//        float mainTimer = 0f;
//        float stagnationTimer = 0f;
//        float lastPosErr = float.MaxValue;
//        const float stagnationThreshold = 0.5f;

//        while (mainTimer < timeout)
//        {
//            Debug.Assert(eef != null);
//            Debug.Assert(targetObj != null);
//            float currentPosErr = Vector3.Distance(eef.position, targetObj.transform.position);

//            // 1. 목표에 도달했으면 즉시 성공 종료
//            if (currentPosErr < 0.005f)
//            {
//                float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
//                // Debug.Log($"[Executor] 위치 도달 성공. 최종 회전 오차: {rotErr:F2}도");
//                yield break;
//            }

//            // 2. 오차 정체 감지
//            if (Mathf.Abs(lastPosErr - currentPosErr) < 0.0001f)
//            {
//                stagnationTimer += Time.deltaTime;
//            }
//            else
//            {
//                stagnationTimer = 0f;
//            }

//            lastPosErr = currentPosErr;

//            // 3. 정체 시간이 임계값을 넘으면 IK가 멈춘 것으로 간주하고 강제 종료
//            if (stagnationTimer > stagnationThreshold)
//            {
//                float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
//                Debug.LogWarning($"[Executor] IK 이동이 정체되어 Point {currentTargetIndex + 1}을 건너뜁니다. (최종 위치 오차: {currentPosErr:F4}m, 회전 오차: {rotErr:F2}도)");

//                // ======================= [핵심 수정 코드] =======================
//                // 건너뛰기 직전, 현재 로봇의 마지막 상태를 XML로 저장합니다.
//                Debug.LogWarning("[Executor] 정체 상태의 마지막 위치를 XML로 저장합니다.");
//                // xmlRecorder.RecordCurrentPose();
//                // ===============================================================

//                yield break; // 코루틴을 종료하고 다음 경로점으로 넘어갑니다.
//            }

//            mainTimer += Time.deltaTime;
//            yield return null;
//        }

//        // 4. 전체 타임아웃 발생 시
//        Debug.LogError($"[Executor] 위치 도달 전체 시간 초과! (Point {currentTargetIndex + 1})");
//    }
//    private void SwitchPathAngleAndApplyToFutureRotations(int startIndex)
//    {
//        // 현재 가로줄 번호가 짝수(0, 2, 4...)이면 120도, 홀수(1, 3, 5...)이면 60도를 목표 각도로 설정합니다.
//        float newTargetAngleBps = (completedHorizontalRows % 2 == 0) ? 120f : 60f;

//        // 이 메서드가 호출될 때마다, 즉시 전역 변수도 업데이트하여 현재 상태를 동기화합니다.
//        currentFace7_AngleBps = newTargetAngleBps;

//        // Debug.Log($"<color=cyan>[Executor] 각도 결정 실행: {completedHorizontalRows + 1}번째 줄의 목표 각도는 {currentFace7_AngleBps}도 입니다.</color>");

//        Quaternion newTargetRotation = Quaternion.LookRotation(
//            Quaternion.AngleAxis(currentFace7_AngleBps, currentPath.HeightDir) * currentPath.FaceNormal,
//            currentPath.HeightDir
//        );

//        for (int i = startIndex; i < currentPath.Rotations.Count; i++)
//        {
//            currentPath.Rotations[i] = newTargetRotation;
//        }
//    }
//    private IEnumerator SolveIKPeriodicallyUntilPositionReached(float timeout)
//    {
//        float timer = 0f;
//        int frameCount = 0; // 프레임 카운터

//        while (timer < timeout)
//        {
//            // 3프레임마다 IK 계산 실행
//            // if (frameCount % 10 == 0)
//            // {
//            //     // Debug.Log($"Frame {Time.frameCount}: Solving IK for point {currentTargetIndex + 1}");
//            //     bioIK.SolveIK();
//            // }

//            // 매 프레임 위치 오차 확인
//            float posErr = Vector3.Distance(eef.position, targetObj.transform.position);
//            float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);

//            if (currentPath.FaceIndex == 8)
//            {
//                posErr = Vector3.Distance(eef.position, targetObj.transform.position);
//                rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
//                Debug.Log($"[Executor-Debug] 7번 면 Point {currentTargetIndex + 1} 대기 중... PosErr: {posErr:F4}, RotErr: {rotErr:F2}");
//            }
//            if (posErr <= 0.01f)
//            {
//                rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);
//                Debug.Log($"[Executor] 위치 도달 완료. 최종 회전 오차: {rotErr:F2}도");
//                yield break; // 성공적으로 코루틴 종료
//            }

//            timer += Time.deltaTime;
//            frameCount++;
//            yield return null; // 다음 프레임까지 대기
//        }

//        // 타임아웃 발생 시
//        Debug.LogWarning($"[Executor] 위치 도달 시간 초과! (Point {currentTargetIndex + 1})");
//    }
//    // 타임아웃 기능이 있는 대기 코루틴 (헬퍼 함수)
//    private IEnumerator WaitUntilCondition(System.Func<bool> condition, float timeout, string taskName)
//    {
//        float timer = 0f;
//        while (!condition())
//        {
//            if (timer > timeout)
//            {
//                Debug.LogWarning($"[Executor] {taskName} 시간 초과! 다음 단계로 강제 진행합니다.");
//                yield break; // Timeout 발생 시 코루틴 종료
//            }
//            timer += Time.deltaTime;
//            yield return null; // 조건 만족까지 매 프레임 대기
//        }
//        Debug.Log($"[Executor] {taskName} 완료.");
//    }
//    /// <summary>
//    /// (추가된 헬퍼 메소드) 면의 법선 벡터를 기반으로 최적의 Pan(Upbox) 각도를 계산합니다.
//    /// </summary>
//    private float CalculatePanAngleForNormal(Vector3 normal)
//    {
//        float[] angles = { 0f, 90f, 180f, -90f };
//        Vector3[] xDirs = { Vector3.right, Vector3.back, Vector3.left, Vector3.forward };
//        Vector3 normalXZ = new Vector3(normal.x, 0, normal.z).normalized;
//        float minDot = float.PositiveInfinity;
//        int bestIdx = 0;
//        for (int i = 0; i < 4; i++)
//        {
//            float dot = Vector3.Dot(normalXZ, xDirs[i]);
//            if (dot < minDot) { minDot = dot; bestIdx = i; }
//        }
//        return angles[bestIdx];
//    }
//    public void RotateUpboxDirectly(float angle)
//    {

//        GameObject upboxObj = GetUpboxGameObject();
//        if (upboxObj != null)
//        {
//            upboxObj.transform.localRotation = Quaternion.Euler(0, 0, angle);

//        }
//        else
//        {
//            Debug.LogWarning("[Executor] upbox GameObject를 찾지 못했습니다.");
//        }
//    }

//    public void RotatedownDirectly(float angle)
//    {
//        // down joint 오브젝트를 찾습니다.
//        GameObject downObj = GetDownGameObject(); 
//        if (downObj != null)
//        {
//            // 찾은 오브젝트를 x축 기준으로 회전시킵니다.
//            downObj.transform.localRotation = Quaternion.Euler(angle, 0, 0); 
//            Debug.Log($"[Executor] down joint를 x축 기준으로 {angle}도 회전시켰습니다.");
//        }
//        else
//        {
//            Debug.LogWarning("[Executor] 'down'이라는 이름의 GameObject를 찾지 못했습니다.");
//        }
//    }
//    public GameObject GetUpboxGameObject()
//    {
//        if (bioIK == null || bioIK.Segments == null)
//            return null;

//        foreach (var segment in bioIK.Segments)
//        {
//            if (segment.Joint != null && segment.Joint.gameObject.name == "upbox")
//            {
//                return segment.Joint.gameObject;
//            }
//        }
//        return null;
//    }
//    public GameObject GetDownGameObject()
//    {
//        if (bioIK == null || bioIK.Segments == null)
//            return null;

//        foreach (var segment in bioIK.Segments)
//        {
//            // "down" 이라는 이름의 joint를 찾습니다.
//            if (segment.Joint != null && segment.Joint.gameObject.name == "down")
//            {
//                return segment.Joint.gameObject;
//            }
//        }
//        return null;
//    }

//}

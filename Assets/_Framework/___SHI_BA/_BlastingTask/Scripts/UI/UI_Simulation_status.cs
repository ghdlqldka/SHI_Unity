using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using _SHI_BA;

/// <summary>
/// main 스크립트의 SequentialMotionWithDeletionCoroutine을 정확히 모니터링하여 
/// 시뮬레이션 진행률을 실시간으로 추적하고 UI에 표시하는 클래스
/// </summary>
public class UI_Simulation_status : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [Tooltip("진행률을 표시할 슬라이더 (0~1)")]
    public Slider Slider_Simulation;

    [Tooltip("진행률을 백분율로 표시할 텍스트")]
    public TextMeshProUGUI Text_Simulation_Percent;

    [Header("모니터링 대상")]
    [Tooltip("모니터링할 main 스크립트")]
    public BA_Main mainController;

    [Header("시뮬레이션 상태")]
    [Tooltip("현재 시뮬레이션이 실행 중인지 여부")]
    public bool IsSimulationRunning 
    { 
        get; 
        private set; } = false;

    // 내부 상태 추적용 변수들
    private int totalPoints = 0;           // 전체 포인트 수
    private int completedPoints = 0;       // 완료된 포인트 수
    private int initialPathCount = 0;      // 초기 경로 수 (면 단위 추적용)
    private int currentPathCount = 0;      // 현재 경로 수
    private bool isTrackingStarted = false;
    private float lastUpdateTime = 0f;

    // 포인트 단위 추적을 위한 변수들
    private int currentFaceIndex = -1;     // 현재 처리 중인 면 번호
    // private int currentPointInFace = 0;    // 현재 면에서 처리 중인 포인트 번호

    protected void Awake()
    {
        Debug.Assert(mainController != null);
    }
    void Start()
    {
        // 초기화
        ResetProgress();
    }

    void Update()
    {
        // 0.1초마다 상태 체크 (너무 자주 체크하지 않도록)
        if (Time.time - lastUpdateTime > 0.1f)
        {
            MonitorSimulationProgress();
            lastUpdateTime = Time.time;
        }
    }

    private void MonitorSimulationProgress()
    {
        // generatedPaths가 null이면 초기화 대기 상태
        if (BA_PathDataManager.Instance.generatedPaths == null)
        {
            return;
        }

        int pathCount = BA_PathDataManager.Instance.generatedPaths.Count;

        // 1. 경로가 처음 생성된 경우 (경로 생성 완료)
        if (!isTrackingStarted && pathCount > 0)
        {
            InitializeTracking(pathCount);
            return;
        }

        // 2. 추적이 시작된 상태에서 상태 변화 감지
        if (isTrackingStarted)
        {
            // RobotMotionExecutor의 상태 체크
            bool isExecutorRunning = mainController.motionExecutor?.IsExecuting ?? false;

            // 시뮬레이션 시작 감지
            if (!IsSimulationRunning && isExecutorRunning)
            {
                StartSimulation();
            }

            // 경로 수가 줄어든 경우 (면 완료) - 단순히 로그만 출력
            if (pathCount < currentPathCount)
            {
                OnPathCompleted(pathCount);
                currentPathCount = pathCount;
            }

            // 모든 경로가 완료된 경우
            if (pathCount == 0 && IsSimulationRunning)
            {
                OnSimulationComplete();
            }
        }
    }

    public void OnPointCompleted()
    {
        if (IsSimulationRunning && totalPoints > 0)
        {
            completedPoints++;
            UpdateProgressUI();

            // 매 포인트마다 로그 출력 (필요시 제거 가능)
            Debug.Log($"[Simulation_status] 포인트 완료: {completedPoints}/{totalPoints} ({GetProgressPercentage()}%)");
        }
    }

    private void InitializeTracking(int pathCount)
    {
        initialPathCount = pathCount;
        currentPathCount = pathCount;
        isTrackingStarted = true;
        IsSimulationRunning = false;

        // 전체 포인트 수 계산
        CalculateTotalPoints();
        completedPoints = 0;
        currentFaceIndex = -1;
        // currentPointInFace = 0;

        UpdateProgressUI();

        Debug.Log($"[Simulation_status] 추적 시작 - 총 {initialPathCount}개 면, {totalPoints}개 포인트 대기 중");
    }

    private void CalculateTotalPoints()
    {
        totalPoints = 0;
        if (BA_PathDataManager.Instance.generatedPaths != null)
        {
            foreach (BA_MotionPath path in BA_PathDataManager.Instance.generatedPaths)
            {
                totalPoints += path.PointList.Count;
            }
        }
        Debug.Log($"[Simulation_status] 전체 포인트 수 계산 완료: {totalPoints}개");
    }

    private void StartSimulation()
    {
        IsSimulationRunning = true;
        Debug.Log($"[Simulation_status] 🚀 시뮬레이션 시작 - 총 {initialPathCount}개 면 처리 중");
    }

    private void OnPathCompleted(int remainingPaths)
    {
        int completedPaths = initialPathCount - remainingPaths;
        // 진행률은 OnPointCompleted()에서만 업데이트하므로 여기서는 로그만 출력

        Debug.Log($"[Simulation_status] ✅ 면 {completedPaths} 완료 - 현재 포인트 진행률: {completedPoints}/{totalPoints} ({GetProgressPercentage()}%) | 남은 경로: {remainingPaths}개");
    }

    private void OnSimulationComplete()
    {
        IsSimulationRunning = false;
        UpdateProgressUI(forceComplete: true);

        Debug.Log("[Simulation_status] 🎉 시뮬레이션 완료!");

        // 완료 후 5초 뒤에 자동으로 초기화
        Invoke(nameof(ResetProgress), 5f);
    }

    public void ResetProgress()
    {
        initialPathCount = 0;
        currentPathCount = 0;
        totalPoints = 0;
        completedPoints = 0;
        currentFaceIndex = -1;
        // currentPointInFace = 0;
        IsSimulationRunning = false;
        isTrackingStarted = false;
        UpdateProgressUI();

        Debug.Log("[Simulation_status] 진행률 초기화 완료");
    }

    public float GetProgressNormalized()
    {
        if (totalPoints <= 0) 
            return 0f;

        return Mathf.Clamp01((float)completedPoints / totalPoints);
    }

    public int GetProgressPercentage()
    {
        return Mathf.RoundToInt(GetProgressNormalized() * 100f);
    }

    private void UpdateProgressUI(bool forceComplete = false)
    {
        float normalizedProgress = forceComplete ? 1.0f : GetProgressNormalized();
        int percentage = forceComplete ? 100 : GetProgressPercentage();

        if (Slider_Simulation != null)
        {
            Slider_Simulation.value = normalizedProgress;
        }

        if (Text_Simulation_Percent != null)
        {
            Text_Simulation_Percent.text = $"{percentage}%";
        }
    }

}
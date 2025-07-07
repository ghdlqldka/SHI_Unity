
using UnityEngine;

public class GantryController : MonoBehaviour
{
    [Header("Control Objects")]
    [Tooltip("로봇이 최종적으로 도달해야 할 목표 지점입니다.")]
    public Transform mainTarget;
    [Tooltip("로봇의 툴 끝(Tool Center Point) 지점입니다.")]
    public Transform tcpTransform;

    [Header("Gantry Mover Components")]
    public IKMover x_mover;
    public IKMover y_mover;
    public IKMover z;

    // 시퀀스의 현재 상태를 관리하는 열거형(Enum)
    private enum GantrySequenceState
    {
        Inactive,      // 일반 작동 모드 (모든 축 동시 이동)
        ConvergingY,   // Y축 우선 수렴 모드
    }
    private GantrySequenceState currentState = GantrySequenceState.Inactive;

    // 수렴을 판단하기 위한 거리 임계값 (예: 1cm)
    private const float CONVERGENCE_THRESHOLD = 0.01f;

    /// <summary>
    /// Y축 우선 이동 시퀀스를 새로 시작합니다.
    /// 외부에서 이 함수를 호출하여 시퀀스를 트리거할 수 있습니다.
    /// </summary>
    public void EnableOnlyYaxis()
    {
        // 상태를 'Y축 수렴' 모드로 변경하여 시퀀스를 시작합니다.
        this.currentState = GantrySequenceState.ConvergingY;
        // Debug.Log("▶▶ [시퀀스 시작] Y축 우선 수렴을 시작합니다.");
    }

    /// <summary>
    /// 매 프레임 호출되며, 상태에 따라 이동 로직을 제어합니다.
    /// </summary>
    void LateUpdate()
    {
        if (mainTarget == null || tcpTransform == null)
        {
            return; // 필수 컴포넌트가 없으면 실행 중지
        }
        // --- 디버깅 로그 강화 시작 ---
        Vector3 tcpPos = tcpTransform.position;
        Vector3 targetPos = mainTarget.position;
        
        // 1. 각 축의 실제 거리 차이 계산
        // float x_distance_actual = Mathf.Abs(tcpPos.x - targetPos.x);
        // float y_distance_actual = Mathf.Abs(tcpPos.y - targetPos.y);
        // float z_distance_actual = Mathf.Abs(tcpPos.z - targetPos.z);
        // // Debug.Log($"<color=lime>[축별 실제 거리] X: {x_distance_actual:F4} m, Y: {y_distance_actual:F4} m, Z: {z_distance_actual:F4} m</color>");

        // // 2. 제어 로직에서 사용하는 거리 계산 (사용자 요청 사항)
        // float x_distance_logic = Mathf.Abs(tcpPos.y - targetPos.y); // y좌표 차이
        // float y_distance_logic = Mathf.Abs(tcpPos.x - targetPos.x); // x좌표 차이 (기존 y_distance)
        // float z_distance_logic = Mathf.Abs(tcpPos.z - targetPos.z); // z좌표 차이

        // Debug.Log($"[로직 기준 거리] X_distance (Y축 차이): {x_distance_logic:F4} m, Y_distance (X축 차이): <color=yellow>{y_distance_logic:F4} m</color>, Z_distance (Z축 차이): {z_distance_logic:F4} m");
        // --- 디버깅 로그 강화 끝 ---
        // 현재 상태에 따라 동작을 분기합니다.
        // 1. TCP와 Target 간의 Y축 거리 차이를 계산합니다.
        float y_distance = Mathf.Abs(tcpTransform.position.x - mainTarget.position.x);

        // 2. Y축 거리가 임계값(0.01f)보다 큰지 확인합니다.
        if (y_distance > CONVERGENCE_THRESHOLD)
        {
            // [Y축 우선 모드] Y축 거리가 멀면 y_mover만 활성화합니다.
            if (y_mover != null) y_mover.enabled = true;
            if (x_mover != null) x_mover.enabled = false;
            if (z != null) z.enabled = false;

            // Debug.Log($"[Y축 우선 수렴] 남은 Y축 거리: <color=yellow>{y_distance:F4} m</color>. Y축만 이동합니다.");
        }
        else
        {
            // [전체 동시 모드] Y축이 수렴했으면 모든 Mover를 활성화합니다.
            if (y_mover != null) y_mover.enabled = true;
            if (x_mover != null) x_mover.enabled = true;
            if (z != null) z.enabled = true;
            // Debug.Log($"[Y축 우선 수렴] 남은 Y축 거리: <color=lime>{y_distance:F4} m</color>. Y축만 이동합니다.");

            // Debug.Log($"[전체 동시 추적] Y축 수렴 완료. <color=lime>모든 축을 함께 이동합니다.</color>");
        }

        // --- PreOffset 계산 로직 (기존과 동일) ---
        // 이 로직은 현재 활성화된(움직이는) Mover에 대해서만 PreOffset을 계속 보정합니다.
        if (x_mover != null && x_mover.enabled)
        {
            float offsetZ = tcpTransform.position.z - x_mover.Origin.position.z;
            x_mover.PreOffset = -offsetZ;
        }
        if (y_mover != null && y_mover.enabled)
        {
            float offsetX = tcpTransform.position.x - y_mover.Origin.position.x;
            y_mover.PreOffset = offsetX;
        }
        if (z != null && z.enabled)
        {
            float offsetY = tcpTransform.position.y - z.Origin.position.y;
            z.PreOffset = -offsetY * 10f;
        }
    }
}
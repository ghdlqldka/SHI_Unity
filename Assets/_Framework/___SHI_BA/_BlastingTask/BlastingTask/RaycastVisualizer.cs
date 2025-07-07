// BlastingTask/RaycastVisualizer.cs
using System.Linq; // OrderBy 사용을 위해 추가
using UnityEngine;

public class RaycastVisualizer : MonoBehaviour
{
    public RobotMotionExecutor executor;
    public StiffnerEdgeMotion StiffnerEdgeMotion; // <<< [추가] Test.cs 참조 변수

    [Header("시각화 및 감지 설정")]
    public Transform tcpTransform;
    public LayerMask detectionLayer;
    public float rayDistance = 0.8f;

    [Header("쿨다운 설정")]
    public float cooldownSeconds = 0.05f;

    private bool isCooldownActive = false;
    private float cooldownTimer = 0f;
    void LateUpdate()
    {
        if (tcpTransform == null) return;

        if (isCooldownActive)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f) isCooldownActive = false;
        }

        Vector3 origin = tcpTransform.position;
        Vector3 direction = tcpTransform.right;

        Debug.DrawRay(origin, direction * rayDistance, Color.yellow);

        if (!isCooldownActive)
        {
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, rayDistance, detectionLayer);

            if (hits.Length > 0)
            {
                RaycastHit closestHit = hits.OrderBy(h => h.distance).First();

                ObstacleIdentifier identifier = closestHit.collider.GetComponent<ObstacleIdentifier>();

                if (identifier != null)
                {
                    Debug.DrawRay(origin, direction * closestHit.distance, Color.red);

                    if (executor != null)
                    {
                        executor.OnObstacleDetected(closestHit.collider.gameObject);
                    }

                    // if (StiffnerEdgeMotion != null)
                    // {
                    //     StiffnerEdgeMotion.OnObstacleDetected(closestHit.collider.gameObject);
                    // }
                    // [수정 끝]

                    isCooldownActive = true;
                    cooldownTimer = cooldownSeconds;
                }
            }
        }
    }
}
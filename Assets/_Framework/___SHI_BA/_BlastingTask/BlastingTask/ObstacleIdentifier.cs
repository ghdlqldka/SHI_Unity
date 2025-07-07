using UnityEngine;

public class ObstacleIdentifier : MonoBehaviour
{
    // 장애물의 타입을 정의합니다 (부모 또는 자식)
    public enum ObstacleType
    {
        Parent,
        Child
    }

    [Tooltip("이 장애물의 타입을 설정하세요 (Parent 또는 Child).")]
    public ObstacleType type;

    [Tooltip("이 장애물이 Child 타입일 경우, 부모가 되는 오브젝트를 연결해주세요.")]
    public GameObject parentObstacle; // 자식일 경우에만 사용됩니다.
}
using System.Collections.Generic;
using UnityEngine;

public class RobotMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public List<Vector3> pathPoints = new List<Vector3>(); // 이동할 위치들
    private int currentTargetIndex = 0;
    private bool isMoving = false;

    void Update()
    {
        if (isMoving && currentTargetIndex < pathPoints.Count)
        {
            Vector3 target = pathPoints[currentTargetIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            // 도착하면 다음 위치로
            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                currentTargetIndex++;
            }
        }
    }

    public void StartMoving()
    {
        if (pathPoints.Count > 0)
        {
            transform.position = pathPoints[0]; // 첫 위치로 바로 이동
            currentTargetIndex = 1;
            isMoving = true;
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        currentTargetIndex = 0;
    }

}
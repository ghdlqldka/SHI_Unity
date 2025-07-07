using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.02f; // ← 여기 추가!
    public RobotMover robotMover;  // ← Robot 이동 연결용

    private GameObject lineParent;
    private Vector3? lastPoint = null;
    public List<Vector3> clickedPoints = new List<Vector3>();

    void Start()
    {
        lineParent = new GameObject("LineParent");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 point = hit.point;
                clickedPoints.Add(point); // ← 클릭한 위치 저장

                if (lastPoint != null)
                {
                    DrawLine(lastPoint.Value, point);
                }
                lastPoint = point;
            }
        }
    }

    void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject lineObj = new GameObject("SegmentLine");
        lineObj.transform.parent = lineParent.transform;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;

        lr.material = lineMaterial; // 머티리얼 할당
    }
    public void DeleteAllLines()
    {
        foreach (Transform child in lineParent.transform)
        {
            Destroy(child.gameObject);
        }

        // 필요 시 시작점도 초기화
        lastPoint = null;
    }
    public void OnPlayButtonClick()
    {
        if (clickedPoints.Count < 2)
        {
            Debug.LogWarning("적어도 2개 이상의 점이 필요합니다!");
            return;
        }

        robotMover.pathPoints = new List<Vector3>(clickedPoints); // 전달!
        robotMover.StartMoving();
    }
}

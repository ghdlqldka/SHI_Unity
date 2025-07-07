using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public Material lineMaterial;
    public float lineWidth = 0.02f; // �� ���� �߰�!
    public RobotMover robotMover;  // �� Robot �̵� �����

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
                clickedPoints.Add(point); // �� Ŭ���� ��ġ ����

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

        lr.material = lineMaterial; // ��Ƽ���� �Ҵ�
    }
    public void DeleteAllLines()
    {
        foreach (Transform child in lineParent.transform)
        {
            Destroy(child.gameObject);
        }

        // �ʿ� �� �������� �ʱ�ȭ
        lastPoint = null;
    }
    public void OnPlayButtonClick()
    {
        if (clickedPoints.Count < 2)
        {
            Debug.LogWarning("��� 2�� �̻��� ���� �ʿ��մϴ�!");
            return;
        }

        robotMover.pathPoints = new List<Vector3>(clickedPoints); // ����!
        robotMover.StartMoving();
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class FindRoot
{

    private static Vector3Int[] _directions; // �� ���� ĳ�ÿ� �ʵ�

    private static Vector3Int[] directions // �� �ܺ� ���ٿ� �Ӽ�
    {
        get
        {
            if (_directions == null || _directions.Length == 0)
            {
                _directions = GenerateDirections(); // �� ���⼭ ����
            }
            return _directions;
        }
    }

    /// <summary>
    ///  ������ ���ã�� 
    /// </summary>
    /// <param name="startPos">���� ��ġ</param>
    /// <param name="goalPos">������ ��ġ</param>
    public static List<Vector3> FindPath(Transform startPos, Vector3 goalPos, Collider blockCollider, float distanceThreshold , LayerMask layerMask)//��ã��
    {
       
        List<Node> openList = new List<Node>(); // Ž���� ��� ����Ʈ
        HashSet<Vector3> closedList = new HashSet<Vector3>(); // �̹� Ž���� ��� ����Ʈ

        Node startNode = new Node(0, Vector3.Distance(startPos.position, goalPos), startPos.position);
        openList.Add(startNode); // ���� ��带 ���� ����Ʈ�� �߰�
        int maxIterations = 10000; // ���� ���� ����
        int iterations = 0;
        while (openList.Count > 0)
        {
            Node current = openList.OrderBy(n => n.f).First();
            openList.Remove(current);
            closedList.Add(current.pos);
            if (++iterations > maxIterations)
            {
                Debug.Log("��� Ž�� �ߴ�: �ִ� �ݺ� Ƚ�� �ʰ�");
                return null;
            }
            //��ǥ ����
            if (Vector3.Distance(current.pos, goalPos) < 1.4f)
            {
                Debug.Log("����");
                current.pos = goalPos; // ��ǥ ��ġ�� ����
                return ReconstructPath(current);
            }
            closedList.Add(current.pos);
            foreach (Vector3Int dir in directions)
            {
                Vector3 neighborPos = current.pos + dir;
                if (closedList.Contains(neighborPos) || Physics.OverlapSphere(neighborPos, distanceThreshold - 1f, layerMask).Length > 0) continue;

                Vector3 center = neighborPos; // �ڽ� �߽�
                Vector3 halfExtents = Vector3.one * (distanceThreshold - 0.4f); // �ڽ� ũ�� (������)
                Quaternion rotation = Quaternion.identity;

                Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, layerMask);

               
                bool isBelowObstacle = false;

                if (neighborPos.y < blockCollider.bounds.min.y - 0.4f || neighborPos.y > blockCollider.bounds.max.y + 0.2f) // ������Ʈ�� �� ,�ٴں��� �Ʒ����̸�
                {
                    isBelowObstacle = true;

                }
                if (isBelowObstacle)
                {
                    continue; // �� ��δ� ���� (�Ʒ��� �İ�� �� ����)
                }




                int tentiveG = current.g + 1; // �ӽ� �̵���

                Node existingNode = openList.Find(a => a.pos == neighborPos); // openList�� neighborPos�� ���� ���� ������� ����
                if (existingNode == null) // ������ ����� �߰�
                {
                    Node newNode = new Node(tentiveG, Heuristic(neighborPos, goalPos), neighborPos, current);
                    openList.Add(newNode);
                }
                else if (tentiveG < existingNode.g)//��ª�� ��� �߽߰� ����
                {
                    existingNode.g = tentiveG;
                    existingNode.parent = current;
                }
            }

        }

        Debug.Log("FindPath : null");
        return null;
    }

    /// <summary>
    /// �������� �Ÿ� ��� �籸��
    /// </summary>
    /// <param name="endpos">���� ���</param>
    /// <returns>���</returns>
    public static List<Vector3> ReconstructPath(Node endpos)
    {
        List<Vector3> path = new List<Vector3>();
        Node currnet = endpos;
        while (currnet != null)
        {
            path.Add(currnet.pos);
            currnet = currnet.parent;
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// �� �� ������ �Ÿ�
    /// </summary>
    /// <param name="a">������ġ</param>
    /// <param name="b">������ġ</param>
    /// <returns></returns>
    private static float Heuristic(Vector3 a, Vector3 b)
    {
        Vector3 delta = b - a;

        float yWeight = 2f; // �� Y�� �̵��� �� ��ȣ�ϵ��� ����ġ �ο�
        float dx = delta.x;
        float dy = delta.y * yWeight;
        float dz = delta.z;

        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// 24�������
    /// </summary>
    /// <returns></returns>
    private static Vector3Int[] GenerateDirections()
    {
        List<Vector3Int> list = new List<Vector3Int>();

        // �⺻ 6����
        list.AddRange(new Vector3Int[]
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
        });

        // ��� �밢�� (XY, YZ, XZ)
        int[] dirs = new int[] { -1, 1 };
        foreach (int x in dirs)
        {
            foreach (int y in dirs)
            {
                list.Add(new Vector3Int(x, y, 0)); // XY
                list.Add(new Vector3Int(x, 0, y)); // XZ
                list.Add(new Vector3Int(0, y, x)); // YZ
            }
        }

        return list.ToArray();
    }
    public static void DrawPath(List<Vector3> path, LineRenderer lineRenderer)
    {
        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path.ToArray());
    }
}

public class Node // ���
{
    public Vector3 pos; // ����� ��ġ
    public Node parent; // �θ� ���
    public int g; // ���� ��忡�� ���� �������� ���
    public float h; // ���� ��忡�� ��ǥ �������� ���� ���
    public float f => g + h; // �� ���
    public Node(int _gCost, float _hCost, Vector3 _pos, Node _parent = null)
    {
        pos = _pos;
        parent = _parent;
        g = _gCost;
        h = _hCost;
    }
}

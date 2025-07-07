using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class FindRoot
{

    private static Vector3Int[] _directions; // ← 내부 캐시용 필드

    private static Vector3Int[] directions // ← 외부 접근용 속성
    {
        get
        {
            if (_directions == null || _directions.Length == 0)
            {
                _directions = GenerateDirections(); // ← 여기서 생성
            }
            return _directions;
        }
    }

    /// <summary>
    ///  최적의 경로찾기 
    /// </summary>
    /// <param name="startPos">시작 위치</param>
    /// <param name="goalPos">마지막 위치</param>
    public static List<Vector3> FindPath(Transform startPos, Vector3 goalPos, Collider blockCollider, float distanceThreshold , LayerMask layerMask)//길찾기
    {
       
        List<Node> openList = new List<Node>(); // 탐색할 노드 리스트
        HashSet<Vector3> closedList = new HashSet<Vector3>(); // 이미 탐색한 노드 리스트

        Node startNode = new Node(0, Vector3.Distance(startPos.position, goalPos), startPos.position);
        openList.Add(startNode); // 시작 노드를 오픈 리스트에 추가
        int maxIterations = 10000; // 무한 루프 방지
        int iterations = 0;
        while (openList.Count > 0)
        {
            Node current = openList.OrderBy(n => n.f).First();
            openList.Remove(current);
            closedList.Add(current.pos);
            if (++iterations > maxIterations)
            {
                Debug.Log("경로 탐색 중단: 최대 반복 횟수 초과");
                return null;
            }
            //목표 도달
            if (Vector3.Distance(current.pos, goalPos) < 1.4f)
            {
                Debug.Log("도착");
                current.pos = goalPos; // 목표 위치로 설정
                return ReconstructPath(current);
            }
            closedList.Add(current.pos);
            foreach (Vector3Int dir in directions)
            {
                Vector3 neighborPos = current.pos + dir;
                if (closedList.Contains(neighborPos) || Physics.OverlapSphere(neighborPos, distanceThreshold - 1f, layerMask).Length > 0) continue;

                Vector3 center = neighborPos; // 박스 중심
                Vector3 halfExtents = Vector3.one * (distanceThreshold - 0.4f); // 박스 크기 (반지름)
                Quaternion rotation = Quaternion.identity;

                Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, layerMask);

               
                bool isBelowObstacle = false;

                if (neighborPos.y < blockCollider.bounds.min.y - 0.4f || neighborPos.y > blockCollider.bounds.max.y + 0.2f) // 오브젝트의 위 ,바닥보다 아래쪽이면
                {
                    isBelowObstacle = true;

                }
                if (isBelowObstacle)
                {
                    continue; // 이 경로는 막음 (아래로 파고들 수 없음)
                }




                int tentiveG = current.g + 1; // 임시 이동값

                Node existingNode = openList.Find(a => a.pos == neighborPos); // openList에 neighborPos와 같은 값이 있을경우 저장
                if (existingNode == null) // 없으면 새노드 추가
                {
                    Node newNode = new Node(tentiveG, Heuristic(neighborPos, goalPos), neighborPos, current);
                    openList.Add(newNode);
                }
                else if (tentiveG < existingNode.g)//더짧은 노드 발견시 변경
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
    /// 두점사의 거리 경로 재구성
    /// </summary>
    /// <param name="endpos">도착 노드</param>
    /// <returns>경로</returns>
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
    /// 두 점 사이의 거리
    /// </summary>
    /// <param name="a">도착위치</param>
    /// <param name="b">시작위치</param>
    /// <returns></returns>
    private static float Heuristic(Vector3 a, Vector3 b)
    {
        Vector3 delta = b - a;

        float yWeight = 2f; // ← Y축 이동을 덜 선호하도록 가중치 부여
        float dx = delta.x;
        float dy = delta.y * yWeight;
        float dz = delta.z;

        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// 24방향생성
    /// </summary>
    /// <returns></returns>
    private static Vector3Int[] GenerateDirections()
    {
        List<Vector3Int> list = new List<Vector3Int>();

        // 기본 6방향
        list.AddRange(new Vector3Int[]
        {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1)
        });

        // 평면 대각선 (XY, YZ, XZ)
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

public class Node // 노드
{
    public Vector3 pos; // 노드의 위치
    public Node parent; // 부모 노드
    public int g; // 시작 노드에서 현재 노드까지의 비용
    public float h; // 현재 노드에서 목표 노드까지의 예상 비용
    public float f => g + h; // 총 비용
    public Node(int _gCost, float _hCost, Vector3 _pos, Node _parent = null)
    {
        pos = _pos;
        parent = _parent;
        g = _gCost;
        h = _hCost;
    }
}

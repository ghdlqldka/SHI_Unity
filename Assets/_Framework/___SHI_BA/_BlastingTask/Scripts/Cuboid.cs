using UnityEngine;
using TMPro; // TMP_InputField 사용을 위해 추가

[System.Serializable]
public class CubeStruct : MonoBehaviour
{
    [Header("큐브의 8개 꼭짓점 좌표")]
    public Vector3 point_0 = new Vector3(-12.454f, 7.711f, 9.793f);
    public Vector3 point_1 = new Vector3(-19.854f, 7.632f, 9.990f);
    public Vector3 point_2 = new Vector3(-19.654f, 7.633f, 17.487f);
    public Vector3 point_3 = new Vector3(-12.254f, 7.712f, 17.290f);
    public Vector3 point_4 = new Vector3(-12.399f, 2.541f, 9.792f);
    public Vector3 point_5 = new Vector3(-19.799f, 2.462f, 9.989f);
    public Vector3 point_6 = new Vector3(-19.599f, 2.462f, 17.486f);
    public Vector3 point_7 = new Vector3(-12.199f, 2.542f, 17.289f);

    [Header("메쉬 설정")]
    public Material cubeMaterial;
    public bool showWireframe = false;
    public bool showSolidMesh = true;
    public Color wireframeColor = Color.green;
    public float wireframeWidth = 2f;

    [Header("시각화 설정")]
    public Color gizmoColor = Color.green;
    public bool showGizmos = true;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh cubeMesh;
    private LineRenderer[] wireframeLines;
    private bool isCubeCreated = false;

    void Start()
    {
        InitializeComponents();
    }

    // 컴포넌트 초기화 함수
    void InitializeComponents()
    {
        // MeshFilter 컴포넌트 추가
        if (meshFilter == null)
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // MeshRenderer 컴포넌트 추가
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // 기본 머티리얼 설정
        if (cubeMaterial == null)
        {
            cubeMaterial = new Material(Shader.Find("Standard"));
            cubeMaterial.color = Color.cyan;
        }

        // MeshRenderer가 확실히 존재할 때만 머티리얼 설정
        if (meshRenderer != null)
        {
            meshRenderer.material = cubeMaterial;
        }
    }

    // 메쉬 박스 토글 함수 (한 번 누르면 생성, 다시 누르면 삭제)
    [ContextMenu("Toggle Mesh Box")]
    public void ToggleMeshBox()
    {
        Debug.Log($"현재 상태: isCubeCreated = {isCubeCreated}");

        if (isCubeCreated)
        {
            Debug.Log("큐브 삭제 시도...");
            RemoveMeshBox();
        }
        else
        {
            Debug.Log("큐브 생성 시도...");
            CreateMeshBox();
        }
    }

    // 메쉬 박스 생성 함수 (이 함수를 호출하면 박스가 나타남)
    [ContextMenu("Create Mesh Box")]
    public void CreateMeshBox()
    {
        Debug.Log($"CreateMeshBox 호출됨. 현재 isCubeCreated = {isCubeCreated}");

        // 이미 생성되어 있으면 리턴
        if (isCubeCreated)
        {
            Debug.Log("큐브가 이미 생성되어 있습니다!");
            return;
        }

        // 컴포넌트가 없으면 먼저 추가
        InitializeComponents();

        // 한 프레임 기다린 후 메쉬 생성 (컴포넌트가 완전히 초기화될 때까지)
        StartCoroutine(CreateMeshDelayed());
    }

    // 지연된 메쉬 생성
    System.Collections.IEnumerator CreateMeshDelayed()
    {
        yield return null; // 한 프레임 대기
        CreateCubeMesh();
        isCubeCreated = true;
        Debug.Log($"큐브 메쉬가 생성되었습니다! isCubeCreated = {isCubeCreated}");
    }

    // 메쉬 박스 제거 함수
    [ContextMenu("Remove Mesh Box")]
    public void RemoveMeshBox()
    {
        Debug.Log($"RemoveMeshBox 호출됨. 현재 isCubeCreated = {isCubeCreated}");

        // 생성되지 않은 상태면 리턴
        if (!isCubeCreated)
        {
            Debug.Log("삭제할 큐브가 없습니다!");
            return;
        }

        // 컴포넌트가 없으면 먼저 추가
        InitializeComponents();

        if (meshFilter != null)
            meshFilter.mesh = null;

        // 와이어프레임도 제거
        RemoveWireframe();

        isCubeCreated = false;
        Debug.Log($"큐브 메쉬가 제거되었습니다! isCubeCreated = {isCubeCreated}");
    }
    public Vector3 GetCenterPoint()
    {
        // 모든 점들의 좌표를 더하기 위한 변수를 초기화합니다.
        Vector3 total = Vector3.zero;

        // GetAllPoints() 메서드를 사용하여 모든 꼭짓점의 배열을 가져옵니다.
        Vector3[] allPoints = GetAllPoints();

        // 반복문을 통해 모든 꼭짓점의 좌표를 더합니다.
        foreach (Vector3 point in allPoints)
        {
            total += point;
        }

        // 더해진 총합을 꼭짓점의 개수(8)로 나누어 평균, 즉 중심점을 계산합니다.
        return total / allPoints.Length;
    }
    public void CreateCustomCubeMesh(Vector3[] customPoints)
    {
        Debug.Log("CreateCustomCubeMesh 호출됨");

        // 이미 생성되어 있으면 기존 큐브 제거
        if (isCubeCreated)
        {
            Debug.Log("기존 큐브 제거 중...");
            RemoveMeshBox();
        }

        // 커스텀 좌표로 메쉬 생성
        CreateCustomMeshWithPoints(customPoints);

        // 인풋필드 값들을 읽어서 커스텀 좌표 생성
        // if (ReadInputFieldValues())
        {
            // 컴포넌트 초기화
            InitializeComponents();

            // 지연된 커스텀 메쉬 생성
            StartCoroutine(CreateCustomMeshDelayed());
        }
    }

    // 지연된 커스텀 메쉬 생성
    System.Collections.IEnumerator CreateCustomMeshDelayed()
    {
        yield return null; // 한 프레임 대기
        // ReadInputFieldValues()에서 이미 커스텀 메쉬를 생성했으므로 여기서는 상태만 업데이트
        isCubeCreated = true;
        Debug.Log($"커스텀 큐브 메쉬가 생성되었습니다! isCubeCreated = {isCubeCreated}");
    }

    // 커스텀 좌표로 메쉬 생성하는 별도 함수
    void CreateCustomMeshWithPoints(Vector3[] customPoints)
    {
        if (cubeMesh == null)
            cubeMesh = new Mesh();

        cubeMesh.Clear();

        // 커스텀 좌표를 로컬 좌표로 변환
        Vector3 center = GetCenterFromPoints(customPoints);
        Vector3[] vertices = new Vector3[8];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = customPoints[i] - center;
        }

        // 오브젝트 위치를 중심점으로 설정
        transform.position = center;

        // 삼각형 인덱스 (12개 삼각형 = 6개 면 × 2개 삼각형)
        int[] triangles = new int[]
        {
            // 앞면 (2-3-7, 2-7-6)
            2, 3, 7,    2, 7, 6,
            // 뒷면 (0-1-5, 0-5-4)
            0, 1, 5,    0, 5, 4,
            // 왼쪽면 (3-0-4, 3-4-7)
            3, 0, 4,    3, 4, 7,
            // 오른쪽면 (1-2-6, 1-6-5)
            1, 2, 6,    1, 6, 5,
            // 윗면 (0-3-2, 0-2-1)
            0, 3, 2,    0, 2, 1,
            // 아랫면 (4-5-6, 4-6-7)
            4, 5, 6,    4, 6, 7
        };

        // UV 좌표 (텍스처 매핑용)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), // 상단 4개점
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)  // 하단 4개점
        };

        // 메쉬에 데이터 할당
        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.uv = uvs;

        // 노말 벡터 자동 계산
        cubeMesh.RecalculateNormals();

        // 솔리드 메쉬 표시 여부에 따라 MeshFilter에 할당
        if (showSolidMesh)
        {
            meshFilter.mesh = cubeMesh;
        }
        else
        {
            meshFilter.mesh = null;
        }

        // 와이어프레임 생성
        if (showWireframe)
        {
            CreateWireframe(vertices);
        }
        else
        {
            RemoveWireframe();
        }

        Debug.Log($"커스텀 큐브가 위치 {center}에 생성되었습니다!");

        

    }

    // 커스텀 포인트들의 중심점 계산
    Vector3 GetCenterFromPoints(Vector3[] points)
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in points)
        {
            center += point;
        }
        return center / 8f;
    }

    // 실제 큐브 메쉬 생성
    void CreateCubeMesh()
    {
        if (cubeMesh == null)
            cubeMesh = new Mesh();

        cubeMesh.Clear();

        // 버텍스 배열 (8개 꼭짓점을 로컬 좌표로 변환)
        Vector3[] vertices = GetAllPoints();

        // 오브젝트의 중심점을 기준으로 로컬 좌표로 변환
        Vector3 center = GetCenter();
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] - center;
        }

        // 오브젝트 위치를 중심점으로 설정
        transform.position = center;

        // 삼각형 인덱스 (12개 삼각형 = 6개 면 × 2개 삼각형)
        int[] triangles = new int[]
        {
            // 앞면 (2-3-7, 2-7-6)
            2, 3, 7,    2, 7, 6,
            // 뒷면 (0-1-5, 0-5-4)
            0, 1, 5,    0, 5, 4,
            // 왼쪽면 (3-0-4, 3-4-7)
            3, 0, 4,    3, 4, 7,
            // 오른쪽면 (1-2-6, 1-6-5)
            1, 2, 6,    1, 6, 5,
            // 윗면 (0-3-2, 0-2-1)
            0, 3, 2,    0, 2, 1,
            // 아랫면 (4-5-6, 4-6-7)
            4, 5, 6,    4, 6, 7
        };

        // UV 좌표 (텍스처 매핑용)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), // 상단 4개점
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0)  // 하단 4개점
        };

        // 메쉬에 데이터 할당
        cubeMesh.vertices = vertices;
        cubeMesh.triangles = triangles;
        cubeMesh.uv = uvs;

        // 노말 벡터 자동 계산
        cubeMesh.RecalculateNormals();

        // 솔리드 메쉬 표시 여부에 따라 MeshFilter에 할당
        if (showSolidMesh)
        {
            meshFilter.mesh = cubeMesh;
        }
        else
        {
            meshFilter.mesh = null;
        }

        // 와이어프레임 생성
        if (showWireframe)
        {
            CreateWireframe(vertices);
        }
        else
        {
            RemoveWireframe();
        }

        Debug.Log($"큐브가 위치 {center}에 생성되었습니다!");
    }

    // 와이어프레임 생성
    void CreateWireframe(Vector3[] vertices)
    {
        // 기존 와이어프레임 제거
        RemoveWireframe();

        // 12개의 선 (큐브의 12개 모서리)
        wireframeLines = new LineRenderer[12];

        // 와이어프레임 선들의 정점 인덱스 쌍
        int[,] lineIndices = new int[,]
        {
            // 상단 면 (0-1-2-3)
            {0, 1}, {1, 2}, {2, 3}, {3, 0},
            // 하단 면 (4-5-6-7)
            {4, 5}, {5, 6}, {6, 7}, {7, 4},
            // 수직 연결선
            {0, 4}, {1, 5}, {2, 6}, {3, 7}
        };

        // 각 선에 대해 LineRenderer 생성
        for (int i = 0; i < 12; i++)
        {
            GameObject lineObj = new GameObject($"WireframeLine_{i}");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = wireframeColor;
            lr.endColor = wireframeColor;
            lr.startWidth = wireframeWidth * 0.01f;
            lr.endWidth = wireframeWidth * 0.01f;
            lr.positionCount = 2;
            lr.useWorldSpace = false; // 로컬 좌표 사용

            // 선의 시작점과 끝점 설정
            Vector3 startPoint = vertices[lineIndices[i, 0]];
            Vector3 endPoint = vertices[lineIndices[i, 1]];

            lr.SetPosition(0, startPoint);
            lr.SetPosition(1, endPoint);

            wireframeLines[i] = lr;
        }
    }

    // 와이어프레임 제거
    void RemoveWireframe()
    {
        if (wireframeLines != null)
        {
            for (int i = 0; i < wireframeLines.Length; i++)
            {
                if (wireframeLines[i] != null)
                {
                    DestroyImmediate(wireframeLines[i].gameObject);
                }
            }
            wireframeLines = null;
        }
    }

    // 모든 점을 배열로 반환
    public Vector3[] GetAllPoints()
    {
        return new Vector3[]
        {
            point_0, point_1, point_2, point_3,
            point_4, point_5, point_6, point_7
        };
    }

    // 특정 인덱스의 점 반환
    public Vector3 GetPoint(int index)
    {
        switch (index)
        {
            case 0: return point_0;
            case 1: return point_1;
            case 2: return point_2;
            case 3: return point_3;
            case 4: return point_4;
            case 5: return point_5;
            case 6: return point_6;
            case 7: return point_7;
            default:
                Debug.LogError("Point index must be 0-7");
                return Vector3.zero;
        }
    }

    // 특정 인덱스의 점 설정
    public void SetPoint(int index, Vector3 point)
    {
        switch (index)
        {
            case 0: point_0 = point; break;
            case 1: point_1 = point; break;
            case 2: point_2 = point; break;
            case 3: point_3 = point; break;
            case 4: point_4 = point; break;
            case 5: point_5 = point; break;
            case 6: point_6 = point; break;
            case 7: point_7 = point; break;
            default:
                Debug.LogError("Point index must be 0-7");
                break;
        }

        // 좌표가 변경되면 메쉬도 업데이트
        if (meshFilter != null && meshFilter.mesh != null)
        {
            CreateCubeMesh();
        }
    }

    // 큐브의 중심점 계산
    public Vector3 GetCenter()
    {
        Vector3[] points = GetAllPoints();
        Vector3 center = Vector3.zero;

        foreach (Vector3 point in points)
        {
            center += point;
        }

        return center / 8f;
    }

    // 좌표 초기화 (기본값으로)
    public void ResetToDefaultCoordinates()
    {
        point_0 = new Vector3(-12.454f, 7.711f, 9.793f);
        point_1 = new Vector3(-19.854f, 7.632f, 9.990f);
        point_2 = new Vector3(-19.654f, 7.633f, 17.487f);
        point_3 = new Vector3(-12.254f, 7.712f, 17.290f);
        point_4 = new Vector3(-12.399f, 2.541f, 9.792f);
        point_5 = new Vector3(-19.799f, 2.462f, 9.989f);
        point_6 = new Vector3(-19.599f, 2.462f, 17.486f);
        point_7 = new Vector3(-12.199f, 2.542f, 17.289f);

        if (meshFilter != null && meshFilter.mesh != null)
        {
            CreateCubeMesh();
        }
    }

    // Scene 뷰에서 Gizmo 그리기
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;

        // 상단 면 (0-1-2-3)
        Gizmos.DrawLine(point_0, point_1);
        Gizmos.DrawLine(point_1, point_2);
        Gizmos.DrawLine(point_2, point_3);
        Gizmos.DrawLine(point_3, point_0);

        // 하단 면 (4-5-6-7)
        Gizmos.DrawLine(point_4, point_5);
        Gizmos.DrawLine(point_5, point_6);
        Gizmos.DrawLine(point_6, point_7);
        Gizmos.DrawLine(point_7, point_4);

        // 수직 연결선
        Gizmos.DrawLine(point_0, point_4);
        Gizmos.DrawLine(point_1, point_5);
        Gizmos.DrawLine(point_2, point_6);
        Gizmos.DrawLine(point_3, point_7);

        // 점 번호 표시 (Scene 뷰에서)
#if UNITY_EDITOR
        UnityEditor.Handles.color = gizmoColor;
        UnityEditor.Handles.Label(point_0, "0");
        UnityEditor.Handles.Label(point_1, "1");
        UnityEditor.Handles.Label(point_2, "2");
        UnityEditor.Handles.Label(point_3, "3");
        UnityEditor.Handles.Label(point_4, "4");
        UnityEditor.Handles.Label(point_5, "5");
        UnityEditor.Handles.Label(point_6, "6");
        UnityEditor.Handles.Label(point_7, "7");
#endif
    }

    // 선택되었을 때만 Gizmo 그리기
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // 중심점 표시
        Vector3 center = GetCenter();
        Gizmos.DrawWireSphere(center, 0.2f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(center, "Center");
#endif
    }

    // 큐브 생성 상태 확인
    public bool IsCubeCreated()
    {
        return isCubeCreated;
    }

    // 큐브 생성 상태 반환 (UI용)
    public string GetCubeStatus()
    {
        return isCubeCreated ? "큐브 삭제" : "큐브 생성";
    }
}
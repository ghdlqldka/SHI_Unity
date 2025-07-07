using UnityEngine;
using System.Collections.Generic;

// 이 스크립트는 'MarchingCubesLogic' 네임스페이스에 있는 클래스들을 사용합니다.
using MarchingCubesLogic;

[RequireComponent(typeof(MeshFilter))]
public class VoxelMeshGenerator : MonoBehaviour
{
    [Header("원본 데이터 설정")]
    [Tooltip("복셀 필터링을 적용할 원본 MeshFilter를 자동으로 감지합니다.")]
    [SerializeField]
    private MeshFilter sourceMeshFilter;

    [Header("필터링 및 생성 설정")]
    public float VoxelSize = 0.2f;
    public Material VoxelMaterial;

    private GameObject generatedVoxelObject;

    // Marching Cubes를 위한 멤버 변수
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Dictionary<Vector3, int> vertexIndexMap = new Dictionary<Vector3, int>();


    private void Awake()
    {
        sourceMeshFilter = GetComponent<MeshFilter>();
    }

    [ContextMenu("Voxel Collider 생성/업데이트 (Marching Cubes)")]
    public void GenerateVoxelCollider()
    {
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null)
        {
            Debug.LogError("VoxelMeshGenerator: MeshFilter 또는 메쉬가 존재하지 않습니다.", gameObject);
            return;
        }

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        // 1. 원본 메시로부터 밀도 그리드 생성하고, 원본 메시의 경계(bounds) 정보를 가져옵니다.
        Debug.Log("1. 복셀 밀도 그리드를 생성합니다...");
        float[,,] densityGrid = CreateDensityGridFromMesh(sourceMeshFilter.sharedMesh, VoxelSize, out Bounds bounds);
        Debug.Log($"   밀도 그리드 생성 완료. 크기: {densityGrid.GetLength(0)}x{densityGrid.GetLength(1)}x{densityGrid.GetLength(2)}");

        // 2. Marching Cubes 알고리즘으로 메시 데이터 생성
        Debug.Log("2. Marching Cubes 알고리즘으로 메시를 생성합니다...");
        GenerateMeshFromDensityGrid(densityGrid, 0.5f);
        Debug.Log($"   메시 생성 완료. 정점 수: {vertices.Count}, 삼각형 수: {triangles.Count / 3}");

        // 3. 최종 메시 생성 및 할당
        Debug.Log("3. Mesh Collider 및 MeshFilter를 설정합니다...");
        if (generatedVoxelObject == null)
        {
            generatedVoxelObject = new GameObject(sourceMeshFilter.name + "_VoxelCollider");
            generatedVoxelObject.transform.SetParent(this.transform, false);
        }

        // --- 최종 오프셋 해결 부분 ---
        // 생성된 오브젝트의 로컬 위치를 원본 메시의 경계 최소 지점(bounds.min)으로 설정합니다.
        // 이를 통해 메시의 시작 위치를 정확하게 맞춰줍니다.
        generatedVoxelObject.transform.localPosition = bounds.min;
        // --- 여기까지 ---

        Mesh combinedVoxelMesh = new Mesh();
        combinedVoxelMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedVoxelMesh.SetVertices(vertices);
        combinedVoxelMesh.SetTriangles(triangles, 0);
        combinedVoxelMesh.RecalculateNormals();

        // 안정적인 방식으로 컴포넌트에 메시 할당
        MeshFilter mf = generatedVoxelObject.GetComponent<MeshFilter>();
        if (mf == null) { mf = generatedVoxelObject.AddComponent<MeshFilter>(); }
        mf.sharedMesh = combinedVoxelMesh;

        MeshRenderer mr = generatedVoxelObject.GetComponent<MeshRenderer>();
        if (mr == null) { mr = generatedVoxelObject.AddComponent<MeshRenderer>(); }
        mr.sharedMaterial = VoxelMaterial;

        MeshCollider mc = generatedVoxelObject.GetComponent<MeshCollider>();
        if (mc == null) { mc = generatedVoxelObject.AddComponent<MeshCollider>(); }
        mc.sharedMesh = combinedVoxelMesh;
        mc.convex = true;

        sw.Stop();
        Debug.Log($"<color=green>생성 완료! ({sw.ElapsedMilliseconds}ms)</color> " + generatedVoxelObject.name + "를 확인하세요.");
    }

    /// <summary>
    /// 원본 메시의 정점 위치를 기반으로 3D 밀도 그리드를 생성하고, 원본 메시의 경계를 반환합니다.
    /// </summary>
    private float[,,] CreateDensityGridFromMesh(Mesh mesh, float voxelSize, out Bounds bounds)
    {
        bounds = mesh.bounds;

        Vector3Int gridSize = new Vector3Int(
            Mathf.CeilToInt(bounds.size.x / voxelSize),
            Mathf.CeilToInt(bounds.size.y / voxelSize),
            Mathf.CeilToInt(bounds.size.z / voxelSize)
        );
        if (gridSize.x == 0) gridSize.x = 1;
        if (gridSize.y == 0) gridSize.y = 1;
        if (gridSize.z == 0) gridSize.z = 1;

        float[,,] densityGrid = new float[gridSize.x + 1, gridSize.y + 1, gridSize.z + 1];

        foreach (var vertex in mesh.vertices)
        {
            Vector3 relativePos = vertex - bounds.min;
            Vector3Int voxelCoord = new Vector3Int(
                Mathf.FloorToInt(relativePos.x / voxelSize),
                Mathf.FloorToInt(relativePos.y / voxelSize),
                Mathf.FloorToInt(relativePos.z / voxelSize)
            );

            if (voxelCoord.x < gridSize.x && voxelCoord.y < gridSize.y && voxelCoord.z < gridSize.z &&
                voxelCoord.x >= 0 && voxelCoord.y >= 0 && voxelCoord.z >= 0)
            {
                densityGrid[voxelCoord.x, voxelCoord.y, voxelCoord.z] = 1.0f;
            }
        }
        return densityGrid;
    }

    /// <summary>
    /// 밀도 그리드를 기반으로 Marching Cubes 알고리즘을 실행하여 메시를 생성합니다.
    /// </summary>
    private void GenerateMeshFromDensityGrid(float[,,] densityGrid, float threshold)
    {
        vertices.Clear();
        triangles.Clear();
        vertexIndexMap.Clear();

        for (int x = 0; x < densityGrid.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < densityGrid.GetLength(1) - 1; y++)
            {
                for (int z = 0; z < densityGrid.GetLength(2) - 1; z++)
                {
                    MarchCube(new Vector3Int(x, y, z), threshold, densityGrid);
                }
            }
        }
    }

    /// <summary>
    /// 단일 큐브(복셀)에 대한 Marching Cubes를 수행하여 삼각형을 생성합니다.
    /// </summary>
    private void MarchCube(Vector3Int index, float threshold, float[,,] chunkData)
    {
        // 정점 위치를 (0,0,0)을 기준으로 계산합니다. 최종 위치는 GameObject의 transform으로 조절됩니다.
        Vector3[] points =
        {
            (new Vector3(index.x + 0, index.y + 0, index.z + 0) * VoxelSize),
            (new Vector3(index.x + 1, index.y + 0, index.z + 0) * VoxelSize),
            (new Vector3(index.x + 1, index.y + 0, index.z + 1) * VoxelSize),
            (new Vector3(index.x + 0, index.y + 0, index.z + 1) * VoxelSize),
            (new Vector3(index.x + 0, index.y + 1, index.z + 0) * VoxelSize),
            (new Vector3(index.x + 1, index.y + 1, index.z + 0) * VoxelSize),
            (new Vector3(index.x + 1, index.y + 1, index.z + 1) * VoxelSize),
            (new Vector3(index.x + 0, index.y + 1, index.z + 1) * VoxelSize)
        };

        float[] values =
        {
            chunkData[index.x + 0, index.y + 0, index.z + 0], chunkData[index.x + 1, index.y + 0, index.z + 0],
            chunkData[index.x + 1, index.y + 0, index.z + 1], chunkData[index.x + 0, index.y + 0, index.z + 1],
            chunkData[index.x + 0, index.y + 1, index.z + 0], chunkData[index.x + 1, index.y + 1, index.z + 0],
            chunkData[index.x + 1, index.y + 1, index.z + 1], chunkData[index.x + 0, index.y + 1, index.z + 1]
        };
        
        int cubeIndex = 0;
        if (values[0] < threshold) cubeIndex |= 1;
        if (values[1] < threshold) cubeIndex |= 2;
        if (values[2] < threshold) cubeIndex |= 4;
        if (values[3] < threshold) cubeIndex |= 8;
        if (values[4] < threshold) cubeIndex |= 16;
        if (values[5] < threshold) cubeIndex |= 32;
        if (values[6] < threshold) cubeIndex |= 64;
        if (values[7] < threshold) cubeIndex |= 128;

        if (Tables.edgeTable[cubeIndex] == 0) return;

        Vector3[] vertList = new Vector3[12];
        if ((Tables.edgeTable[cubeIndex] & 1) != 0) { vertList[0] = Interpolate(points[0], points[1], values[0], values[1], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 2) != 0) { vertList[1] = Interpolate(points[1], points[2], values[1], values[2], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 4) != 0) { vertList[2] = Interpolate(points[2], points[3], values[2], values[3], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 8) != 0) { vertList[3] = Interpolate(points[3], points[0], values[3], values[0], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 16) != 0) { vertList[4] = Interpolate(points[4], points[5], values[4], values[5], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 32) != 0) { vertList[5] = Interpolate(points[5], points[6], values[5], values[6], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 64) != 0) { vertList[6] = Interpolate(points[6], points[7], values[6], values[7], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 128) != 0) { vertList[7] = Interpolate(points[7], points[4], values[7], values[4], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 256) != 0) { vertList[8] = Interpolate(points[0], points[4], values[0], values[4], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 512) != 0) { vertList[9] = Interpolate(points[1], points[5], values[1], values[5], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 1024) != 0) { vertList[10] = Interpolate(points[2], points[6], values[2], values[6], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 2048) != 0) { vertList[11] = Interpolate(points[3], points[7], values[3], values[7], threshold); }

        for (int i = 0; Tables.triangleTable[cubeIndex, i] != -1; i += 3)
        {
            Vector3 v1 = vertList[Tables.triangleTable[cubeIndex, i + 2]];
            Vector3 v2 = vertList[Tables.triangleTable[cubeIndex, i + 1]];
            Vector3 v3 = vertList[Tables.triangleTable[cubeIndex, i + 0]];

            triangles.Add(GetVertexIndex(v1));
            triangles.Add(GetVertexIndex(v2));
            triangles.Add(GetVertexIndex(v3));
        }
    }

    private Vector3 Interpolate(Vector3 p1, Vector3 p2, float v1, float v2, float threshold)
    {
        if (Mathf.Abs(v1 - v2) < 0.00001f)
        {
            return p1;
        }
        float t = (threshold - v1) / (v2 - v1);
        return p1 + t * (p2 - p1);
    }

    private int GetVertexIndex(Vector3 vert)
    {
        if (vertexIndexMap.TryGetValue(vert, out int index))
        {
            return index;
        }
        
        vertices.Add(vert);
        int newIndex = vertices.Count - 1;
        vertexIndexMap.Add(vert, newIndex);
        return newIndex;
    }
}
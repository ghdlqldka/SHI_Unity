
// BlastingTask/Plane_and_NV.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System.Collections; // IEnumerator 사용을 위해 추가
using UnityEngine.Networking; // UnityWebRequest 사용을 위해 추가
using _SHI_BA; // _SHI_BA 네임스페이스를 사용하도록 추가합니다.


public class Plane_and_NV : MonoBehaviour
{
    private static string LOG_FORMAT = "<color=#25E04E><b>[Plane_and_NV]</b></color> {0}";

    [SerializeField]
    protected _BA_LoadPlaneJson _jsonLoader;
    public _BA_LoadPlaneJson JsonLoader
    {
        get
        {
            return _jsonLoader;
        }
    }

    [Header("시각화 재질 설정")]
    [Tooltip("R 계열 면에 적용할 Material 입니다.")]
    public Material rMaterial;
    [Tooltip("W 계열 면에 적용할 Material 입니다.")]
    public Material wMaterial;


    [Header("시각화 상세 설정")]
    [Tooltip("면 법선 방향으로 띄울 거리입니다.")]
    public float normalOffset = 0.1f;
    [Tooltip("시각화 면의 두께입니다.")]
    public float visualThickness = 0.02f;
    private List<GameObject> visualizerObjects = new List<GameObject>();
    // #endregion

    private void Awake()
    {
        Debug.LogFormat(LOG_FORMAT, "Awake()");

        Debug.Assert(_jsonLoader != null);
    }

    // #region 핵심 로직
    // [ContextMenu("1. [Editor] JSON 데이터 로드 및 적용")] // ContextMenu는 EditorOnly 속성이므로 Coroutine과 직접 연결하기 어렵습니다.
    // 기존 LoadDataFromJSONInEditor 메서드는 내부에서 LoadDataCoroutine을 시작하도록 변경하거나, 필요 없으면 제거할 수 있습니다.
    public void LoadDataFromJSONInEditor()
    {
        StartCoroutine(_jsonLoader.LoadDataCoroutine());
    }

    // #region 시각화 관련 메서드
    [ContextMenu("2. [Editor] 두꺼운 면 시각화 켜기/끄기")]
    public void ToggleThickPlaneVisibility()
    {
        // 현재 시각화된 오브젝트가 있다면 모두 지웁니다.
        if (visualizerObjects.Count > 0)
        {
            ClearVisualizers();
        }
        else
        {
            // 데이터가 로드되지 않았다면 경고 메시지를 출력하고 리턴합니다.
            if (JsonLoader.IsDataLoaded == false)
            {
                Debug.LogWarning("먼저 JSON 데이터를 로드하세요.");
                return;
            }
            // 데이터가 로드되었다면 모든 두꺼운 면을 생성합니다.
            CreateAllThickPlanes();
        }
    }

    private void CreateAllThickPlanes()
    {
        ClearVisualizers(); // 기존 시각화 객체를 먼저 지웁니다.
        // R 계열 및 W 계열의 모든 면에 대해 두꺼운 면을 생성합니다.
        foreach (var cube in JsonLoader.R_Faces) { CreateThickPlane(cube); }
        foreach (var cube in JsonLoader.W_Faces.Values) { CreateThickPlane(cube); }
    }

    private void CreateThickPlane(BA_Motion.Cube cube)
    {
        // 시각화 오브젝트를 생성하고 이 스크립트의 하위로 설정합니다.
        GameObject planeObject = new GameObject("Visualizer_" + cube.Name);
        planeObject.transform.SetParent(this.transform);

        // MeshFilter와 MeshRenderer 컴포넌트를 추가합니다.
        MeshFilter meshFilter = planeObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = planeObject.AddComponent<MeshRenderer>();

        // 면 이름에 따라 적절한 Material을 적용합니다.
        if (cube.Name.StartsWith("W", StringComparison.OrdinalIgnoreCase))
        {
            meshRenderer.material = this.wMaterial;
        }
        else
        {
            meshRenderer.material = this.rMaterial;
        }

        // 법선 방향으로의 오프셋을 계산하여 면의 앞면 꼭짓점을 정의합니다.
        Vector3 offsetVector = cube.normal * normalOffset;
        Vector3 v0_front = cube.R1 + offsetVector;
        Vector3 v1_front = cube.R2 + offsetVector;
        Vector3 v2_front = cube.R3 + offsetVector;
        Vector3 v3_front = cube.R4 + offsetVector;

        // 면의 두께를 적용하여 뒷면 꼭짓점을 정의합니다.
        Vector3 back_offset = -cube.normal * visualThickness;
        Vector3 v4_back = v0_front + back_offset;
        Vector3 v5_back = v1_front + back_offset;
        Vector3 v6_back = v2_front + back_offset;
        Vector3 v7_back = v3_front + back_offset;

        // 메쉬를 생성하고 모든 꼭짓점과 삼각형 인덱스를 할당합니다.
        Mesh mesh = new Mesh
        {
            vertices = new Vector3[] { v0_front, v1_front, v2_front, v3_front, v4_back, v5_back, v6_back, v7_back },
            // 큐브의 12개 삼각형 (6개 면 * 2개 삼각형)을 정의합니다.
            triangles = new int[] { 
                // 앞면 (v0_front, v1_front, v2_front, v3_front)
                0, 1, 3,    // v0_front, v1_front, v3_front
                0, 3, 2,    // v0_front, v3_front, v2_front
                
                // 뒷면 (v4_back, v5_back, v6_back, v7_back) - 뒤집힌 순서로 정의하여 법선이 바깥쪽을 향하도록
                5, 4, 6,    // v5_back, v4_back, v6_back
                5, 6, 7,    // v5_back, v6_back, v7_back

                // 왼쪽 면 (v0_front, v3_front, v7_back, v4_back)
                4, 0, 3,    // v4_back, v0_front, v3_front
                4, 3, 7,    // v4_back, v3_front, v7_back

                // 오른쪽 면 (v1_front, v5_back, v6_back, v2_front)
                1, 5, 6,    // v1_front, v5_back, v6_back
                1, 6, 2,    // v1_front, v6_back, v2_front

                // 윗면 (v0_front, v1_front, v5_back, v4_back)
                0, 4, 5,    // v0_front, v4_back, v5_back
                0, 5, 1,    // v0_front, v5_back, v1_front

                // 아랫면 (v3_front, v2_front, v6_back, v7_back)
                3, 2, 6,    // v3_front, v2_front, v6_back
                3, 6, 7     // v3_front, v6_back, v7_back
            }
        };
        // 노말과 바운드를 다시 계산하여 올바른 렌더링을 보장합니다.
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        // 메쉬필터에 생성된 메쉬를 할당합니다.
        meshFilter.mesh = mesh;
        // 생성된 시각화 오브젝트를 추적 리스트에 추가합니다.
        visualizerObjects.Add(planeObject);

        // 각 꼭짓점 위에 라벨을 생성합니다.
        CreateVertexLabels(cube, offsetVector);
    }

    // 각 면의 꼭짓점에 번호 라벨을 생성하는 헬퍼 메서드
    private void CreateVertexLabels(BA_Motion.Cube cube, Vector3 offset)
    {
        // 큐브의 네 꼭짓점과 해당 라벨을 딕셔너리로 정의합니다.
        var vertices = new Dictionary<string, Vector3>
        {
            { "1", cube.R1 }, { "2", cube.R2 }, { "3", cube.R3 }, { "4", cube.R4 }
        };

        foreach (var vertex in vertices)
        {
            // 각 꼭짓점 라벨 오브젝트를 생성하고 부모를 설정합니다.
            GameObject labelObj = new GameObject("Label_" + cube.Name + "_" + vertex.Key);
            labelObj.transform.SetParent(this.transform);
            // 라벨의 위치는 꼭짓점 위치에 오프셋과 약간의 법선 방향 오프셋을 더합니다.
            labelObj.transform.position = vertex.Value + offset + (cube.normal * 0.1f);

            // TextMesh 컴포넌트를 추가하고 텍스트 및 스타일을 설정합니다.
            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = vertex.Key; // 꼭짓점 번호 (예: "1", "2")

            // 폰트 크기 및 문자 크기를 조정하여 가시성을 높입니다.
            textMesh.fontSize = 10;
            textMesh.characterSize = 0.1f;
            textMesh.color = Color.black;
            textMesh.anchor = TextAnchor.MiddleCenter; // 텍스트 앵커를 중앙으로 설정

            // 라벨이 항상 카메라를 바라보도록 Billboard 컴포넌트를 추가합니다.
            labelObj.AddComponent<Billboard>();

            // 생성된 라벨 오브젝트를 추적 리스트에 추가합니다.
            visualizerObjects.Add(labelObj);
        }
    }

    // 생성된 모든 시각화 오브젝트를 제거하는 헬퍼 메서드
    private void ClearVisualizers()
    {
        // 리스트를 역순으로 순회하며 오브젝트를 파괴하여 리스트 수정 중 오류를 방지합니다.
        for (int i = visualizerObjects.Count - 1; i >= 0; i--)
        {
            if (visualizerObjects[i] != null)
            {
                // 플레이 모드와 에디터 모드에 따라 적절한 파괴 메서드를 사용합니다.
                if (Application.isPlaying) Destroy(visualizerObjects[i]);
                else DestroyImmediate(visualizerObjects[i]);
            }
        }
        visualizerObjects.Clear(); // 리스트를 비웁니다.
    }
    // #endregion
}


// 라벨이 항상 카메라를 바라보도록 하는 Billboard 컴포넌트
public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 메인 카메라를 찾습니다. (씬에 "MainCamera" 태그가 지정된 카메라가 있어야 합니다.)
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 메인 카메라가 존재하면 라벨이 카메라를 바라보도록 회전시킵니다.
        if (mainCamera != null)
        {
            this.transform.LookAt(this.transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}

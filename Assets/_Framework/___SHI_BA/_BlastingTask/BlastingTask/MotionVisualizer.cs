using UnityEngine;
using System.Collections.Generic;
using _SHI_BA;
using System.Linq;

/// <summary>
/// 경로, 좌표계 등 모션 계획과 관련된 정보들을 Scene에 시각적으로 표현하는 클래스.
/// 기존 Plane_Motion_Planning의 시각화 알고리즘을 그대로 보존합니다.
/// WebGL 호환성을 위해 셰이더 로딩 부분을 개선했습니다.
/// </summary>
public class MotionVisualizer
{
    private Transform visualParent;
    private List<GameObject> createdVisuals = new List<GameObject>(); // 생성된 GameObject들을 추적
    private GameObject pointPrefab; // [1] 전달받은 프리팹을 저장할 변수 추가

    // Material들을 미리 로드하거나 생성하고 재활용합니다.
    private Material lineMaterial; // LineRenderer 전용 Material

    private readonly Color[] faceColors = {
        Color.magenta, Color.cyan, Color.yellow, Color.green,
        new Color(1f, 0.5f, 0f), new Color(0.5f, 0f, 1f), Color.white, Color.gray
    };

    /// <summary>
    /// 생성자: 시각적 요소들을 그룹화할 부모 Transform을 지정합니다.
    /// </summary>
    public MotionVisualizer(Transform parent, GameObject prefab)
    {
        this.visualParent = parent;
        this.pointPrefab = prefab; // [3] 전달받은 프리팹을 저장합니다.
        InitializeMaterials(); // Material 초기화
    }

    // Material들을 미리 초기화하거나, 필요할 때 한 번만 로드/생성하도록 수정
    private void InitializeMaterials()
    {
        // LineRenderer용 Material만 필요
        Shader defaultShader = Shader.Find("Sprites/Default");
        Shader unlitShader = Shader.Find("Unlit/Color");

        if (defaultShader != null)
        {
            lineMaterial = new Material(defaultShader);
            //Debug.Log($"MotionVisualizer: Sprites/Default 셰이더 사용 (라인)");
        }
        else if (unlitShader != null)
        {
            lineMaterial = new Material(unlitShader);
            //Debug.Log($"MotionVisualizer: Unlit/Color 셰이더 사용 (라인)");
        }
        else
        {
            lineMaterial = Resources.GetBuiltinResource<Material>("Default-Line.mat");
            //Debug.Log($"MotionVisualizer: Built-in Default-Line 머티리얼 사용 (라인)");
        }
    }

    /// <summary>
    /// 단일 경로에 대한 시각적 요소를 그립니다 (이제 경로 라인만 그립니다).
    /// </summary>
    public void VisualizePath(BA_MotionPath path)
    {
        if (path == null) return;

        // 기존 경로 라인 그리기
        DrawPathLine(path);

        // ▼▼▼ [추가] 경로의 각 지점에 targetPrefab을 생성하는 로직 ▼▼▼
        if (pointPrefab != null)
        {
            for (int i = 0; i < path.PointList.Count; i++)
            {
                Vector3 position = path.PointList[i].position;
                Quaternion rotation = Quaternion.Euler(path.PointList[i].eulerAngles);
                
                // 프리팹을 생성하고, 관리 리스트에 추가합니다.
                GameObject waypointVisual = Object.Instantiate(pointPrefab, position, rotation, visualParent);
                waypointVisual.name = $"Waypoint_{path.FaceIndex}_{i}";
                createdVisuals.Add(waypointVisual);
            }
        }
    }

    /// <summary>
    /// Scene에 생성된 모든 시각적 요소들을 삭제합니다.
    /// 생성된 Material과 Mesh도 명시적으로 해제합니다.
    /// </summary>
    public void ClearAllVisuals()
    {
        // 생성된 GameObject들을 역순으로 순회하며 파괴 (리스트 수정에 안전)
        for (int i = createdVisuals.Count - 1; i >= 0; i--)
        {
            GameObject visual = createdVisuals[i];
            if (visual == null) continue;

            // LineRenderer가 있는 경우 Material 해제 (LineRenderer가 자신만의 Material 인스턴스를 생성했다면)
            LineRenderer lr = visual.GetComponent<LineRenderer>();
            if (lr != null && lr.material != null && lr.material != lineMaterial) // 미리 할당된 Material이 아니라면
            {
                if (Application.isPlaying)
                    Object.Destroy(lr.material);
                else
                    Object.DestroyImmediate(lr.material);
                lr.material = null;
            }
            
            // GameObject 자체 파괴
            if (Application.isPlaying)
                Object.Destroy(visual);
            else
                Object.DestroyImmediate(visual);
        }
        createdVisuals.Clear();
        //Debug.Log("[MotionVisualizer] 모든 시각적 요소가 제거되었습니다.");
    }
    
    // 이 클래스 인스턴스가 파괴될 때 모든 Material도 해제합니다.
    public void OnDestroy()
    {
        ClearAllVisuals(); // 남아있는 비주얼도 정리

        if (lineMaterial != null)
        {
            if (Application.isPlaying) Object.Destroy(lineMaterial);
            else Object.DestroyImmediate(lineMaterial);
            lineMaterial = null;
        }
        // 이 버전에서는 sphereMaterial, arrowMaterial, coneMaterial, cachedConeMesh를 사용하지 않으므로 해제 로직이 필요 없습니다.
        //Debug.Log("[MotionVisualizer] 모든 Material이 해제되었습니다.");
    }

    // ==================================================================
    // 아래는 private 헬퍼 메소드들
    // ==================================================================

    private void DrawPathLine(BA_MotionPath path)
    {
        if (path.PointList.Count < 2)
            return;

        GameObject lineObj = new GameObject($"ZigzagPath_Face_{path.FaceIndex}");
        lineObj.hideFlags = HideFlags.HideAndDontSave; // 씬 계층에서 숨기고 저장하지 않음
        lineObj.transform.SetParent(visualParent, false);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = path.PointList.Count;
        Vector3[] positions = path.PointList.Select(p => p.position).ToArray();
        lr.SetPositions(positions);
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.useWorldSpace = true;

        // 미리 로드된 Material을 사용합니다.
        lr.material = lineMaterial;

        Color lineColor = faceColors[(path.FaceIndex - 1) % faceColors.Length];
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        createdVisuals.Add(lineObj);
    }

    // 구체, 화살표, 텍스트 라벨 관련 모든 메서드와 필드를 삭제했으므로 여기에 포함되지 않습니다.
    // Plane_and_NV.cs에서 생성되는 "면" 시각화는 MotionVisualizer와는 별개로 처리됩니다.
}
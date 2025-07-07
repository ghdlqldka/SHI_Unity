using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _SHI_BA; // BioIK 네임스페이스 추가
using BioIK;
using static EA.Line3D._LineRenderer3D; // BioIK 네임스페이스 추가

/// <summary>
/// This script visualizes the R7 face and can generate motion paths on it.
/// 1. The R7 face is rendered as a mesh plane using its four corner vertices.
/// 2. It can generate and visualize a zigzag motion path directly on the R7 plane.
/// 3. It can also visualize projected vectors from edges 5-8.
/// </summary>
public class StiffnerBotMotion : MonoBehaviour
{
    [Header("Core Dependencies")]
    [Tooltip("로봇의 BioIK 컴포넌트를 할당하세요.")]
    public BA_BioIK _bioIK;
    [Tooltip("로봇 IK의 목표가 되는 Target 오브젝트를 할당하세요.")]
    public GameObject ikTarget;
    [Tooltip("로봇의 TCP(Tool Center Point) 오브젝트를 할당하세요.")]
    public Transform tcpTransform;

    [Header("Robot Control Components")]
    public Gauntry_or_6Joint_or_all jointController;
    public RobotStartPose robotStartPose;
    [Header("Component References")]
    [Tooltip("Assign the Plane_and_NV object from your scene here.")]
    public Plane_and_NV planeAndNvReference;

    [Header("Visualization Settings")]
    public Material faceMaterial;
    public Color projectedEdgeColor = Color.cyan;
    public float projectedEdgeWidth = 0.05f;
    public Color zigzagPathColor = Color.magenta;

    [Header("Zigzag Motion Settings")]
    public int numHorizontalPoints = 2;
    public float verticalStep = 0.08f;
    public float normalOffsetForExtension = 0.4f;

    [Header("사용자 설정 변수")]
    [Tooltip("각도 계산에 사용될 기준 값입니다.")]
    public float angle_bps = 60.0f;

    [Header("Path Point Visualization")]
    [Tooltip("경로의 각 지점에 표시할 프리팹을 여기에 할당하세요.")]
    public GameObject targetPrefab;

    [Tooltip("XML 생성을 위한 Make_XML 인스턴스를 할당하세요.")]
    public Make_XML makeXmlInstance;
    public XMLSender xmlSender;
    private XmlPathRecorder xmlRecorder;
    // --- 내부 변수 ---
    private List<GameObject> visualObjects = new List<GameObject>();
    private Coroutine motionCoroutine;
    private bool isExecuting = false;







    // List<_LinePoint> path = new List<_LinePoint>();









    void Awake()
    {
        xmlRecorder = new XmlPathRecorder();
    }
    [ContextMenu("Generate Path")]
    public void GeneratePath()
    {
        BA_PathDataManager.Instance.path_stiffnerBot = VisualizeZigzagMotionOnFace();
    }

    [ContextMenu("Execute Motion")]
    public void ExecutePath()
    {
        if (isExecuting)
        {
            Debug.LogWarning("현재 다른 모션이 실행 중입니다. 기존 모션을 중지하고 새로 시작합니다.");
            StopMotion();
        }

        // 2. 경로 검증 및 실행
        if (BA_PathDataManager.Instance.path_stiffnerBot != null && BA_PathDataManager.Instance.path_stiffnerBot.Count > 0)
        {
            Debug.Log("경로 생성이 완료되었습니다. 로봇 모션 실행을 시작합니다.");
            motionCoroutine = StartCoroutine(ExecutePathCoroutine(BA_PathDataManager.Instance.path_stiffnerBot));
        }
        else
        {
            Debug.LogError("경로 생성에 실패하여 로봇 모션을 시작할 수 없습니다.");
        }
    }

    /// <summary>
    /// 현재 진행 중인 로봇 모션을 중지합니다.
    /// </summary>
    [ContextMenu("5. Stop Motion")]
    public void StopMotion()
    {
        if (motionCoroutine != null)
        {
            StopCoroutine(motionCoroutine);
            motionCoroutine = null;
        }
        isExecuting = false;
        Debug.Log("로봇 모션이 수동으로 중지되었습니다.");
    }
    // ▲▲▲

    /// <summary>
    /// 생성된 경로를 따라 IK Target을 순차적으로 이동시키는 단순화된 코루틴.
    /// </summary>
    /// <param name="path">실행할 경로 데이터 (위치, 회전)</param>
    private IEnumerator ExecutePathCoroutine(List<_LinePoint> pointList)
    {
        isExecuting = true;

        if (_bioIK == null || ikTarget == null || tcpTransform == null || jointController == null || robotStartPose == null)
        {
            Debug.LogError("핵심 컴포넌트가 모두 할당되지 않았습니다. 모션을 중단합니다.");
            isExecuting = false;
            yield break;
        }

        // ▼▼▼ [요청된 로직 추가] 로봇 초기화 및 자세 설정 단계 ▼▼▼
        Debug.Log("--- 로봇 초기화 시작 ---");

        Debug.Log("Step 1: Setting initial pose ('ready_top2')");
        jointController.EnableOnlyA1toA6();
        _bioIK.autoIK = true;
        yield return StartCoroutine(robotStartPose.PosingSequenceCoroutine("ready_top2", Vector3.down));
        _bioIK.autoIK = false;

        Debug.Log("Step 2: Setting Pan/Tilt angles directly");
        RotateUpboxDirectly(90f);
        RotatedownDirectly(60f);

        Debug.Log("Step 3: Moving Gantry to the path's starting point");
        jointController.EnableOnlyGauntry();
        yield return new WaitForSeconds(0.5f);

        ikTarget.transform.position = pointList[0].position;
        ikTarget.transform.rotation = Quaternion.Euler(pointList[0].eulerAngles);
        yield return new WaitForSeconds(0.5f);

        

        Debug.Log("Step 4: Aligning TCP");
        jointController.EnableOnlyA1toA6();
        yield return StartCoroutine(WaitUntilRotationAligned(5.0f, 3.0f));
        // ▲▲▲ [초기화 로직 완료] ▲▲▲

        Debug.Log("--- 경로 추종 시작 ---");
        jointController.EnableGantryAndA5Orientation(); // 갠트리 제어로 전환

        for (int i = 0; i < pointList.Count; i++)
        {
            _LinePoint targetPoint = pointList[i];
            ikTarget.transform.position = targetPoint.position;
            ikTarget.transform.rotation = Quaternion.Euler(targetPoint.eulerAngles);

            float timeOut = 5f;
            float timer = 0f;
            float thres_pos = 0.01f;
            float thres_rot = 3.0f;
            while (true)
            {
                float dist_err = Vector3.Distance(tcpTransform.position, ikTarget.transform.position);
                float rot_err = Quaternion.Angle(tcpTransform.rotation, ikTarget.transform.rotation);
                // Debug.Log($"포인트 {i + 1} / {path.Count} 도달 중... 거리 오차: {dist_err:F4}, 회전 오차: {rot_err:F2}");
                if (dist_err < thres_pos && rot_err < thres_rot)
                {
                    Debug.Log($"포인트 {i + 1} / {pointList.Count} 도달 완료.");
                    break;
                }

                if (timer > timeOut)
                {
                    Debug.LogWarning($"포인트 {i} 도달 시간 초과! 다음 지점으로 넘어갑니다.");
                    Debug.Log($"현재 위치: {tcpTransform.position}, 목표 위치: {ikTarget.transform.position}");
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (i == 0 || i == -1)
            {
                xmlRecorder.RecordCurrentPose(this.makeXmlInstance);
            }
            else
            {
                xmlRecorder.RecordCurrentPose(this.makeXmlInstance);
            }
            // Debug.Log($"포인트 {i + 1} / {path.Count} 도달 완료.");
        }

        Debug.Log("모든 경로 지점 실행 완료.");
        string finalXmlString = makeXmlInstance.ConvertPoseListToXmlString(xmlRecorder.AllPoses);

        // XML 데이터가 존재하고, XMLSender가 할당되었다면 저장을 요청합니다.
        if (!string.IsNullOrEmpty(finalXmlString))
        {
            if (xmlSender != null)
            {
                    // XMLSender는 현재 환경(에디터/PC 또는 WebGL)에 맞춰 자동으로 파일 저장 또는 서버 전송을 수행합니다.
                    // 파일 이름은 "edge_1_4_motion_1.xml", "_2.xml" 등으로 자동 생성됩니다.
                    xmlSender.SendLargeXmlInChunks(finalXmlString, "StiffnerBotFace"); //, "edge_1_4_motion_1.xml");
            }
            else
            {
                Debug.LogError("[XML Save Failed] XMLSender가 StiffnerEdgeMotion 스크립트에 할당되지 않았습니다.");
            }
        }
        jointController.EnableOnlyGauntry();

        isExecuting = false;
        motionCoroutine = null;
    }
    /// <summary>
    /// This method can be called from the component's context menu in the Inspector.
    /// It visualizes the R7 face and the projected vectors of edges 5-8.
    /// </summary>
    [ContextMenu("1. Visualize R7 Face and Projected Edges 5-8")]
    public void VisualizeFaceAndProjectedEdges()
    {
        ClearVisuals();

        if (planeAndNvReference == null || !planeAndNvReference.JsonLoader.IsDataLoaded)
        {
            Debug.LogError("[StiffnerBotMotion] The Plane_and_NV reference is not set or its data has not been loaded.");
            return;
        }

        Plane? r7Plane = VisualizeR7Face();
        if (r7Plane == null) return;

        VisualizeProjectedEdges(r7Plane.Value);
    }

    /// <summary>
    /// R7 면에 위치와 회전 정보를 포함한 지그재그 경로를 생성 및 시각화하고, 생성된 경로를 반환합니다.
    /// 교차점 웨이포인트에 대해 조건부 회전 값을 적용하고, 경로의 특정 구간에 인덱스를 시각화합니다.
    /// </summary>
    /// <returns>생성된 경로(위치, 회전) 또는 생성 실패 시 null을 반환합니다.</returns>
    [ContextMenu("2. Visualize Zigzag Motion on R7 Face")]
    public List<_LinePoint> VisualizeZigzagMotionOnFace()
    {
        ClearVisuals();

        if (planeAndNvReference == null || !planeAndNvReference.JsonLoader.IsDataLoaded)
        {
            Debug.LogError("[StiffnerBotMotion] The Plane_and_NV reference is not set or its data has not been loaded.");
            return null;
        }

        var r7FaceData = planeAndNvReference.JsonLoader.R_Faces.FirstOrDefault(f => f.Name == "R7");
        if (r7FaceData.Name == null)
        {
            Debug.LogError("[StiffnerBotMotion] R7 face data could not be found.");
            return null;
        }

        // 1. 경로 생성에 필요한 기본 정보 설정
        Plane r7Plane = new Plane(r7FaceData.R1, r7FaceData.R2, r7FaceData.R4);
        var targetEdges = planeAndNvReference.JsonLoader.Edge_List
            .Where(e => e.Name.StartsWith("R7_Edge_") && new[] { "5", "6", "7", "8" }.Contains(e.Name.Split('_').Last()))
            .ToList();

        List<(string Name, Vector3 start, Vector3 end)> projectedEdges = new List<(string, Vector3, Vector3)>();
        foreach (var edge in targetEdges)
        {
            projectedEdges.Add((edge.Name, r7Plane.ClosestPointOnPlane(edge.Start), r7Plane.ClosestPointOnPlane(edge.End)));
        }

        VisualizeR7Face();

        Vector3 startPoint = r7FaceData.R3;
        Vector3 faceNormal = -Vector3.Cross(r7FaceData.R2 - r7FaceData.R1, r7FaceData.R4 - r7FaceData.R1).normalized;
        var allPoints = new List<Vector3> { r7FaceData.R1, r7FaceData.R2, r7FaceData.R3, r7FaceData.R4 };
        var adjacentPoints = allPoints.Where(p => (p - startPoint).sqrMagnitude > 1e-6f).OrderBy(p => (p - startPoint).magnitude).ToList();
        if (adjacentPoints.Count < 2)
        {
            Debug.LogError("[StiffnerBotMotion] Could not determine adjacent points for path generation.");
            return null;
        }

        Vector3 edge1 = adjacentPoints[0] - startPoint;
        Vector3 edge2 = adjacentPoints[1] - startPoint;
        Vector3 widthEdge, heightEdge;
        if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.95f)
        {
            if (edge1.magnitude > edge2.magnitude) { widthEdge = edge1; heightEdge = edge2; } else { widthEdge = edge2; heightEdge = edge1; }
        }
        else
        {
            if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) { heightEdge = edge1; widthEdge = edge2; } else { heightEdge = edge2; widthEdge = edge1; }
        }
        Vector3 heightDir = heightEdge.normalized;
        Vector3 widthDir = widthEdge.normalized;
        float heightLen = heightEdge.magnitude;
        float widthLen = widthEdge.magnitude;
        if (Vector3.Dot(widthDir, widthEdge) < 0) widthDir = -widthDir;

        // 2. 회전값 계산을 위한 변수 추가
        angle_bps = 180.0f - angle_bps; // 180도에서 빼기
        Quaternion defaultRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
        Quaternion reversedRotation = Quaternion.LookRotation(Quaternion.AngleAxis(180 - angle_bps, heightDir) * faceNormal, heightDir);

        List<(Vector3 position, Quaternion rotation)> workPoints = new List<(Vector3, Quaternion)>();
        Vector3 currentPosition = startPoint;
        float movedHeight = 0f;
        bool movingRight = true;
        float extensionDistance = normalOffsetForExtension * Mathf.Tan(30f * Mathf.Deg2Rad);
        float waypointOffset = extensionDistance;

        // 3. 경로 생성 루프 (회전 정보 추가)
        while (movedHeight <= heightLen + 1e-4f)
        {
            Vector3 rowStart = currentPosition;
            Vector3 extendedStartPoint = rowStart - (widthDir * extensionDistance);
            Vector3 extendedEndPoint = rowStart + (widthDir * widthLen) + (widthDir * extensionDistance);

            var basePoints = new List<(Vector3 position, Quaternion rotation)>();
            for (int i = 0; i < numHorizontalPoints; i++)
            {
                float t = (numHorizontalPoints > 1) ? (float)i / (numHorizontalPoints - 1) : 0;
                basePoints.Add((Vector3.Lerp(extendedStartPoint, extendedEndPoint, t), defaultRotation));
            }

            var intersections = new List<Vector3>();
            foreach (var projEdge in projectedEdges)
            {
                if (LineLineIntersection(out Vector3 intersection, extendedStartPoint, widthDir, projEdge.start, (projEdge.end - projEdge.start).normalized))
                {
                    float rowSegmentLength = (extendedEndPoint - extendedStartPoint).magnitude;
                    if (Vector3.Dot(intersection - extendedStartPoint, widthDir) > 0 && Vector3.Dot(intersection - extendedStartPoint, widthDir) < rowSegmentLength &&
                        Vector3.Distance(projEdge.start, intersection) + Vector3.Distance(projEdge.end, intersection) - Vector3.Distance(projEdge.start, projEdge.end) < 0.01f)
                    {
                        intersections.Add(intersection);
                    }
                }
            }

            List<Vector3> sortedIntersections;
            if (movingRight)
                sortedIntersections = intersections.OrderBy(p => Vector3.Dot(p - extendedStartPoint, widthDir)).ToList();
            else
                sortedIntersections = intersections.OrderByDescending(p => Vector3.Dot(p - extendedStartPoint, widthDir)).ToList();

            var currentRowPoints = new List<(Vector3 position, Quaternion rotation)>();
            currentRowPoints.Add(movingRight ? basePoints.First() : basePoints.Last());

            foreach (var interPoint in sortedIntersections)
            {
                if (movingRight)
                {
                    currentRowPoints.Add((interPoint - widthDir * waypointOffset, defaultRotation));
                    currentRowPoints.Add((interPoint + widthDir * waypointOffset, reversedRotation));
                }
                else
                {
                    currentRowPoints.Add((interPoint + widthDir * waypointOffset, reversedRotation));
                    currentRowPoints.Add((interPoint - widthDir * waypointOffset, defaultRotation));
                }
            }

            currentRowPoints.Add(movingRight ? basePoints.Last() : basePoints.First());

            workPoints.AddRange(currentRowPoints.Distinct());

            float remainingHeight = heightLen - movedHeight;
            if (remainingHeight < 0.04f)
            {
                Debug.Log($"[StiffnerBotMotion] 남은 높이({remainingHeight:F2}m)가 0.4m 미만이므로 경로 생성을 중단합니다.");
                break;
            }

            if (movedHeight >= heightLen) break;

            movedHeight = Mathf.Min(movedHeight + verticalStep, heightLen);
            currentPosition = startPoint + heightDir * movedHeight;
            movingRight = !movingRight;
        }

        if (workPoints.Count > 1)
        {
            // 4.1. 오프셋이 적용되지 않은 원본 경로 생성
            var originalPath = new List<(Vector3 position, Quaternion rotation)>();
            float safeZoneDistance = 1.2f;

            Vector3 entryDirection = (workPoints[0].position - workPoints[1].position).normalized;
            originalPath.Add((workPoints[0].position + entryDirection * safeZoneDistance, defaultRotation));
            originalPath.AddRange(workPoints);
            Vector3 newEndPoint = workPoints.Last().position;
            Vector3 exitDirection = (newEndPoint - workPoints[workPoints.Count - 2].position).normalized;
            originalPath.Add((newEndPoint + exitDirection * safeZoneDistance, defaultRotation));

            // ▼▼▼ [최종 수정] 경로에 최종 법선 오프셋 적용 ▼▼▼
            // var finalPath = new List<(Vector3 position, Quaternion rotation)>();
            List<_LinePoint> _pointList = new List<_LinePoint>();
            Vector3 offsetVector = faceNormal * normalOffsetForExtension;

            foreach (var point in originalPath)
            {
                // finalPath.Add((point.position + offsetVector, point.rotation));
                _pointList.Add(new _LinePoint(point.position + offsetVector, point.rotation.eulerAngles));
            }
            // ▲▲▲

#if false //
            // 4.2. 최종 경로 시각화 (finalPath 사용)
            GameObject pathObject = new GameObject("Zigzag_Path_Visual");
            pathObject.transform.SetParent(this.transform);
            visualObjects.Add(pathObject);
            LineRenderer lr = pathObject.AddComponent<LineRenderer>();
            lr.positionCount = _pointList.Count;
            lr.SetPositions(_pointList.Select(p => p.position).ToArray());
            lr.startWidth = projectedEdgeWidth;
            lr.endWidth = projectedEdgeWidth;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = zigzagPathColor;
            lr.endColor = zigzagPathColor;

            if (targetPrefab != null)
            {
                foreach (var point in _pointList)
                {
                    GameObject instantiatedTarget = Instantiate(targetPrefab, point.position, Quaternion.Euler(point.eulerAngles), this.transform);
                    visualObjects.Add(instantiatedTarget);
                }
            }

            if (_pointList.Count > 0)
            {
                int visualizedCount = 0;
                for (int i = 0; i < _pointList.Count; i++)
                {
                    if ((i >= 0 && i <= 10) || (i >= 30 && i <= 40))
                    {
                        GameObject textObject = new GameObject($"Waypoint_Index_{i}");
                        textObject.transform.SetParent(this.transform);
                        textObject.transform.position = _pointList[i].position + Vector3.up * 0.1f;

                        TextMesh textMesh = textObject.AddComponent<TextMesh>();
                        textMesh.text = i.ToString();
                        textMesh.fontSize = 20;
                        textMesh.characterSize = 0.1f;
                        textMesh.color = Color.white;
                        textMesh.anchor = TextAnchor.MiddleCenter;

                        visualObjects.Add(textObject);
                        visualizedCount++;
                    }
                }
                Debug.Log($"[StiffnerBotMotion] 경로에 {visualizedCount}개의 인덱스 번호를 시각화했습니다.");
            }
#endif

            Debug.Log($"[StiffnerBotMotion] 최종 경로 생성 완료. 총 포인트: {_pointList.Count}.");


            ////// @@@@@@@@@@@@@@@@@
            BA_PathLineManager.Instance.LineRendererDic["StiffBot01"].PointList = _pointList;
            ////// @@@@@@@@@@@@@@@@@



            return _pointList;
        }
        else
        {
            Debug.LogWarning("[StiffnerBotMotion] Could not generate enough points for a zigzag path.");
            return null;
        }
    }

    [ContextMenu("3. Clear All Visuals")]
    public void ClearVisuals()
    {
        for (int i = visualObjects.Count - 1; i >= 0; i--)
        {
            if (visualObjects[i] != null)
            {
                if (Application.isPlaying)
                    Destroy(visualObjects[i]);
                else
                    DestroyImmediate(visualObjects[i]);
            }
        }
        visualObjects.Clear();
        Debug.Log("[StiffnerBotMotion] All visual objects have been cleared.");
    }

    private Plane? VisualizeR7Face()
    {
        var r7FaceData = planeAndNvReference.JsonLoader.R_Faces.FirstOrDefault(f => f.Name == "R7");
        if (r7FaceData.Name == null)
        {
            Debug.LogError("[StiffnerBotMotion] R7 face data could not be found.");
            return null;
        }

        GameObject faceObject = new GameObject("R7_Face_Visual");
        faceObject.transform.SetParent(this.transform);
        visualObjects.Add(faceObject);

        MeshFilter meshFilter = faceObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = faceObject.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh { name = "R7_Face_Mesh" };
        mesh.vertices = new Vector3[] { r7FaceData.R1, r7FaceData.R2, r7FaceData.R3, r7FaceData.R4 };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshRenderer.material = faceMaterial != null ? faceMaterial : new Material(Shader.Find("Standard"));

        return new Plane(r7FaceData.R1, r7FaceData.R2, r7FaceData.R4);
    }

    private void VisualizeProjectedEdges(Plane plane)
    {
        var targetEdges = planeAndNvReference.JsonLoader.Edge_List
            .Where(e => e.Name == "R7_Edge_5" || e.Name == "R7_Edge_6" || e.Name == "R7_Edge_7" || e.Name == "R7_Edge_8")
            .ToList();

        if (targetEdges.Count < 4)
        {
            Debug.LogWarning($"[StiffnerBotMotion] Could not find all target edges (5-8). Found: {targetEdges.Count}");
            return;
        }

        Debug.Log($"--- Visualizing Projected Edges 5-8 ({targetEdges.Count} found) ---");

        foreach (var edge in targetEdges)
        {
            Vector3 projectedStart = plane.ClosestPointOnPlane(edge.Start);
            Vector3 projectedEnd = plane.ClosestPointOnPlane(edge.End);

            GameObject lineObj = new GameObject($"Visual_Projected_{edge.Name}");
            lineObj.transform.SetParent(this.transform);
            visualObjects.Add(lineObj);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.startWidth = projectedEdgeWidth;
            lr.endWidth = projectedEdgeWidth;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = projectedEdgeColor;
            lr.endColor = projectedEdgeColor;
            lr.positionCount = 2;
            lr.SetPosition(0, projectedStart);
            lr.SetPosition(1, projectedEnd);
        }
    }

    // 교차점 계산을 위한 헬퍼 함수
    bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        intersection = Vector3.zero;
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        if (crossVec1and2.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        if (Mathf.Abs(planarFactor) < 0.01f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            return false;
        }
    }
    private void RotateUpboxDirectly(float angle)
    {
        BioSegment upboxSegment = _bioIK.FindSegment("upbox");
        if (upboxSegment != null && upboxSegment.Joint != null)
        {
            upboxSegment.Joint.gameObject.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            Debug.LogWarning("[StiffnerBotMotion] upbox GameObject를 찾지 못했습니다.");
        }
    }

    private void RotatedownDirectly(float angle)
    {
        BioSegment downSegment = _bioIK.FindSegment("down");
        if (downSegment != null && downSegment.Joint != null)
        {
            downSegment.Joint.gameObject.transform.localRotation = Quaternion.Euler(angle, 0, 0);
        }
        else
        {
            Debug.LogWarning("[StiffnerBotMotion] 'down' GameObject를 찾지 못했습니다.");
        }
    }
    
    public void ApplyPoseByName(string poseName)
    {
        Debug.Log("@@@@@@@@ApplyPoseByName(), poseName : " + poseName);
        // this.poseName = poseName;
        if (RobotPoses.Presets.ContainsKey(poseName))
            robotStartPose.ApplyPoseWithJointValue(RobotPoses.Presets[poseName]);
        else
            Debug.LogWarning($"[RobotStartPose] '{poseName}' preset이 없습니다.");
    }
    
    public IEnumerator PosingSequenceCoroutine(string poseName, Vector3? faceNormal = null)
    {
        // Debug.Log($"[RobotStartPose] PosingSequenceCoroutine 시작: {poseName}");
        // 1. 목표 자세 설정 (Limit은 일시적으로 해제됨)
        ApplyPoseByName(poseName);

        // [핵심 수정] IK Solver가 최소 한 프레임 실행될 시간을 보장하기 위해 yield return null을 먼저 호출합니다.
        yield return null;

        // 2. 목표 자세에 도달할 때까지 대기
        float timeout = 10.0f;
        float timer = 0f;
        // bool poseReached = false;
        // Debug.Log($"[RobotStartPose] IK가 자세를 잡도록 최대 {timeout}초간 대기합니다 (Limit 해제 상태).");

        while (timer < timeout)
        {
            if (robotStartPose.IsPoseReached())
            {
                // poseReached = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 3. 자세 도달 후, 원래의 Joint Limit을 다시 적용합니다.
        robotStartPose.ReapplyJointLimits(poseName);

        // 4. 자세 잡기에 사용된 JointValue Objective들을 제거합니다.
        robotStartPose.RemoveAllJointValueObjectives();
    }
    private IEnumerator WaitUntilRotationAligned(float threshold, float timeout)
    {
        float timer = 0f;
        while (timer < timeout)
        {
            if (tcpTransform != null && ikTarget != null)
            {
                float rotErr = Quaternion.Angle(tcpTransform.rotation, ikTarget.transform.rotation);
                if (rotErr < threshold) yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Debug.LogWarning($"[StiffnerBotMotion] TCP 회전 정렬 시간 초과 (오차: {Quaternion.Angle(tcpTransform.rotation, ikTarget.transform.rotation):F2}도)");
    }
}
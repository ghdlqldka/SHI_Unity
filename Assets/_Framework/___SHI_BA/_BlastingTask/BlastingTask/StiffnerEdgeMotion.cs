using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BioIK; // 로봇 제어를 위해 BioIK 네임스페이스 추가
using _SHI_BA;
using Unity.VisualScripting;
using static EA.Line3D._LineRenderer3D;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stiffner의 모든 중간 벡터에 대한 6-DOF 왕복 경로를 생성하고,
/// 생성된 경로를 따라 로봇을 움직이거나 시각화하는 스크립트입니다.
/// </summary>
public class StiffnerEdgeMotion : MonoBehaviour
{
    [Header("핵심 컴포넌트 참조")]
    [Tooltip("씬에 있는 Plane_and_NV 오브젝트를 할당해야 합니다.")]
    public Plane_and_NV planeAndNvReference;
    [Tooltip("경로의 각 지점에 생성할 6-DOF 포즈 시각화용 프리팹입니다.")]
    public GameObject targetPrefab;

    [Header("로봇 제어 컴포넌트 (Motion 실행용)")]
    [Tooltip("로봇의 BioIK 컴포넌트를 할당하세요.")]
    public BA_BioIK _bioIK;
    [Tooltip("로봇 IK의 목표가 되는 Target 오브젝트를 할당하세요.")]
    public GameObject targetObj;
    [Tooltip("로봇의 TCP(Tool Center Point) 오브젝트를 할당하세요.")]
    public Transform TCP;
    [Tooltip("로봇의 관절 및 갠트리 제어 스크립트를 할당하세요.")]
    public Gauntry_or_6Joint_or_all jointController;
    [Tooltip("로봇의 초기 자세 설정 스크립트를 할당하세요.")]
    public RobotStartPose robotStartPose;
    [Tooltip("위빙 각도를 적용할 오브젝트를 할당하세요.")]
    public GameObject weaving2;
    [Tooltip("XML 생성을 위한 Make_XML 인스턴스를 할당하세요.")]
    public Make_XML makeXmlInstance;
    public XMLSender xmlSender;
    private XmlPathRecorder xmlRecorder;

    [Header("사용자 설정 변수")]
    public float angle_bps = 60.0f;
    public float plane_offset = 0.4f;
    public float safetyDistance = 0.7f;
    private float spd = 44.0f;
    private float acc = 22.0f;
    private float onoff = 1.0f;

    [Header("시각화 설정")]
    public Color finalPathColor = Color.cyan;
    public Color indexTextColor = Color.white;
    public float pointSize = 0.05f;
    public float visualOffsetDistance = 0.2f;
    
    // 내부 상태 변수
    private List<GameObject> visualObjects = new List<GameObject>();
    private bool isExecuting = false;
    private Coroutine motionCoroutine;
    private List<float> weavingList = null;

    void Awake()
    {
        xmlRecorder = new XmlPathRecorder();
    }
    #region --- 공개 메서드 및 컨텍스트 메뉴 ---

    [ContextMenu("1. Generate Path")]
    public void GenerateMotion()
    {
        if (isExecuting)
        {
            Debug.LogWarning("이미 다른 모션이 실행 중입니다.");
            return;
        }
        
        ClearVisuals();
        weavingList = null;

        GeneratePathData(out BA_PathDataManager.Instance.path_stiffnerEdge, out weavingList);
        if (BA_PathDataManager.Instance.path_stiffnerEdge == null || BA_PathDataManager.Instance.path_stiffnerEdge.Count == 0)
        {
            Debug.LogError("경로 생성에 실패하여 로봇 모션을 시작할 수 없습니다.");
            return;
        }

#if false //@@@@@@@@@@@@@@@@
        VisualizePath(path);
#endif
        
        
    }

    [ContextMenu("2. EXECUTE MOTION")]
    public void ExecuteMotion()
    {
        if (isExecuting)
        {
            Debug.LogWarning("이미 다른 모션이 실행 중입니다.");
            return;
        }

        motionCoroutine = StartCoroutine(ExecuteMotionCoroutine(BA_PathDataManager.Instance.path_stiffnerEdge, weavingList));

#if false //@@@@@@@@@@@@@@@@
        VisualizePath(path);
#endif


    }
 
#if false //@@@@@@@@@@@@@@@@
    [ContextMenu("2. Visualize Path Only")]
    public void VisualizeAll()
    {
        ClearVisuals();
        var path = GeneratePathData();
        if (path == null)
            return;
        VisualizePath(path);
    }
#endif

    [ContextMenu("3. STOP MOTION")]
    public void StopMotion()
    {
        if (motionCoroutine != null)
        {
            StopCoroutine(motionCoroutine);
            motionCoroutine = null;
        }
        isExecuting = false;
        
        // 모션 중지 시 weaving2 각도 초기화
        if (weaving2 != null)
        {
            weaving2.transform.localRotation = Quaternion.identity;
            Debug.Log("Weaving2 각도가 0으로 초기화되었습니다.");
        }
        
        Debug.Log("로봇 모션이 수동으로 중지되었습니다.");
    }
    
    [ContextMenu("4. 모든 시각화 제거")]
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
    }

#endregion

    #region --- 경로 생성 로직 ---

    private void GeneratePathData(out List<_LinePoint> pointList, out List<float> weavingList)
    {
        if (planeAndNvReference == null || !planeAndNvReference.JsonLoader.IsDataLoaded)
        {
            Debug.LogError("Plane_and_NV 참조가 없거나 데이터가 로드되지 않았습니다.");
            pointList = null;
            weavingList = null;
            return;
        }

        var r7Face = planeAndNvReference.JsonLoader.R_Faces.FirstOrDefault(f => f.Name == "R7");
        if (r7Face.Name == null) 
        { 
            Debug.LogError("R7 면 데이터를 찾을 수 없습니다.");
            pointList = null;
            weavingList = null;
            return; 
        }
        
        Vector3 r7Normal = -Vector3.Cross(r7Face.R2 - r7Face.R1, r7Face.R4 - r7Face.R1).normalized;
        var edges = planeAndNvReference.JsonLoader.Edge_List
            .Where(e => e.Name.StartsWith("R7_Edge_"))
            .ToDictionary(e => e.Name, e => e);

        if (edges.Count < 8) 
        { 
            Debug.LogError("R7의 Edge 1~8 데이터를 모두 찾을 수 없습니다.");
            pointList = null;
            weavingList = null;
            return;
        }

        Plane r7Plane = new Plane(r7Face.R1, r7Face.R2, r7Face.R4);
        var projectedEdges = new Dictionary<string, (Vector3 start, Vector3 end)>();
        for (int i = 1; i <= 8; i++)
        {
            string edgeName = $"R7_Edge_{i}";
            if (edges.ContainsKey(edgeName))
            {
                var edge = edges[edgeName];
                projectedEdges.Add(edgeName, (r7Plane.ClosestPointOnPlane(edge.Start), r7Plane.ClosestPointOnPlane(edge.End)));
            }
        }


        // var masterPath = new List<(Vector3 position, Quaternion rotation, float weavingAngle)>();
        var masterPath = new List<float>();
        List<_LinePoint> _pointList = new List<_LinePoint>();

        for (int i = 1; i <= 3; i++)
        {
            var edge1_proj = projectedEdges[$"R7_Edge_{i}"];
            var edge2_proj = projectedEdges[$"R7_Edge_{i+1}"];
            Vector3 originalStartPoint = (edge1_proj.start + edge2_proj.start) / 2;
            Vector3 originalEndPoint = (edge1_proj.end + edge2_proj.end) / 2;
            Vector3 offsetStartPoint = originalStartPoint + r7Normal * plane_offset;
            Vector3 offsetEndPoint = originalEndPoint + r7Normal * plane_offset;
            Vector3 frontDirection = (offsetEndPoint - offsetStartPoint).normalized;
            Vector3 start_safety = offsetStartPoint - frontDirection * safetyDistance;
            Vector3 end_safety = offsetEndPoint + frontDirection * safetyDistance;
            
            var adjacentPoints = new Vector3[] {edges[$"R7_Edge_{i}"].Start, edges[$"R7_Edge_{i+1}"].Start};
            Vector3 heightDir = (adjacentPoints[0] - adjacentPoints[1]).normalized;
            Quaternion defaultRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * r7Normal, heightDir);
            Quaternion reversedRotation = Quaternion.LookRotation(Quaternion.AngleAxis(180 - angle_bps, heightDir) * r7Normal, heightDir);
            
            var edge1_orig = edges[$"R7_Edge_{i}"];
            var edge2_orig = edges[$"R7_Edge_{i+1}"];
            float startAngle = Vector3.Angle(edge1_orig.Start - offsetStartPoint, edge2_orig.Start - offsetStartPoint);
            float weavingAngle = startAngle / 2.0f;

            // Quaternion forwardWeaveRotation = defaultRotation * Quaternion.Euler(0, 0, weavingAngle);
            // Quaternion backwardWeaveRotation = reversedRotation * Quaternion.Euler(0, 0, -weavingAngle); // 복귀 시에는 반대 방향으로 위빙
            List<Vector3> calculatedPoints = new List<Vector3>();
            for(int j = 5; j <= 8; j++)
            {
                string projectedEdgeName = $"R7_Edge_{j}";
                if (projectedEdges.ContainsKey(projectedEdgeName))
                {
                    var projectedEdge = projectedEdges[projectedEdgeName];
                    if (LineLineIntersection(out Vector3 intersectionPoint, originalStartPoint, (originalEndPoint - originalStartPoint).normalized, projectedEdge.start, (projectedEdge.end - projectedEdge.start).normalized))
                    {
                        float offsetDistance = plane_offset * Mathf.Tan(Mathf.Deg2Rad * (90f - angle_bps));
                        Vector3 direction = (originalEndPoint - originalStartPoint).normalized;
                        calculatedPoints.Add((intersectionPoint + direction * offsetDistance) + r7Normal * plane_offset);
                        calculatedPoints.Add((intersectionPoint - direction * offsetDistance) + r7Normal * plane_offset);
                    }
                }
            }

            var forwardPoints = calculatedPoints.OrderBy(p => Vector3.Distance(offsetStartPoint, p)).ToList();
            var backwardPoints = calculatedPoints.OrderBy(p => Vector3.Distance(offsetEndPoint, p)).ToList();
            
            Debug.Log($"[Path Segment {i}-Forward] Weaving Angle: +{weavingAngle:F2}");
            // masterPath.Add((start_safety, defaultRotation, 0));
            _LinePoint point = new _LinePoint(start_safety, defaultRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(0);

            // masterPath.Add((offsetStartPoint, defaultRotation, weavingAngle));
            point = new _LinePoint(offsetStartPoint, defaultRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(weavingAngle);
            for (int k = 0; k < forwardPoints.Count; k++)
            {
                // masterPath.Add((forwardPoints[k], (k % 2 == 0) ? defaultRotation : reversedRotation, weavingAngle));
                Quaternion _quaternion = (k % 2 == 0) ? defaultRotation : reversedRotation;
                point = new _LinePoint(forwardPoints[k], _quaternion.eulerAngles);
                _pointList.Add(point);
                masterPath.Add(weavingAngle);
            }
            // masterPath.Add((offsetEndPoint, defaultRotation, weavingAngle));
            point = new _LinePoint(offsetEndPoint, defaultRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(weavingAngle);

            // masterPath.Add((end_safety, defaultRotation, 0));
            point = new _LinePoint(end_safety, defaultRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(0);

            Debug.Log($"[Path Segment {i}-Backward] Weaving Angle: -{weavingAngle:F2}");
            float returnWeavingAngle = -weavingAngle;
            // masterPath.Add((offsetEndPoint, reversedRotation, returnWeavingAngle));
            point = new _LinePoint(offsetEndPoint, reversedRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(returnWeavingAngle);
            for (int k = 0; k < backwardPoints.Count; k++)
            {
                // masterPath.Add((backwardPoints[k], (k % 2 == 0) ? reversedRotation : defaultRotation, returnWeavingAngle));
                Quaternion _quaternion = (k % 2 == 0) ? reversedRotation : defaultRotation;
                point = new _LinePoint(backwardPoints[k], _quaternion.eulerAngles);
                _pointList.Add(point);
                masterPath.Add(returnWeavingAngle);
            }
            // masterPath.Add((offsetStartPoint, reversedRotation, returnWeavingAngle));
            point = new _LinePoint(offsetStartPoint, reversedRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(returnWeavingAngle);
            // masterPath.Add((start_safety, reversedRotation, 0));
            point = new _LinePoint(start_safety, reversedRotation.eulerAngles);
            _pointList.Add(point);
            masterPath.Add(0);
        }


        ////// @@@@@@@@@@@@@@@@@
        BA_PathLineManager.Instance.LineRendererDic["StiffEdge01"].PointList = _pointList;
        ////// @@@@@@@@@@@@@@@@@


        pointList = _pointList;
        weavingList = masterPath;

        return;
    }

    #endregion

    #region --- 로봇 모션 실행 로직 ---

    // private IEnumerator ExecuteMotionCoroutine(List<(Vector3 position, Quaternion rotation, float weavingAngle)> path)
    private IEnumerator ExecuteMotionCoroutine(List<_LinePoint> pointList, List<float> weavingList)
    {
        isExecuting = true;

        if (_bioIK == null || targetObj == null || TCP == null || jointController == null || robotStartPose == null)
        {
            Debug.LogError("로봇 제어에 필요한 핵심 컴포넌트가 모두 할당되지 않았습니다. 모션을 중단합니다.");
            isExecuting = false;
            yield break;
        }

        Debug.Log("--- 로봇 초기화 시작 ---");
        jointController.EnableOnlyA1toA6();
        _bioIK.autoIK = true;
        yield return StartCoroutine(robotStartPose.PosingSequenceCoroutine("ready_top2", Vector3.down));
        _bioIK.autoIK = false;

        Debug.Log("Step 2: Pan/Tilt 각도 직접 설정");
        RotateUpboxDirectly(90f);
        RotatedownDirectly(60f);

        Debug.Log("Step 3: 갠트리를 경로 시작점으로 이동");
        jointController.EnableOnlyGauntry();
        yield return new WaitForSeconds(0.5f);

        targetObj.transform.position = pointList[0].position;
        targetObj.transform.rotation = Quaternion.Euler(pointList[0].eulerAngles);
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Step 4: TCP 정렬");
        jointController.EnableOnlyA1toA6();
        yield return StartCoroutine(WaitUntilRotationAligned(5.0f, 3.0f));

        Debug.Log("--- 경로 추종 시작 ---");
        jointController.EnableGantryAndA5Orientation();
        // spd = 44.0f;
        // acc = 22.0f;
        for (int i = 0; i < pointList.Count; i++)
        {
            // var (targetPosition, targetRotation, targetWeavingAngle) = path[i];
            targetObj.transform.position = pointList[i].position;
            targetObj.transform.rotation = Quaternion.Euler(pointList[i].eulerAngles);

            // ★★★ weaving2 각도 적용 로직 추가 ★★★
            if (weaving2 != null)
            {
                weaving2.transform.localEulerAngles = new Vector3(
                    weaving2.transform.localEulerAngles.x,
                    weaving2.transform.localEulerAngles.y,
                    weavingList[i]
                );
            }

            float timeOut = 5f;
            float timer = 0f;
            float thres_pos = 0.01f;
            float thres_rot = 3.0f;
            while (true)
            {
                float dist_err = Vector3.Distance(TCP.position, targetObj.transform.position);
                float rot_err = Quaternion.Angle(TCP.rotation, targetObj.transform.rotation) - Mathf.Abs(weavingList[i]);
                // Debug.Log($"포인트 {i + 1} / {path.Count} 도달 중... 거리 오차: {dist_err:F4}, 회전 오차: {rot_err:F2}");
                if (dist_err < thres_pos && rot_err < thres_rot)
                {
                    // Debug.Log($"포인트 {i + 1} / {path.Count} 도달 완료.");
                    break;
                }

                if (timer > timeOut)
                {
                    Debug.LogWarning($"포인트 {i} 도달 시간 초과! 다음 지점으로 넘어갑니다.");
                    Debug.Log($"현재 위치: {TCP.position}, 목표 위치: {targetObj.transform.position}");
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (i == 0 || i == -1)
            {
                Debug.Log(spd);
                xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2.transform.localEulerAngles.z, 400, acc, onoff);
            }
            else
            {
                Debug.Log(spd);

                xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2.transform.localEulerAngles.z, spd, acc, onoff);
            }


        }

        Debug.Log("모든 경로 지점 실행 완료.");

        // ★★★ 모션 종료 후 weaving2 각도 초기화 ★★★
        if (weaving2 != null)
        {
            weaving2.transform.localRotation = Quaternion.identity;
            Debug.Log("Weaving2 각도가 0으로 초기화되었습니다.");
        }
        xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2.transform.localEulerAngles.z, spd, acc, onoff);
        // 기록된 모든 Pose 데이터를 하나의 큰 XML 문자열로 변환합니다.
        string finalXmlString = makeXmlInstance.ConvertPoseListToXmlString(xmlRecorder.AllPoses);

        // XML 데이터가 존재하고, XMLSender가 할당되었다면 저장을 요청합니다.
        if (!string.IsNullOrEmpty(finalXmlString))
        {
            if (xmlSender != null)
            {
                    // XMLSender는 현재 환경(에디터/PC 또는 WebGL)에 맞춰 자동으로 파일 저장 또는 서버 전송을 수행합니다.
                    // 파일 이름은 "edge_1_4_motion_1.xml", "_2.xml" 등으로 자동 생성됩니다.
                    xmlSender.SendLargeXmlInChunks(finalXmlString, "StiffnerEdge"); //, "edge_1_4_motion_1.xml");
            }
            else
            {
                Debug.LogError("[XML Save Failed] XMLSender가 StiffnerEdgeMotion 스크립트에 할당되지 않았습니다.");
            }
        }
        isExecuting = false;
        motionCoroutine = null;
    }
    
    private void RotateUpboxDirectly(float angle)
    {
        BioSegment upboxSegment = _bioIK.FindSegment("upbox");
        if (upboxSegment != null && upboxSegment.Joint != null)
        {
            upboxSegment.Joint.gameObject.transform.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void RotatedownDirectly(float angle)
    {
        BioSegment downSegment = _bioIK.FindSegment("down");
        if (downSegment != null && downSegment.Joint != null)
        {
            downSegment.Joint.gameObject.transform.localRotation = Quaternion.Euler(angle, 0, 0);
        }
    }

    private IEnumerator WaitUntilRotationAligned(float threshold, float timeout)
    {
        float timer = 0f;
        while (timer < timeout)
        {
            if (TCP != null && targetObj != null)
            {
                if (Quaternion.Angle(TCP.rotation, targetObj.transform.rotation) < threshold) yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region --- 시각화 및 유틸리티 ---

    private void VisualizePath(List<(Vector3 position, Quaternion rotation, float weavingAngle)> path)
    {
        if (path == null) return;
        
        CreateLine(path.Select(p => p.position).ToList(), finalPathColor, "Final_Motion_Path");

        Dictionary<Vector3, int> pointVisitCount = new Dictionary<Vector3, int>();
        for(int i = 0; i < path.Count; i++)
        {
            var (currentPoint, currentRotation, _) = path[i];
            int visitCount = 0;
            if (pointVisitCount.ContainsKey(currentPoint))
            {
                visitCount = pointVisitCount[currentPoint];
                pointVisitCount[currentPoint]++;
            }
            else
            {
                pointVisitCount.Add(currentPoint, 1);
            }

            Vector3 visualOffset = Vector3.Cross((path[1].position - path[0].position).normalized, Vector3.up).normalized * visualOffsetDistance * visitCount;
            
            CreatePosePrefab(currentPoint + visualOffset, currentRotation, $"Path_Pose_{i}");
            CreateTextLabel(currentPoint + visualOffset, i.ToString(), indexTextColor);
        }
    }

    private bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        intersection = Vector3.zero;
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);
        if (crossVec1and2.sqrMagnitude < 0.0001f) return false;
        if (Mathf.Abs(planarFactor) < 0.01f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        return false;
    }

    private void CreateLine(List<Vector3> points, Color color, string name)
    {
        if (points == null || points.Count < 2) return;
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(this.transform);
        visualObjects.Add(lineObj);
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = points.Count;
        lr.SetPositions(points.ToArray());
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
    }
    
    private void CreatePosePrefab(Vector3 position, Quaternion rotation, string name)
    {
        if (targetPrefab == null) return;
        GameObject poseObj = Instantiate(targetPrefab, position, rotation, this.transform);
        poseObj.name = name;
        visualObjects.Add(poseObj);
    }

    private void CreateTextLabel(Vector3 position, string text, Color color)
    {
        GameObject textObj = new GameObject($"Label_{text}");
        textObj.transform.SetParent(this.transform);
        visualObjects.Add(textObj);
        textObj.transform.position = position + Vector3.up * (pointSize * 1.5f);
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 20;
        textMesh.characterSize = 0.1f;
        textMesh.color = color;
        textMesh.anchor = TextAnchor.MiddleCenter;
    }
    
    #endregion
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class MousePathController : MonoBehaviour
{
    [Header("레이어 및 입력")]
    public LayerMask clickableLayer;        // 클릭 가능한 레이어
    private Camera mainCamera;              // 메인 카메라

    [Header("오브젝트")]
    public Transform startObj;              // 실제 이동할 로봇 오브젝트
   // public Transform test;                  // 임시 목표 위치 표시 오브젝트
    public Vector3 endObj;

    [Header("이동 설정")]
    public float speed = 2f;                //이동속도
    [Range(2f, 2.5f)]
    public float distanceThreshold = 2f;    // 목표 위치와의 거리 임계값
    private Vector3 goalPos;                // 목표 위치
    private bool isMoving = false;          // 이동 중인지 여부
    private Coroutine coroutine;            // 현재 실행 중인 코루틴 참조

    [Header("시각화")]
    private LineRenderer lineRenderer;      // 경로 시각화를 위한 LineRenderer

    [Header("충돌/경계")]
    private MeshCollider meshCol;            // 충돌체로 사용할 메쉬 콜라이더

    [Header("IK 및 제어")]
    public Go_to_One_Point_UI go_To_One_Point_UI;
    public BioIK.BioIK bioIK;
    public RobotPoseDropdown robotPoseDropdown;

    [Header("경로 기록")]
    private XmlPathRecorder xmlPathRecorder;
    public Make_XML makeXmlInstance;


    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // 기본 카메라 설정
        }

    //    xmlPathRecorder = new XmlPathRecorder(makeXmlInstance, bioIK);
        lineRenderer = GetComponent<LineRenderer>();

    }
    public void TargetMove()
    {
        if (coroutine != null) return; // 이미 경로 따라가기 중이면 중복 실행 방지
        if (meshCol == null)
        {
            meshCol = SystemController.Instance.GetCollider();
            
        }
        if (meshCol != null)
        {
            meshCol.convex = true;
        }
        StartCoroutine(CoroutineUpdatee()); // 코루틴 시작

    }
    /// <summary>
    /// 버튼클릭시 클릭한결로까지 패스 연결
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoroutineUpdatee()
    {  // 경로 찾기
        isMoving = true; // 이동 상태 설정
        
        while (isMoving) // 무한 루프
        {
            if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 시
            {
                ClickToSetPosition();
                FindAndFollowPath();
            }

            yield return null; // 지정된 시간 대기
        }

    }
    /// <summary>
    /// 경로를 찾고 따라가기
    /// </summary>
    private void FindAndFollowPath()
    {
        if (coroutine != null) return; // 이미 경로 따라가기 중이면 중복 실행 방지
        if (startObj == null)
        {
            Debug.LogError("Start Object is not assigned.");
            return;
        }
        if (goalPos == Vector3.zero) // 목표 위치가 설정되지 않은 경우
        {
            Debug.LogWarning("Goal position is not set. Click to set a position first.");
            return;
        }

        List<Vector3> movePath = FindRoot.FindPath(startObj, goalPos, meshCol, distanceThreshold, clickableLayer);

        if (movePath != null)
        {
            FindRoot.DrawPath(movePath, lineRenderer);
            coroutine = StartCoroutine(FollowPath(movePath));
        }
    }
    /// <summary>
    /// 클릭한 위치를 목표위치로 설정
    /// </summary>
    private void ClickToSetPosition()
    {
        if (coroutine != null) return; // 이미 경로 따라가기 중이면 중복 실행 방지
        if (startObj == null)
        {
            Debug.LogError("Start Object is not assigned.");
            return;
        }
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 클릭 가능한 레이어에 충돌한 경우
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
        {
            float minDistance = 2f;
            float maxDistance = 3f;
            float step = 0.1f;
            float checkRadius = 0.3f;

            Vector3 origin = hit.point;
            endObj = hit.point;
            Vector3 normal = hit.normal.normalized;
            Bounds bounds = meshCol.bounds;
            float maxY = bounds.max.y - 0.01f;
            float minY = bounds.min.y;

            Vector3 bestPos = origin + normal * maxDistance;

            bool found = false;

            for (float dist = minDistance; dist <= maxDistance; dist += step)
            {
                // 1. 기본 방향 (법선 방향)
                Vector3 candidate = origin + normal * dist;

                if ((candidate.y + checkRadius <= maxY) &&
                    (candidate.y - checkRadius >= minY) &&
                    !Physics.CheckSphere(candidate, checkRadius, clickableLayer))
                {
                    bestPos = candidate;
                    found = true;
                    break;
                }

                // 2. 대안: XZ 평면 방향들로 우회
                Vector3[] altDirs = {
                    Vector3.right, Vector3.left,
                    Vector3.forward, Vector3.back,
                    (Vector3.right + Vector3.forward).normalized,
                    (Vector3.right + Vector3.back).normalized,
                    (Vector3.left + Vector3.forward).normalized,
                    (Vector3.left + Vector3.back).normalized
                    };

                foreach (var dir in altDirs)
                {
                    Vector3 alt = origin + dir * dist;
                    alt.y = Mathf.Clamp(alt.y, minY + checkRadius, maxY - checkRadius);

                    if (!Physics.CheckSphere(alt, checkRadius, clickableLayer))
                    {
                        bestPos = alt;
                        found = true;
                        break;
                    }
                }

                if (found) break;
            }

            //test.position = bestPos; //테스트용   
            goalPos = bestPos;
        }
    }
   
    /// <summary>
    /// 경로 따라 이동
    /// </summary>
    /// <param name="path"><이동 경로/param>
    /// <returns></returns>
    private IEnumerator FollowPath(List<Vector3> path)
    {
        yield return new WaitForSeconds(1f); // 시작 전 대기

        int currentIndex = 0;
        robotPoseDropdown.SetPoseByName("ready_front");

        if (path == null || path.Count == 0)
            yield break;
        //string savePose = ""; // 현재 포즈 저장용 변수
        while (currentIndex < path.Count)
        {
            Vector3 targetPos = path[currentIndex]; // 경로에서 목표 지점 가져오기
            ///면마다 포즈 넣을위치
            ///
            float step = speed * Time.deltaTime;

            // 이동
            startObj.transform.position = Vector3.MoveTowards(startObj.transform.position, targetPos, step);

            // 목표 지점에 도달하면 다음 인덱스로
            if (Vector3.Distance(startObj.transform.position, targetPos) < 0.05f)
            {

           /*     xmlPathRecorder.RecordCurrentPose();
                xmlPathRecorder.FinalizeRecording();
*/
                currentIndex++;
                if (currentIndex >= path.Count)
                {
                    path.Clear(); // 경로 초기화
                    SetJointRotation();
                    ResetState();
                    yield break;
                }
            }

            yield return null; // 다음 프레임까지 대기
        }
    }
    /// <summary>
    /// 상태 초기화
    /// </summary>
    void ResetState()
    {
        coroutine = null;
        isMoving = false;

        if (meshCol != null)
            meshCol.convex = false;
    }
    /// <summary>
    /// 선택방향으로 도착후 회전
    /// </summary>
    private void SetJointRotation()
    {
        Vector3 direction = endObj - startObj.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        // 목표 방향을 기준으로 회전 쿼터니언 계산 (forward를 target으로 돌림)
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);

        // 로봇은 forward가 아니라 right가 앞 → Y축 기준으로 -90도 회전 보정 필요
        Quaternion correctedRotation = lookRotation * Quaternion.Euler(0, -90f, 0);

        // Y축 회전만 추출
        float angleY = correctedRotation.eulerAngles.y;

        // -180 ~ 180으로 보정
        if (angleY > 180f) angleY -= 360f;

        // 적용
        go_To_One_Point_UI.RotateUpboxDirectly(angleY);
        go_To_One_Point_UI._bioIK_UI.autoIK = true;
        go_To_One_Point_UI.jointController_UI.EnableOnlyGauntry();
        go_To_One_Point_UI.bioIK_UI.Refresh();
    }
    /*     private Dictionary<Vector3, string> dirToPose = new Dictionary<Vector3, string>
        {
            { Vector3.forward, "ready_front" },
            { Vector3.back,    "ready_front" },
            { Vector3.right,   "ready_front" },
            { Vector3.left,    "ready_front" },
            { Vector3.up,      "ready_front" },
            { Vector3.down,    "home" }
        };
     *   // 포즈설정
             /*   Vector3 origin = startObj.position + Vector3.up * 0.1f; // 약간 위로 띄우면 충돌 놓칠 확률 감소
                //string pose = "";
                if (!rotated)
                {
                    pose = GetClosestSurfacePose(origin, targetObj.position);
                    if (!pose.Equals(""))
                    {
                        rotated = true;
                        Invoke("ResetRotationFlag", 1);
                    }
                }

                if (!savePose.Equals(pose))
                {
                    if (!pose.Equals(""))
                    {
                        robotPoseDropdown.SetPoseByName(pose);
                        savePose = pose; // 현재 포즈 저장

                        yield return new WaitForSeconds(2f); // 포즈 변경 대기 시간
                    }
                }
                //포즈설정 종료
       private void ResetRotationFlag()
        {
            rotated = false;
        }
        /// <summary>
        /// 근처 벽면 정보 가져옴
        /// </summary>
        /// <param name="origin">시작 위치</param>
        /// <param name="targetPos">클릭시 지정되는 오브젝트</param>
        /// <returns></returns>
        private string GetClosestSurfacePose(Vector3 origin, Vector3 targetPos)
        {
            Vector3 bestNormal = Vector3.zero;
            float minDistance = Mathf.Infinity;
            string pose = "";

            for (int i = 0; i < rayCount; i++)
            {
                // 여러 방향으로 퍼지게 샘플링 (여기선 수평 + 상하 방향 조합 예시)
                float angle = i * Mathf.PI * 2f / rayCount;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized; // 살짝 아래도 포함

                if (Physics.Raycast(origin, dir, out RaycastHit hit, 5f, clickableLayer))
                {
                    if (hit.distance < minDistance)
                    {
                        minDistance = hit.distance;
                        bestNormal = hit.normal;

                    }
                }
            }

            if (minDistance < Mathf.Infinity)
            {
                pose = GetPoseFromDirection(bestNormal);

                Vector3 flatNormal = new Vector3(bestNormal.x, 0f, bestNormal.z).normalized;

                if (flatNormal.sqrMagnitude > 0.001f)
                {
                    float targetYAngle = Mathf.Atan2(flatNormal.z, flatNormal.x) * Mathf.Rad2Deg;

                    // 좌우 방향(dot)이 클 때만 180도 회전
                    float sideDot = Mathf.Abs(Vector3.Dot(flatNormal, Vector3.right)); // 1이면 완전 좌우 방향

                    if (sideDot > 0.7f) // 임계값 0.7, 필요에 따라 조절 가능
                    {
                        targetYAngle += 180f;
                    }

                    targetYAngle = (targetYAngle + 360) % 360;

                    ///
                    go_To_One_Point_UI.RotateUpboxDirectly(targetYAngle);
                    go_To_One_Point_UI.bioIK_UI.autoIK = true; // IK 켜기
                    go_To_One_Point_UI.jointController_UI.EnableOnlyGauntry();
                    go_To_One_Point_UI.bioIK_UI.Refresh();
                    ///
                    Vector3 currentEuler = startObj.rotation.eulerAngles;
                    startObj.rotation = Quaternion.Euler(currentEuler.x, targetYAngle, currentEuler.z);
                }
            }

            return pose;
        }
        /// <summary>
        /// 면에따라 해동 지정가능
        /// </summary>
        /// <param name="dir"> 방향</param>
        /// <returns></returns>
        private string GetPoseFromDirection(Vector3 dir)
        {
            Vector3 bestMatch = Vector3.zero;
            float maxDot = -Mathf.Infinity;

            foreach (var kvp in dirToPose)
            {
                float dot = Vector3.Dot(dir.normalized, kvp.Key);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestMatch = kvp.Key;
                }
            }

            return dirToPose[bestMatch];
        }*/
}

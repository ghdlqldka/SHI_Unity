using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(LineRenderer))]
public class MousePathController : MonoBehaviour
{
    [Header("���̾� �� �Է�")]
    public LayerMask clickableLayer;        // Ŭ�� ������ ���̾�
    private Camera mainCamera;              // ���� ī�޶�

    [Header("������Ʈ")]
    public Transform startObj;              // ���� �̵��� �κ� ������Ʈ
   // public Transform test;                  // �ӽ� ��ǥ ��ġ ǥ�� ������Ʈ
    public Vector3 endObj;

    [Header("�̵� ����")]
    public float speed = 2f;                //�̵��ӵ�
    [Range(2f, 2.5f)]
    public float distanceThreshold = 2f;    // ��ǥ ��ġ���� �Ÿ� �Ӱ谪
    private Vector3 goalPos;                // ��ǥ ��ġ
    private bool isMoving = false;          // �̵� ������ ����
    private Coroutine coroutine;            // ���� ���� ���� �ڷ�ƾ ����

    [Header("�ð�ȭ")]
    private LineRenderer lineRenderer;      // ��� �ð�ȭ�� ���� LineRenderer

    [Header("�浹/���")]
    private MeshCollider meshCol;            // �浹ü�� ����� �޽� �ݶ��̴�

    [Header("IK �� ����")]
    public Go_to_One_Point_UI go_To_One_Point_UI;
    public BioIK.BioIK bioIK;
    public RobotPoseDropdown robotPoseDropdown;

    [Header("��� ���")]
    private XmlPathRecorder xmlPathRecorder;
    public Make_XML makeXmlInstance;


    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // �⺻ ī�޶� ����
        }

    //    xmlPathRecorder = new XmlPathRecorder(makeXmlInstance, bioIK);
        lineRenderer = GetComponent<LineRenderer>();

    }
    public void TargetMove()
    {
        if (coroutine != null) return; // �̹� ��� ���󰡱� ���̸� �ߺ� ���� ����
        if (meshCol == null)
        {
            meshCol = SystemController.Instance.GetCollider();
            
        }
        if (meshCol != null)
        {
            meshCol.convex = true;
        }
        StartCoroutine(CoroutineUpdatee()); // �ڷ�ƾ ����

    }
    /// <summary>
    /// ��ưŬ���� Ŭ���Ѱ�α��� �н� ����
    /// </summary>
    /// <returns></returns>
    public IEnumerator CoroutineUpdatee()
    {  // ��� ã��
        isMoving = true; // �̵� ���� ����
        
        while (isMoving) // ���� ����
        {
            if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư Ŭ�� ��
            {
                ClickToSetPosition();
                FindAndFollowPath();
            }

            yield return null; // ������ �ð� ���
        }

    }
    /// <summary>
    /// ��θ� ã�� ���󰡱�
    /// </summary>
    private void FindAndFollowPath()
    {
        if (coroutine != null) return; // �̹� ��� ���󰡱� ���̸� �ߺ� ���� ����
        if (startObj == null)
        {
            Debug.LogError("Start Object is not assigned.");
            return;
        }
        if (goalPos == Vector3.zero) // ��ǥ ��ġ�� �������� ���� ���
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
    /// Ŭ���� ��ġ�� ��ǥ��ġ�� ����
    /// </summary>
    private void ClickToSetPosition()
    {
        if (coroutine != null) return; // �̹� ��� ���󰡱� ���̸� �ߺ� ���� ����
        if (startObj == null)
        {
            Debug.LogError("Start Object is not assigned.");
            return;
        }
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Ŭ�� ������ ���̾ �浹�� ���
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
                // 1. �⺻ ���� (���� ����)
                Vector3 candidate = origin + normal * dist;

                if ((candidate.y + checkRadius <= maxY) &&
                    (candidate.y - checkRadius >= minY) &&
                    !Physics.CheckSphere(candidate, checkRadius, clickableLayer))
                {
                    bestPos = candidate;
                    found = true;
                    break;
                }

                // 2. ���: XZ ��� ������ ��ȸ
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

            //test.position = bestPos; //�׽�Ʈ��   
            goalPos = bestPos;
        }
    }
   
    /// <summary>
    /// ��� ���� �̵�
    /// </summary>
    /// <param name="path"><�̵� ���/param>
    /// <returns></returns>
    private IEnumerator FollowPath(List<Vector3> path)
    {
        yield return new WaitForSeconds(1f); // ���� �� ���

        int currentIndex = 0;
        robotPoseDropdown.SetPoseByName("ready_front");

        if (path == null || path.Count == 0)
            yield break;
        //string savePose = ""; // ���� ���� ����� ����
        while (currentIndex < path.Count)
        {
            Vector3 targetPos = path[currentIndex]; // ��ο��� ��ǥ ���� ��������
            ///�鸶�� ���� ������ġ
            ///
            float step = speed * Time.deltaTime;

            // �̵�
            startObj.transform.position = Vector3.MoveTowards(startObj.transform.position, targetPos, step);

            // ��ǥ ������ �����ϸ� ���� �ε�����
            if (Vector3.Distance(startObj.transform.position, targetPos) < 0.05f)
            {

           /*     xmlPathRecorder.RecordCurrentPose();
                xmlPathRecorder.FinalizeRecording();
*/
                currentIndex++;
                if (currentIndex >= path.Count)
                {
                    path.Clear(); // ��� �ʱ�ȭ
                    SetJointRotation();
                    ResetState();
                    yield break;
                }
            }

            yield return null; // ���� �����ӱ��� ���
        }
    }
    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    void ResetState()
    {
        coroutine = null;
        isMoving = false;

        if (meshCol != null)
            meshCol.convex = false;
    }
    /// <summary>
    /// ���ù������� ������ ȸ��
    /// </summary>
    private void SetJointRotation()
    {
        Vector3 direction = endObj - startObj.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        // ��ǥ ������ �������� ȸ�� ���ʹϾ� ��� (forward�� target���� ����)
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);

        // �κ��� forward�� �ƴ϶� right�� �� �� Y�� �������� -90�� ȸ�� ���� �ʿ�
        Quaternion correctedRotation = lookRotation * Quaternion.Euler(0, -90f, 0);

        // Y�� ȸ���� ����
        float angleY = correctedRotation.eulerAngles.y;

        // -180 ~ 180���� ����
        if (angleY > 180f) angleY -= 360f;

        // ����
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
     *   // �����
             /*   Vector3 origin = startObj.position + Vector3.up * 0.1f; // �ణ ���� ���� �浹 ��ĥ Ȯ�� ����
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
                        savePose = pose; // ���� ���� ����

                        yield return new WaitForSeconds(2f); // ���� ���� ��� �ð�
                    }
                }
                //����� ����
       private void ResetRotationFlag()
        {
            rotated = false;
        }
        /// <summary>
        /// ��ó ���� ���� ������
        /// </summary>
        /// <param name="origin">���� ��ġ</param>
        /// <param name="targetPos">Ŭ���� �����Ǵ� ������Ʈ</param>
        /// <returns></returns>
        private string GetClosestSurfacePose(Vector3 origin, Vector3 targetPos)
        {
            Vector3 bestNormal = Vector3.zero;
            float minDistance = Mathf.Infinity;
            string pose = "";

            for (int i = 0; i < rayCount; i++)
            {
                // ���� �������� ������ ���ø� (���⼱ ���� + ���� ���� ���� ����)
                float angle = i * Mathf.PI * 2f / rayCount;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized; // ��¦ �Ʒ��� ����

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

                    // �¿� ����(dot)�� Ŭ ���� 180�� ȸ��
                    float sideDot = Mathf.Abs(Vector3.Dot(flatNormal, Vector3.right)); // 1�̸� ���� �¿� ����

                    if (sideDot > 0.7f) // �Ӱ谪 0.7, �ʿ信 ���� ���� ����
                    {
                        targetYAngle += 180f;
                    }

                    targetYAngle = (targetYAngle + 360) % 360;

                    ///
                    go_To_One_Point_UI.RotateUpboxDirectly(targetYAngle);
                    go_To_One_Point_UI.bioIK_UI.autoIK = true; // IK �ѱ�
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
        /// �鿡���� �ص� ��������
        /// </summary>
        /// <param name="dir"> ����</param>
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

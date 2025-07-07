using System.Collections;
using UnityEngine;
using BioIK;
using TMPro;
using _SHI_BA;
public class Go_to_One_Point_UI : MonoBehaviour
{
    public BioIK.BioIK bioIK_UI;
    public BA_BioIK _bioIK_UI
    {
        get
        {
            return bioIK_UI as BA_BioIK;
        }
    }

    public Transform eef_UI;
    public GameObject targetObj_UI;
    public Vector3 targetPosition_UI;
    public Quaternion targetRotation_UI = Quaternion.identity;
    public Gauntry_or_6Joint_or_all jointController_UI;

    [Header("upbox 조인트 이름")]
    public string upboxJointName_UI = "upbox";

    [Header("UI 입력 필드")]
    public TMP_InputField inputX_UI;
    public TMP_InputField inputY_UI;
    public TMP_InputField inputZ_UI;
    public TMP_InputField inputPan_UI; // upbox 각도 입력용

    // 원래 위치/회전 저장용
    private Vector3 originalTargetPosition_UI;
    private Quaternion originalTargetRotation_UI;
    private bool savedOriginal_UI = false;

    public float defaultX = -12.27424f;
    public float defaultY = 7.774f;
    public float defaultZ = 9.8048f;
    public float defaultPan = 90f;
    public float defaultRotY = 180f;
    

    void Start()
    {
        if (inputX_UI != null) inputX_UI.text = defaultX.ToString();
        if (inputY_UI != null) inputY_UI.text = defaultY.ToString();
        if (inputZ_UI != null) inputZ_UI.text = defaultZ.ToString();
        if (inputPan_UI != null) inputPan_UI.text = defaultPan.ToString();
    }
    public void RotateUpboxDirectly(float angle)
    {
        if (_bioIK_UI != null)
            _bioIK_UI.autoIK = false; // IK 끄기

        GameObject upboxObj = GetUpboxGameObject_UI();
        if (upboxObj != null)
        {
            upboxObj.transform.localRotation = Quaternion.Euler(0, 0, angle);
            jointController_UI.EnableOnlyUpbox(angle);
            Debug.Log($"[Plane_Motion_Planning] IK OFF, upbox를 {angle}도로 직접 회전");
        }
        else
        {
            Debug.LogWarning("[Plane_Motion_Planning] upbox GameObject를 찾지 못했습니다.");
        }
    }
    public GameObject GetUpboxGameObject_UI()
    {
        if (bioIK_UI == null || bioIK_UI.Segments == null)
            return null;

        foreach (var segment in bioIK_UI.Segments)
        {
            if (segment.Joint != null && segment.Joint.gameObject.name == "upbox")
            {
                return segment.Joint.gameObject;
            }
        }
        return null;
    }



    

    [ContextMenu("Go_to_One_Point_UI (All)")]
    public void GoToOnePointAll_UI()
    {
        // 1. Gantry만 Enable
        if (jointController_UI != null)
        {
            jointController_UI.EnableOnlyGauntry();
            // jointController_UI.EnableAllJoints();
            Debug.Log("[Go_to_One_Point_UI] 모든 Joint Enable 적용 완료");
        }
        else
        {
            Debug.LogWarning("[Go_to_One_Point_UI] jointController_UI가 할당되지 않았습니다.");
        }

        // 2. upbox 조인트 각도 고정 (inputPan_UI 값 사용)
        if (!float.TryParse(inputPan_UI.text, out defaultPan))
        {
            Debug.LogWarning("[Go_to_One_Point_UI] inputPan_UI 값이 올바르지 않습니다. 0도로 설정합니다.");
            defaultPan = 0f;
        }
        RotateUpboxDirectly(defaultPan);

        // 디버깅: upbox의 실제 localRotation 값 출력
        // GameObject upboxObj = GetUpboxGameObject_UI();
        // if (upboxObj != null)
        // {
        //     Debug.Log($"[MotionPlanning] upboxObj.transform.localRotation: {upboxObj.transform.localRotation.eulerAngles}");
        // }
        // else
        // {
        //     Debug.LogWarning("[MotionPlanning] upbox GameObject를 찾지 못했습니다.");
        // }

        // 3. Target 오브젝트 준비
        if (targetObj_UI == null)
        {
            targetObj_UI = GameObject.Find("Target_UI");
            if (targetObj_UI == null)
            {
                targetObj_UI = new GameObject("Target_UI");
            }
        }

        // 4. 원래 위치/회전 저장 (한 번만)
        if (!savedOriginal_UI)
        {
            originalTargetPosition_UI = targetObj_UI.transform.position;
            originalTargetRotation_UI = targetObj_UI.transform.rotation;
            savedOriginal_UI = true;
        }

        // 5. UI 입력값 적용
        float x = 0, y = 0, z = 0;
        if (inputX_UI != null) float.TryParse(inputX_UI.text, out x);
        if (inputY_UI != null) float.TryParse(inputY_UI.text, out y);
        if (inputZ_UI != null) float.TryParse(inputZ_UI.text, out z);

        // 원하는 위치와 회전값을 바로 Target 오브젝트에 적용
        Vector3 targetPosition = new Vector3(x, y, z);
        Quaternion targetRotation = Quaternion.Euler(0, defaultRotY, 0);

        // 6. Target을 (-1,2,2.8)로 먼저 이동
        Vector3 prePosition_UI = new Vector3(-5f, 5f, 5f);
        targetObj_UI.transform.position = prePosition_UI;
        targetObj_UI.transform.rotation = targetRotation;
        Debug.Log($"[Go_to_One_Point_UI] Target을 먼저 {prePosition_UI}로 이동");

        // 7. 입력한 지점으로 이동
        targetObj_UI.transform.position = targetPosition;
        targetObj_UI.transform.rotation = targetRotation;
        // bioIK_UI.autoIK = true; // IK 켜기
        // jointController_UI.EnableOnlyA1toA6();
        // bioIK_UI.Refresh();
        // bioIK_UI.autoIK = true; // IK 켜기
        // jointController_UI.EnableOnlyGauntry();
        // bioIK_UI.Refresh();

        Debug.Log($"[Go_to_One_Point_UI] Target 위치: {targetObj_UI.transform.position}, 회전: {targetObj_UI.transform.rotation.eulerAngles}");
    }

    /// <summary>
    /// inputPan_UI의 값을 upbox 조인트 각도로 적용하고 리미트도 동일하게 고정
    /// </summary>
    public void SetUpboxJointToAngleFromInput()
    {
        if (bioIK_UI == null)
        {
            Debug.LogError("BioIK_UI reference not set!");
            return;
        }

        float upboxAngle = 0f;
        if (inputPan_UI != null)
        {
            if (!float.TryParse(inputPan_UI.text, out upboxAngle))
            {
                Debug.LogWarning("[SetUpboxJointToAngleFromInput] 입력값이 올바르지 않습니다. 0도로 설정합니다.");
                upboxAngle = 0f;
            }
        }

        foreach (var seg in bioIK_UI.Segments)
        {
            if (seg.Joint == null) continue;
            if (seg.Joint.gameObject.name == upboxJointName_UI)
            {
                var motions = new BioJoint.Motion[] { seg.Joint.X, seg.Joint.Y, seg.Joint.Z };
                foreach (var motion in motions)
                {
                    if (motion == null || !motion.Enabled) continue;
                    motion.SetLowerLimit(upboxAngle);
                    motion.SetUpperLimit(upboxAngle);
                    motion.SetTargetValue(upboxAngle);

                    Debug.Log(
                        $"[SetUpboxJointToAngleFromInput] Axis: {motion.Axis}, Angle: {upboxAngle}, LowerLimit: {motion.LowerLimit}, UpperLimit: {motion.UpperLimit}, TargetValue: {motion.TargetValue}, Enabled: {motion.Enabled}"
                    );
                }
                break;
            }
        }
        jointController_UI.EnableOnlyGauntry();
        bioIK_UI.Refresh();
        Debug.Log($"[SetUpboxJointToAngleFromInput] upbox 조인트를 {upboxAngle}도로 고정");
    }

    // 에디터/플레이모드 전환 시 원래 위치로 복원
    void OnDisable()
    {
        RestoreTargetTransform_UI();
    }
    void OnApplicationQuit()
    {
        RestoreTargetTransform_UI();
    }
    private void RestoreTargetTransform_UI()
    {
        if (targetObj_UI != null && savedOriginal_UI)
        {
            targetObj_UI.transform.position = originalTargetPosition_UI;
            targetObj_UI.transform.rotation = originalTargetRotation_UI;
            Debug.Log("[Go_to_One_Point_UI] Target 위치/회전 원래대로 복원");
        }
    }
}

// using System.Collections;
// using UnityEngine;
// using BioIK;
// using TMPro;

// public class Go_to_One_Point_UI : MonoBehaviour
// {
//     public BioIK.BioIK bioIK_UI;
//     public Transform eef_UI;
//     public GameObject targetObj_UI;
//     public Vector3 targetPosition_UI;
//     public Quaternion targetRotation_UI = Quaternion.identity;
//     public IkKuka6JointController jointController_UI;

//     [Header("upbox 조인트 이름")]
//     public string upboxJointName_UI = "upbox";

//     [Header("UI 입력 필드")]
//     public TMP_InputField inputX_UI;
//     public TMP_InputField inputY_UI;
//     public TMP_InputField inputZ_UI;
//     public TMP_InputField inputPan_UI; // upbox 각도 입력용

//     // 원래 위치/회전 저장용
//     private Vector3 originalTargetPosition_UI;
//     private Quaternion originalTargetRotation_UI;
//     private bool savedOriginal_UI = false;

//     public float defaultX = -12.27424f;
//     public float defaultY = 7.774f;
//     public float defaultZ = 9.8048f;
//     public float defaultPan = 90f;
//     public float defaultRotY = 180f;
    

//     void Start()
//     {
//         if (inputX_UI != null) inputX_UI.text = defaultX.ToString();
//         if (inputY_UI != null) inputY_UI.text = defaultY.ToString();
//         if (inputZ_UI != null) inputZ_UI.text = defaultZ.ToString();
//         if (inputPan_UI != null) inputPan_UI.text = defaultPan.ToString();
//     }
    
//     public void RotateUpboxDirectly(float angle)
//     {
//         // 이 함수는 외부에서 직접 호출될 수 있으므로 bioIK_UI null 체크를 유지합니다.
//         if (bioIK_UI == null) return;

//         // IK 자동 업데이트를 잠시 비활성화하여 직접 제어합니다.
//         // 이 함수가 끝난 후, 상위 제어 로직(예: 코루틴)에서 autoIK를 다시 활성화해야 합니다.
//         bioIK_UI.autoIK = false;

//         GameObject upboxObj = GetUpboxGameObject_UI();
//         if (upboxObj != null)
//         {
//             upboxObj.transform.localRotation = Quaternion.Euler(0, 0, angle);
//             if(jointController_UI != null)
//             {
//                 jointController_UI.EnableOnlyUpbox(angle);
//             }
//             Debug.Log($"[Go_to_One_Point_UI] upbox를 {angle}도로 직접 회전 (IK 비활성화 상태)");
//         }
//         else
//         {
//             Debug.LogWarning("[Go_to_One_Point_UI] upbox GameObject를 찾지 못했습니다.");
//         }
//     }

//     public GameObject GetUpboxGameObject_UI()
//     {
//         if (bioIK_UI == null || bioIK_UI.Segments == null)
//             return null;

//         foreach (var segment in bioIK_UI.Segments)
//         {
//             if (segment.Joint != null && segment.Joint.gameObject.name == upboxJointName_UI)
//             {
//                 return segment.Joint.gameObject;
//             }
//         }
//         return null;
//     }

//     [ContextMenu("Go_to_One_Point_UI (All)")]
//     public void GoToOnePointAll_UI()
//     {
//         // 1. UI에서 값 읽기
//         if (!float.TryParse(inputX_UI.text, out float x)) x = defaultX;
//         if (!float.TryParse(inputY_UI.text, out float y)) y = defaultY;
//         if (!float.TryParse(inputZ_UI.text, out float z)) z = defaultZ;
//         if (!float.TryParse(inputPan_UI.text, out float pan)) pan = defaultPan;

//         // 2. upbox 각도 직접 회전
//         // 이 함수는 IK를 잠시 끄고 값을 설정합니다.
//         RotateUpboxDirectly(pan); 

//         // 3. Target 오브젝트 위치/회전 설정
//         if (targetObj_UI == null)
//         {
//             targetObj_UI = new GameObject("Target_UI");
//         }
        
//         // Target의 원래 위치/회전 저장 (한 번만)
//         if (!savedOriginal_UI)
//         {
//             originalTargetPosition_UI = targetObj_UI.transform.position;
//             originalTargetRotation_UI = targetObj_UI.transform.rotation;
//             savedOriginal_UI = true;
//         }

//         targetObj_UI.transform.position = new Vector3(x, y, z);
//         targetObj_UI.transform.rotation = Quaternion.Euler(0, defaultRotY, 0);

//         // 4. IK 활성화 (가장 중요)
//         // 모든 설정이 끝난 후, IK가 새로운 목표를 향해 계산을 시작하도록 합니다.
//         bioIK_UI.autoIK = true; 

//         Debug.Log($"[Go_to_One_Point_UI] Target 설정 완료. Pos: {targetObj_UI.transform.position}, Rot: {targetObj_UI.transform.rotation.eulerAngles}");
//     }


//     /// <summary>
//     /// inputPan_UI의 값을 upbox 조인트 각도로 적용하고 리미트도 동일하게 고정
//     /// </summary>
//     public void SetUpboxJointToAngleFromInput()
//     {
//         if (bioIK_UI == null)
//         {
//             Debug.LogError("BioIK_UI reference not set!");
//             return;
//         }

//         float upboxAngle = 0f;
//         if (inputPan_UI != null)
//         {
//             if (!float.TryParse(inputPan_UI.text, out upboxAngle))
//             {
//                 Debug.LogWarning("[SetUpboxJointToAngleFromInput] 입력값이 올바르지 않습니다. 0도로 설정합니다.");
//                 upboxAngle = 0f;
//             }
//         }

//         foreach (var seg in bioIK_UI.Segments)
//         {
//             if (seg.Joint == null) continue;
//             if (seg.Joint.gameObject.name == upboxJointName_UI)
//             {
//                 var motions = new BioJoint.Motion[] { seg.Joint.X, seg.Joint.Y, seg.Joint.Z };
//                 foreach (var motion in motions)
//                 {
//                     if (motion == null || !motion.Enabled) continue;
//                     motion.SetLowerLimit(upboxAngle);
//                     motion.SetUpperLimit(upboxAngle);
//                     motion.SetTargetValue(upboxAngle);

//                     Debug.Log(
//                         $"[SetUpboxJointToAngleFromInput] Axis: {motion.Axis}, Angle: {upboxAngle}, LowerLimit: {motion.LowerLimit}, UpperLimit: {motion.UpperLimit}, TargetValue: {motion.TargetValue}, Enabled: {motion.Enabled}"
//                     );
//                 }
//                 break;
//             }
//         }
        
//         if (jointController_UI != null)
//         {
//             jointController_UI.EnableOnlyGauntry();
//         }
//         bioIK_UI.Refresh();
//         Debug.Log($"[SetUpboxJointToAngleFromInput] upbox 조인트를 {upboxAngle}도로 고정");
//     }

//     // 에디터/플레이모드 전환 시 원래 위치로 복원
//     void OnDisable()
//     {
//         RestoreTargetTransform_UI();
//     }
//     void OnApplicationQuit()
//     {
//         RestoreTargetTransform_UI();
//     }
//     private void RestoreTargetTransform_UI()
//     {
//         if (targetObj_UI != null && savedOriginal_UI)
//         {
//             targetObj_UI.transform.position = originalTargetPosition_UI;
//             targetObj_UI.transform.rotation = originalTargetRotation_UI;
//             Debug.Log("[Go_to_One_Point_UI] Target 위치/회전 원래대로 복원");
//         }
//     }
// }
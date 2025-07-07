// using UnityEngine;
// using BioIK;

// public enum UpboxAngle
// {
//     Deg0 = 0,
//     Deg90 = 90,
//     Deg180 = 180,
//     DegMinus90 = -90
// }

// public class Go_to_One_Point : MonoBehaviour
// {
//     public BioIK.BioIK bioIK;
//     public Transform eef;
//     public GameObject targetObj;
//     public Vector3 targetPosition;
//     public Quaternion targetRotation = Quaternion.identity;
//     public IkKuka6JointController jointController; // 인스펙터에서 할당
    
//     [Header("upbox 조인트 이름")]
//     public string upboxJointName = "upbox";

//     [Header("upbox 각도 선택 (0, 90, -90, 180)")]
//     public UpboxAngle upboxAngle = UpboxAngle.Deg0;

//     // 원래 위치/회전 저장용
//     private Vector3 originalTargetPosition;
//     private Quaternion originalTargetRotation;
//     private bool savedOriginal = false;

//     [ContextMenu("Go_to_One_Point (All)")]
//     public void GoToOnePointAll()
//     {
//         // 1. Gantry만 Enable
//         if (jointController != null)
//         {
//             // jointController.EnableOnlyGauntry();
//             // Debug.Log("[Go_to_One_Point] Gantry Joint만 Enable 적용 완료");
//             jointController.EnableAllJoints();
//             Debug.Log("[Go_to_One_Point] 모든 Joint Enable 적용 완료");
//         }
//         else
//         {
//             Debug.LogWarning("[Go_to_One_Point] jointController가 할당되지 않았습니다.");
//         }

//         // 2. upbox 조인트 각도 고정
//         SetUpboxJointToAngle((float)upboxAngle);

//         // 3. Target 오브젝트 준비
//         if (targetObj == null)
//         {
//             targetObj = GameObject.Find("Target");
//             if (targetObj == null)
//             {
//                 targetObj = new GameObject("Target");
//             }
//         }

//         // 4. 원래 위치/회전 저장 (한 번만)
//         if (!savedOriginal)
//         {
//             originalTargetPosition = targetObj.transform.position;
//             originalTargetRotation = targetObj.transform.rotation;
//             savedOriginal = true;
//         }

//         // 5. Target을 (-1,2,2.8)로 먼저 이동
//         Vector3 prePosition = new Vector3(-1f, 2f, 2.8f);
//         targetObj.transform.position = prePosition;
//         targetObj.transform.rotation = targetRotation;
//         Debug.Log($"[Go_to_One_Point] Target을 먼저 {prePosition}로 이동");

//         // 6. 입력한 지점으로 이동
//         targetObj.transform.position = targetPosition;
//         targetObj.transform.rotation = targetRotation;
//         Debug.Log($"[Go_to_One_Point] Target 위치: {targetPosition}, 회전: {targetRotation.eulerAngles}");
//     }

//     /// <summary>
//     /// upbox 조인트를 0, 90, -90, 180 중 하나로 고정
//     /// </summary>
//     [ContextMenu("Set Upbox Joint To Selected Angle")]
//     public void SetUpboxJointToSelectedAngle()
//     {
//         SetUpboxJointToAngle((float)upboxAngle);
//     }

//     public void SetUpboxJointToAngle(float angle)
//     {
//         if (bioIK == null)
//         {
//             Debug.LogError("BioIK reference not set!");
//             return;
//         }

//         foreach (var seg in bioIK.Segments)
//         {
//             if (seg.Joint == null) continue;
//             if (seg.Joint.gameObject.name == upboxJointName)
//             {
//                 var motions = new BioJoint.Motion[] { seg.Joint.X, seg.Joint.Y, seg.Joint.Z };
//                 foreach (var motion in motions)
//                 {
//                     if (motion == null || !motion.Enabled) continue;
//                     motion.SetLowerLimit(angle);
//                     motion.SetUpperLimit(angle);
//                     motion.SetTargetValue(angle);

//                     Debug.Log(
//                         $"[SetUpboxJointToAngle] Axis: {motion.Axis}, Angle: {angle}, LowerLimit: {motion.LowerLimit}, UpperLimit: {motion.UpperLimit}, TargetValue: {motion.TargetValue}, Enabled: {motion.Enabled}"
//                     );
//                 }
//                 break;
//             }
//         }
//         // bioIK.Refresh();
//         Debug.Log($"[SetUpboxJointToAngle] upbox 조인트를 {angle}도로 고정");
//     }

//     // 에디터/플레이모드 전환 시 원래 위치로 복원
//     void OnDisable()
//     {
//         RestoreTargetTransform();
//     }
//     void OnApplicationQuit()
//     {
//         RestoreTargetTransform();
//     }
//     private void RestoreTargetTransform()
//     {
//         if (targetObj != null && savedOriginal)
//         {
//             targetObj.transform.position = originalTargetPosition;
//             targetObj.transform.rotation = originalTargetRotation;
//             Debug.Log("[Go_to_One_Point] Target 위치/회전 원래대로 복원");
//         }
//     }
// }
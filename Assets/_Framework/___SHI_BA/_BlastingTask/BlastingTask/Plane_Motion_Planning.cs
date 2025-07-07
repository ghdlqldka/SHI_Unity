// 더이상 사용하지 않는 코드

// using UnityEngine;
// using System.Collections.Generic;
// using System.IO;
// using System.Collections;
// using BioIK;
// using TMPro;
// using System; 

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// public class Plane_Motion_Planning : MonoBehaviour
// {
//     public Plane_and_NV planeAndNV;
//     public BioIK.BioIK bioIK;
//     public IkKuka6JointController jointController;
//     private bool isPainting = false;
//     public FKButtonTester fkTester;
//     public RobotStartPose robotStartPose;
//     public GameObject targetObj;
//     public GameObject TCP;
//     public Make_XML makeXmlInstance;
//     private List<Make_XML.PoseData> xmlPoseBuffer = new List<Make_XML.PoseData>();
//     private int xmlFileIndex = 1;
//     private int globalPoseId = 1;
//     private int lastDebuggedTargetIdx = -1;
//     private int StartingMode = 1;
//     private int MultipleFaceMotionCount = 0;
//     private int PoseReadyCount = 0;
//     private List<Vector3> faceWidthDirs = new List<Vector3>(); // 각 면의 가로 방향 저장
//     private List<Vector3> faceHeightDirs = new List<Vector3>(); // 각 면의 세로 방향 저장
//     private readonly string[] poseNames = { "ready_front", "ready_top", "ready_diag" };
//     private readonly int[] faceOrder = { 1, 2, 3 };

//     [Header("Zigzag 설정")]
//     [Tooltip("가로 방향 점 개수 (최소 2)")]
//     public int numHorizontalPoints = 5;
//     [Tooltip("세로 방향 이동 거리(m)")]
//     public float verticalStep = 0.08f;

//     [Header("Zigzag 평면 오프셋(법선방향)")]
//     [Tooltip("Zigzag 모션이 평면에서 떨어지는 거리(m) - Input_MotionBPS와 자동 연동됩니다 (BPS값의 1/100)")]
//     public float zigzagNormalOffset = 0.6f;

//     [Header("zigzagNormalOffset 자동 연동 설정")]
//     [Tooltip("Input_MotionBPS TMP_InputField를 드래그해서 연결하세요")]
//     public TMP_InputField inputMotionBPS;
//     [Tooltip("수동으로 Motion BPS 값을 설정할 수도 있습니다")]
//     public float manualMotionBPSValue = 60f;
//     [Tooltip("자동 연동을 사용할지 여부")]
//     public bool useMotionBPSAutoSync = true;

//     [Header("verticalStep 자동 연동 설정")]
//     [Tooltip("Input_MotionVertical TMP_InputField를 드래그해서 연결하세요")]
//     public TMP_InputField inputMotionVertical;
//     [Tooltip("수동으로 Motion Vertical 값을 설정할 수도 있습니다")]
//     public float manualMotionVerticalValue = 8f;
//     [Tooltip("자동 연동을 사용할지 여부")]
//     public bool useMotionVerticalAutoSync = true;

//     [Header("면별 Zigzag 모션 좌표 (자동 생성)")]
//     public List<List<Vector3>> allFaceMotions = new List<List<Vector3>>();

//     [Header("모션 추출 기능")]
//     [Tooltip("모션을 추출할 면 번호 (1부터 시작)")]
//     public int selectedFaceIndex = 1;
//     public TMP_InputField inputSelectedFaceIndex;
//     // public TMP_InputField selectedFaceIndex = 1;

//     [Header("Motion CSV 파일명 (예: FaceMotion_1.csv)")]
//     public string motionCsvFileName = "FaceMotion_1.csv";

//     // Motion BPS 값 추적용 (변경 감지)
//     private float previousMotionBPSValue = -1f;
    
//     // Motion Vertical 값 추적용 (변경 감지)
//     private float previousMotionVerticalValue = -1f;

//     [System.Serializable]
//     public class Motion_Making
//     {
//         public Vector3 start;
//         public Vector3 end;
//         public int faceIndex;
//         public string startName;
//         public string endName;
//     }

//     [System.Serializable]
//     public struct FaceEndpoints
//     {
//         public int faceIndex;
//         public Vector3 startPoint;
//         public Vector3 endPoint;
//         public string startName;
//         public string endName;
//     }

//     public List<Motion_Making> motions = new List<Motion_Making>();
//     public List<Vector3> faceNormals = new List<Vector3>();
//     public List<FaceEndpoints> faceEndpointsList = new List<FaceEndpoints>();

//     // 상태머신용 변수
//     private List<Vector3> currentPath;
//     private List<Quaternion> currentRotations; // 회전값 저장용 변수 추가
//     private int currentTargetIdx = 0;
//     private int currentConnectionIdx = 0;
//     private bool isMotionActive = false;
//     private int prevTargetIdx = -1;
//     private int prevTargetIdx_conection = -1;
//     private Transform eef;
//     private Vector3 normal;
//     private int cnt_bm = 0;
//     private bool check_pose_correction = true; 

//     void Start()
//     {
//         string xmlDir = Application.dataPath + "/Motion_XML";
//         if (Directory.Exists(xmlDir))
//         {
//             var files = Directory.GetFiles(xmlDir, "*.xml");
//             foreach (var file in files)
//             {
//                 try
//                 {
//                     File.Delete(file);
//                 }
//                 catch (System.Exception ex)
//                 {
//                     Debug.LogWarning($"[Start] 파일 삭제 실패: {file} - {ex.Message}");
//                 }
//             }
//             Debug.Log($"[Start] {xmlDir} 내 모든 xml 파일 삭제 완료 ({files.Length}개)");
//         }
//         else
//         {
//             Directory.CreateDirectory(xmlDir);
//             Debug.Log($"[Start] Motion_XML 폴더가 없어 새로 생성했습니다: {xmlDir}");
//         }
        
//         if (inputSelectedFaceIndex != null)
//         {
//             inputSelectedFaceIndex.onValueChanged.AddListener(OnSelectedFaceIndexChanged);
//             OnSelectedFaceIndexChanged(inputSelectedFaceIndex.text); // 초기값 반영
//         }
//         if (TCP != null)
//         {
//             eef = TCP.transform;
//             Debug.Log($"[Plane_Motion_Planning] eef 초기화 완료: {eef.name}");
//         }
//         else
//         {
//             Debug.LogError("[Plane_Motion_Planning] TCP가 설정되지 않았습니다.");
//         }
//     }
//     private void OnSelectedFaceIndexChanged(string value)
//     {
//         if (int.TryParse(value, out int idx))
//             selectedFaceIndex = idx;
//         else
//             selectedFaceIndex = 1;
//     }

//     void Update()
//     {
//         // Motion Vertical 값 자동 연동 처리 (최우선)
//         if (useMotionVerticalAutoSync)
//         {
//             UpdateVerticalStepFromMotionVertical();
//         }

//         // Motion BPS 값 자동 연동 처리 (기존)
//         if (useMotionBPSAutoSync)
//         {
//             UpdateZigzagNormalOffsetFromMotionBPS();
//         }

//         // 기존 모션 처리 로직
//         if (!isMotionActive || currentPath == null || targetObj == null || eef == null)
//         {
//             // Debug.LogError($"Update 중단: isMotionActive={isMotionActive}, currentPath={currentPath}, targetObj={targetObj}, eef={eef}");
//             return;
//         }

//         if (currentTargetIdx != prevTargetIdx)
//         {
//             // Debug.Log($"Update 진행: currentTargetIdx={currentTargetIdx}, prevTargetIdx={prevTargetIdx}");

//             if (currentTargetIdx == 0 && StartingMode != 5)
//             {
//                 if (StartingMode == 1)
//                 {
//                     if (robotStartPose != null)
//                     {
//                         // faceOrder와 poseNames를 사용하여 현재 faceIndex에 맞는 poseName을 가져옴
//                         int poseIndex = System.Array.IndexOf(faceOrder, selectedFaceIndex);
//                         if (poseIndex >= 0 && poseIndex < poseNames.Length)
//                         {
//                             string poseName = poseNames[poseIndex];
//                             robotStartPose.ApplyPoseByName(poseName);
//                             Debug.Log($"[MotionPlanning] StartingMode 3: robotStartPose 적용 완료 (faceIndex: {selectedFaceIndex}, pose: {poseName})");
//                         }
//                         else
//                         {
//                             Debug.LogWarning($"[MotionPlanning] selectedFaceIndex({selectedFaceIndex})에 해당하는 poseName을 찾을 수 없습니다.");
//                         }
//                     }
//                     else
//                     {
//                         Debug.LogWarning("[MotionPlanning] robotStartPose가 설정되지 않았습니다.");
//                     }

//                     float panAngle = Normal_to_Pan(selectedFaceIndex);
//                     Debug.Log($"[MotionPlanning] 도출된 panAngle: {panAngle} (normal: {normal})");
//                     RotateUpboxDirectly(panAngle);
//                     GameObject upboxObj = GetUpboxGameObject();
//                     if (upboxObj != null)
//                     {
//                         Debug.Log($"[MotionPlanning] upboxObj.transform.localRotation: {upboxObj.transform.localRotation.eulerAngles}");
//                     }
//                     else
//                     {
//                         Debug.LogWarning("[MotionPlanning] upbox GameObject를 찾지 못했습니다.");
//                     }

//                     jointController.EnableOnlyUpbox(panAngle);
//                     jointController.EnableOnlyGauntry();

//                     cnt_bm++;
//                     if (cnt_bm >= 300)
//                     {
//                         cnt_bm = 0;
//                         StartingMode++;
//                         Debug.Log("[MotionPlanning] StartingMode를 2로 변경");

//                     }
//                 }
//                 else if (StartingMode == 2)
//                 {
//                     jointController.EnableOnlyGauntry();
//                     // bioIK.Refresh();
//                     LogBioIKJointStatus("currentTargetIdx=0, StartingMode=2, After EnableOnlyGauntry");

//                     // Target position만 설정
//                     Vector3 targetPos = currentPath[currentTargetIdx];
//                     targetObj.transform.position = targetPos;

//                     Debug.Log($"[MotionPlanning] StartingMode 2: Target moved to first path point: {targetObj.transform.position}");
//                     cnt_bm++;
//                     if (cnt_bm >= 300)
//                     {
//                         cnt_bm = 0;
//                         StartingMode++;
//                         Debug.Log("[MotionPlanning] StartingMode를 3로 변경");
//                     }
//                 }
//                 else if (StartingMode == 3)
//                 {

//                     jointController.EnableOnlyA1toA6();
//                     targetObj.transform.position = currentPath[currentTargetIdx];
//                     targetObj.transform.rotation = currentRotations[currentTargetIdx];
//                     cnt_bm++;
//                     if (cnt_bm >= 600)
//                     {
//                         cnt_bm = 0;
//                         StartingMode++;
//                         Debug.Log("[MotionPlanning] StartingMode를 4로 변경");
//                     }
//                 }
//                 else if (StartingMode == 4)
//                 {
//                     jointController.EnableOnlyGauntry();
//                     cnt_bm++;
//                     if (cnt_bm >= 300)
//                     {
//                         cnt_bm = 0;
//                         StartingMode++;
//                         currentTargetIdx = 0;
//                         prevTargetIdx = -1;
//                         Debug.Log("[MotionPlanning] StartingMode완료");
//                     }


//                 }
//             }
//             else if (currentTargetIdx >= 0 && StartingMode == 5)
//             {
//                 bioIK.autoIK = true;
//                 // bioIK.Refresh();

//                 // Target position만 설정
//                 targetObj.transform.position = currentPath[currentTargetIdx];
//                 targetObj.transform.rotation = currentRotations[currentTargetIdx];

//                 float posErr = Vector3.Distance(eef.position, targetObj.transform.position);
//                 float rotErr = Quaternion.Angle(eef.rotation, targetObj.transform.rotation);

//                 // Debug.Log($"[Motion] 현재 목표 좌표: {targetObj.transform.position}, EEF 위치: {eef.position}, 위치 오차: {posErr}");

//                 Debug.Log($"[Motion] 현재 목표 좌표: {targetObj.transform.position}, EEF 위치: {eef.position}, 위치 오차: {posErr}");
//                 Debug.Log($"[Motion] 현재 목표 회전값: {targetObj.transform.rotation.eulerAngles}, EEF 회전값: {eef.rotation.eulerAngles}, 회전 오차: {rotErr}");
//                 if (posErr > 0.001f && rotErr > 5.0f)
//                 {
//                     Debug.Log($"[Motion] 위치 오차가 크고 회전 오차도 큽니다: 위치 오차 {posErr}, 회전 오차 {rotErr}");
//                     jointController.EnableOnlyGauntry();
//                 }
//                 else if (posErr < 0.001f && rotErr < 5.0f) 
//                 {
//                     // 단일 pose를 현재 파일에 저장 (ID는 자동 연속 부여)
//                     var pose = new Make_XML.PoseData();
//                     makeXmlInstance.FillPoseFromBioIK(bioIK, makeXmlInstance.baseSegment, makeXmlInstance.tcpSegment, pose);
//                     pose.id = globalPoseId++;
//                     xmlPoseBuffer.Add(pose);

//                     if (xmlPoseBuffer.Count == 50)
//                     {
//                         makeXmlInstance.SavePoseListToXml(xmlPoseBuffer, xmlFileIndex);
//                         xmlPoseBuffer.Clear();
//                         xmlFileIndex++;
//                     }

//                     // makeXmlInstance.SaveCurrentPoseToSingleXml(xmlFileIndex, globalPoseId);

//                     // // 50개마다 파일 인덱스 증가
//                     // if (globalPoseId % 50 == 0)
//                     //     xmlFileIndex++;

//                     // globalPoseId++;                    
//                     currentTargetIdx++;
//                     if (currentTargetIdx >= currentPath.Count)
//                     {
//                         if (xmlPoseBuffer.Count > 0)
//                         {
//                             makeXmlInstance.SavePoseListToXml(xmlPoseBuffer, xmlFileIndex);
//                             xmlPoseBuffer.Clear();
//                             xmlFileIndex++;
//                         }
//                         isMotionActive = false;
//                         StartingMode = 1;
//                         currentTargetIdx = 0;
//                         prevTargetIdx = -1;
//                         prevTargetIdx = currentTargetIdx;

//                         // prevTargetIdx = currentTargetIdx;
//                         Debug.Log("[Motion] 모든 모션 좌표 이동 완료");
//                     }
//                 }
//                 else if (posErr > 0.001f) // 그냥 멀때때
//                 {
//                     Debug.Log($"[Motion] 위치 오차가 여전히 존재합니다(case1): {posErr} (목표: {targetObj.transform.position}, EEF: {eef.position})");
//                     jointController.EnableOnlyGauntry();
//                 }
//                 else if (rotErr > 5.0f && check_pose_correction) // 자세만 잡아야 할 때
//                 {
//                     Debug.Log($"[Motion] 위치 오차가 여전히 존재합니다(case2): {posErr} (목표: {targetObj.transform.position}, EEF: {eef.position})");
//                     jointController.EnableOnlyA1toA6();
//                     cnt_bm++;
//                     if (cnt_bm >= 50)
//                     {
//                         cnt_bm = 0;
//                         check_pose_correction = false;   
//                         jointController.EnableOnlyGauntry();
//                     }
//                 }
//                 else // 오류 조건 불만족 시 그냥 진행
//                 {
//                     jointController.EnableOnlyGauntry();
//                     var pose = new Make_XML.PoseData();
//                     makeXmlInstance.FillPoseFromBioIK(bioIK, makeXmlInstance.baseSegment, makeXmlInstance.tcpSegment, pose);
//                     pose.id = globalPoseId++;
//                     xmlPoseBuffer.Add(pose);

//                     if (xmlPoseBuffer.Count == 50)
//                     {
//                         makeXmlInstance.SavePoseListToXml(xmlPoseBuffer, xmlFileIndex);
//                         xmlPoseBuffer.Clear();
//                         xmlFileIndex++;
//                     }                   
//                     currentTargetIdx++;
//                     if (currentTargetIdx >= currentPath.Count)
//                     {
//                         if (xmlPoseBuffer.Count > 0)
//                         {
//                             makeXmlInstance.SavePoseListToXml(xmlPoseBuffer, xmlFileIndex);
//                             xmlPoseBuffer.Clear();
//                             xmlFileIndex++;
//                         }
//                         isMotionActive = false;
//                         StartingMode = 1;
//                         currentTargetIdx = 0;
//                         prevTargetIdx = -1;
//                         prevTargetIdx = currentTargetIdx;
//                         Debug.Log("[Motion] 모든 모션 좌표 이동 완료");
//                     }
//                     Debug.Log("Check");
//                 }
//             }
//         }
//     }

//     // Motion Vertical 값에서 verticalStep 자동 계산 및 업데이트
//     private void UpdateVerticalStepFromMotionVertical()
//     {
//         float currentMotionVerticalValue = GetCurrentMotionVerticalValue();

//         // Motion Vertical 값이 변경되었을 때만 업데이트
//         if (Mathf.Abs(currentMotionVerticalValue - previousMotionVerticalValue) > 0.001f)
//         {
//             // verticalStep = Motion Vertical값 / 100
//             verticalStep = currentMotionVerticalValue / 100f;
//             previousMotionVerticalValue = currentMotionVerticalValue;

//             Debug.Log($"[Plane_Motion_Planning] Motion Vertical값 변경: {currentMotionVerticalValue} → verticalStep: {verticalStep}");

//             // 현재 생성된 모션들이 있다면 재생성 (선택적)
//             // RefreshAllMotionsWithNewVerticalStep(); // 필요시 구현
//         }
//     }

//     // 현재 Motion Vertical 값 가져오기 (InputField 또는 Manual 값)
//     private float GetCurrentMotionVerticalValue()
//     {
//         if (inputMotionVertical != null && !string.IsNullOrEmpty(inputMotionVertical.text))
//         {
//             if (float.TryParse(inputMotionVertical.text, out float inputValue))
//             {
//                 return inputValue;
//             }
//         }

//         // InputField가 없거나 파싱 실패시 Manual 값 사용
//         return manualMotionVerticalValue;
//     }

//     // 수동으로 Motion Vertical 값 설정하는 공개 메서드
//     public void SetMotionVerticalValue(float verticalValue)
//     {
//         manualMotionVerticalValue = verticalValue;

//         if (!useMotionVerticalAutoSync)
//         {
//             verticalStep = verticalValue / 100f;
//             // RefreshAllMotionsWithNewVerticalStep(); // 필요시 구현
//         }
//     }

//     // InputField 연결 확인용 메서드
//     [ContextMenu("Test Motion Vertical Connection")]
//     public void TestMotionVerticalConnection()
//     {
//         float currentMotionVertical = GetCurrentMotionVerticalValue();
//         Debug.Log($"현재 Motion Vertical 값: {currentMotionVertical}, verticalStep: {currentMotionVertical / 100f}");
//     }

//     // Motion BPS 값에서 zigzagNormalOffset 자동 계산 및 업데이트
//     private void UpdateZigzagNormalOffsetFromMotionBPS()
//     {
//         float currentMotionBPSValue = GetCurrentMotionBPSValue();

//         // Motion BPS 값이 변경되었을 때만 업데이트
//         if (Mathf.Abs(currentMotionBPSValue - previousMotionBPSValue) > 0.001f)
//         {
//             // zigzagNormalOffset = Motion BPS값 / 100
//             zigzagNormalOffset = currentMotionBPSValue / 100f;
//             previousMotionBPSValue = currentMotionBPSValue;

//             Debug.Log($"[Plane_Motion_Planning] Motion BPS값 변경: {currentMotionBPSValue} → zigzagNormalOffset: {zigzagNormalOffset}");

//             // 현재 생성된 모션들이 있다면 재생성 (선택적)
//             // RefreshAllMotionsWithNewOffset(); // 필요시 구현
//         }
//     }

//     // 현재 Motion BPS 값 가져오기 (InputField 또는 Manual 값)
//     private float GetCurrentMotionBPSValue()
//     {
//         if (inputMotionBPS != null && !string.IsNullOrEmpty(inputMotionBPS.text))
//         {
//             if (float.TryParse(inputMotionBPS.text, out float inputValue))
//             {
//                 return inputValue;
//             }
//         }

//         // InputField가 없거나 파싱 실패시 Manual 값 사용
//         return manualMotionBPSValue;
//     }

//     // 수동으로 Motion BPS 값 설정하는 공개 메서드
//     public void SetMotionBPSValue(float bpsValue)
//     {
//         manualMotionBPSValue = bpsValue;

//         if (!useMotionBPSAutoSync)
//         {
//             zigzagNormalOffset = bpsValue / 100f;
//             // RefreshAllMotionsWithNewOffset(); // 필요시 구현
//         }
//     }

//     // InputField 연결 확인용 메서드
//     [ContextMenu("Test Motion BPS Connection")]
//     public void TestMotionBPSConnection()
//     {
//         float currentMotionBPS = GetCurrentMotionBPSValue();
//         Debug.Log($"현재 Motion BPS 값: {currentMotionBPS}, zigzagNormalOffset: {currentMotionBPS / 100f}");
//     }

//     // Inspector에서 실시간 확인용
//     void OnValidate()
//     {
//         // 에디터에서 manualMotionBPSValue 변경시 즉시 반영 (기존)
//         if (useMotionBPSAutoSync && Application.isPlaying)
//         {
//             UpdateZigzagNormalOffsetFromMotionBPS();
//         }
        
//         // 에디터에서 manualMotionVerticalValue 변경시 즉시 반영 (신규)
//         if (useMotionVerticalAutoSync && Application.isPlaying)
//         {
//             UpdateVerticalStepFromMotionVertical();
//         }
//     }

//    [ContextMenu("Make Motions")]
//     public void MakeMotions()
//     {
//         Debug.Log("--- MakeMotions() 함수가 호출되었습니다. ---"); 

//         // 폴더 경로 확인 및 생성
//         string xmlDir = Application.dataPath + "/Motion_XML";

//         // string/00000
//         // 
//         // 
//         // 
//         // 
//         // 
//         //  xmlDir = "C:\\Users\\user\\Desktop\\BM_BT_unity\\BM_BT_unity\\Assets\\Motion_XML";
//         if (Directory.Exists(xmlDir))
//         {
//             var files = Directory.GetFiles(xmlDir, "*.xml");
//             foreach (var file in files)
//             {
//                 try
//                 {
//                     File.Delete(file);
//                 }
//                 catch (System.Exception ex)
//                 {
//                     Debug.LogWarning($"[MakeMotions] 파일 삭제 실패: {file} - {ex.Message}");
//                 }
//             }
//             Debug.Log($"[MakeMotions] {xmlDir} 내 모든 xml 파일 삭제 완료 ({files.Length}개)");
//         }
//         else
//         {
//             Debug.LogWarning($"[MakeMotions] 폴더가 존재하지 않습니다: {xmlDir}");
//             Directory.CreateDirectory(xmlDir); // 폴더가 없으면 생성
//             Debug.Log($"[MakeMotions] 폴더 생성 완료: {xmlDir}");
//         }

//         faceEndpointsList.Clear();

//     #if UNITY_EDITOR
//         UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(this);
//         so.Update();
//         numHorizontalPoints = so.FindProperty("numHorizontalPoints").intValue;
//         verticalStep = so.FindProperty("verticalStep").floatValue;
//         zigzagNormalOffset = so.FindProperty("zigzagNormalOffset").floatValue;
//     #endif

//         foreach (Transform child in transform)
//         {
//             if (child.name.StartsWith("MotionPoint_") ||
//                 child.name.StartsWith("MotionLabel_") ||
//                 child.name.StartsWith("QuadAxesLabel_") ||
//                 child.name.StartsWith("ZigzagTarget_") ||
//                 child.name.StartsWith("ZigzagPath_Face_"))
//             {
//                 DestroyImmediate(child.gameObject);
//             }
//         }

//         motions.Clear();
//         allFaceMotions.Clear();
//         faceNormals.Clear();
//         faceWidthDirs.Clear(); // 수정: 리스트 초기화
//         faceHeightDirs.Clear(); // 수정: 리스트 초기화

//         if (planeAndNV == null || planeAndNV.cubes == null || planeAndNV.cubes.Length == 0)
//         {
//             Debug.LogError("Plane_and_NV 또는 면 정보가 없습니다.");
//             return;
//         }
//         if (bioIK == null)
//         {
//             Debug.LogError("BioIK reference not set!");
//             return;
//         }

//         Vector3 robotInit = Vector3.zero;
//         int count = 0;
//         foreach (var seg in bioIK.Segments)
//         {
//             if (seg.Joint == null) continue;
//             string n = seg.Joint.gameObject.name;
//             if (n == "x" || n == "y" || n == "z")
//             {
//                 robotInit += seg.Joint.transform.position;
//                 count++;
//             }
//         }
//         if (count > 0) robotInit /= count;

//         Vector3 prevEnd = Vector3.zero;

//         for (int i = 0; i < planeAndNV.cubes.Length; i++)
//         {
//             var cube = planeAndNV.cubes[i];
//             Vector3[] points = { cube.R1, cube.R2, cube.R3, cube.R4 };
//             string[] pointNames = { "R1", "R2", "R3", "R4" };

//             Vector3 start, end;
//             string startName, endName;
//             if (i == 0)
//             {
//                 int minIdx = 0;
//                 float minDist = Vector3.Distance(robotInit, points[0]);
//                 for (int j = 1; j < 4; j++)
//                 {
//                     float d = Vector3.Distance(robotInit, points[j]);
//                     if (d < minDist)
//                     {
//                         minDist = d;
//                         minIdx = j;
//                     }
//                 }
//                 start = points[minIdx];
//                 startName = pointNames[minIdx];
//                 int endIdx = (minIdx + 2) % 4;
//                 end = points[endIdx];
//                 endName = pointNames[endIdx];
//                 prevEnd = end;
//             }
//             else
//             {
//                 int minIdx = 0;
//                 float minDist = Vector3.Distance(prevEnd, points[0]);
//                 for (int j = 0; j < 4; j++)
//                 {
//                     float d = Vector3.Distance(prevEnd, points[j]);
//                     if (j > 0 && d < minDist)
//                     {
//                         minDist = d;
//                         minIdx = j;
//                     }
//                 }
//                 start = points[minIdx];
//                 startName = pointNames[minIdx];
//                 int endIdx = (minIdx + 2) % 4;
//                 end = points[endIdx];
//                 endName = pointNames[endIdx];
//                 prevEnd = end;
//             }

//             faceNormals.Add(cube.normal);
//             motions.Add(new Motion_Making { start = start, end = end, faceIndex = i + 1, startName = startName, endName = endName });

//             faceEndpointsList.Add(new FaceEndpoints
//             {
//                 faceIndex = i + 1,
//                 startPoint = start,
//                 endPoint = end,
//                 startName = startName,
//                 endName = endName
//             });

//             CreateSphereWithLabel(start, Color.red, $"MotionPoint_Start_{i + 1}", $"{startName}", 0.03f);
//             CreateSphereWithLabel(end, Color.blue, $"MotionPoint_End_{i + 1}", $"{endName}", 0.03f);

//             // 수정: 먼저 GenerateZigzagPath를 호출하여 faceWidthDirs와 faceHeightDirs를 채움
//             List<Vector3> zigzagPath = GenerateZigzagPath(
//                 cube.R1, cube.R2, cube.R3, cube.R4,
//                 start, end,
//                 numHorizontalPoints, verticalStep,
//                 cube.normal, zigzagNormalOffset,
//                 i + 1
//             );
//             allFaceMotions.Add(zigzagPath);

//             // 수정: GenerateZigzagPath 호출 후 VisualizeQuadAxes 호출
//             VisualizeQuadAxes(start, end, cube.normal, i + 1);

//             for (int z = 0; z < zigzagPath.Count; z++)
//             {
//                 GameObject targetObjPoint = new GameObject($"ZigzagTarget_{i + 1}_{z + 1}");
//                 targetObjPoint.transform.position = zigzagPath[z];
//                 targetObjPoint.transform.parent = this.transform;
//             }

//             CreateZigzagPathLine(zigzagPath, i + 1);
//         }
//     }

//     private void CreateZigzagPathLine(List<Vector3> zigzagPath, int faceIndex)
//     {
//         if (zigzagPath == null || zigzagPath.Count < 2)
//         {
//             Debug.LogWarning($"[CreateZigzagPathLine] 면 {faceIndex}: 경로가 너무 짧습니다.");
//             return;
//         }

//         GameObject lineObj = new GameObject($"ZigzagPath_Face_{faceIndex}");
//         lineObj.transform.parent = this.transform;

//         LineRenderer lr = lineObj.AddComponent<LineRenderer>();
//         lr.positionCount = zigzagPath.Count;
//         lr.SetPositions(zigzagPath.ToArray());
//         lr.startWidth = 0.02f;
//         lr.endWidth = 0.02f;
//         lr.useWorldSpace = true;
//         lr.material = new Material(Shader.Find("Sprites/Default"));

//         Color lineColor = GetFaceColor(faceIndex);
//         lr.startColor = lineColor;
//         lr.endColor = lineColor;

//         Debug.Log($"[CreateZigzagPathLine] 면 {faceIndex}: {zigzagPath.Count}개 포인트로 경로 라인 생성 완료");
//     }

//     private Color GetFaceColor(int faceIndex)
//     {
//         Color[] faceColors = {
//             Color.magenta,
//             Color.cyan,
//             Color.yellow,
//             Color.green,
//             new Color(1f, 0.5f, 0f),
//             new Color(0.5f, 0f, 1f),
//             Color.white,
//             Color.gray
//         };

//         int colorIndex = (faceIndex - 1) % faceColors.Length;
//         return faceColors[colorIndex];
//     }

//     [ContextMenu("Toggle Zigzag Lines")]
//     public void ToggleZigzagLines()
//     {
//         bool hasLines = false;

//         foreach (Transform child in transform)
//         {
//             if (child.name.StartsWith("ZigzagPath_Face_"))
//             {
//                 hasLines = true;
//                 break;
//             }
//         }

//         if (hasLines)
//         {
//             var lineList = new List<GameObject>();
//             foreach (Transform child in transform)
//             {
//                 if (child.name.StartsWith("ZigzagPath_Face_"))
//                 {
//                     lineList.Add(child.gameObject);
//                 }
//             }

//             foreach (var line in lineList)
//             {
//                 DestroyImmediate(line);
//             }
//             Debug.Log("[ToggleZigzagLines] 지그재그 라인 제거 완료");
//         }
//         else
//         {
//             for (int i = 0; i < allFaceMotions.Count; i++)
//             {
//                 CreateZigzagPathLine(allFaceMotions[i], i + 1);
//             }
//             Debug.Log("[ToggleZigzagLines] 지그재그 라인 생성 완료");
//         }
//     }

//     // UI 버튼에서 호출할 수 있는 public 메서드
//     public void StartMultiFaceMotion()
//     {
//         StartCoroutine(MultiFaceMotionWithAvoidance());
//     }

//     [ContextMenu("Clear All Motions")]
//     public void ClearAllMotions()
//     {
//         int deletedCount = 0;
//         var deleteList = new List<GameObject>();

//         foreach (Transform child in transform)
//         {
//             if (child.name.StartsWith("MotionPoint_") ||
//                 child.name.StartsWith("MotionLabel_") ||
//                 child.name.StartsWith("QuadAxesLabel_") ||
//                 child.name.StartsWith("ZigzagTarget_") ||
//                 child.name.StartsWith("ZigzagPath_Face_") ||
//                 child.name.StartsWith("QuadAxis_") ||
//                 child.name.StartsWith("FaceConnection_"))
//             {
//                 deleteList.Add(child.gameObject);
//             }
//         }

//         foreach (var obj in deleteList)
//         {
//             DestroyImmediate(obj);
//             deletedCount++;
//         }

//         motions.Clear();
//         allFaceMotions.Clear();
//         faceNormals.Clear();
//         faceEndpointsList.Clear();

//         Debug.Log($"[ClearAllMotions] 🧹 모든 모션 관련 오브젝트 삭제 완료! (삭제된 오브젝트: {deletedCount}개)");
//         Debug.Log("[ClearAllMotions] ✅ 모션 데이터 리스트들도 모두 클리어됨");

// #if UNITY_EDITOR
//         UnityEditor.SceneView.RepaintAll();
// #endif
//     }

//     public IEnumerator MultiFaceMotionWithAvoidance()
//     {
//         // string[] poseNames = { "ready_front", "ready_top", "ready_diag" };
//         // int[] faceOrder = { 1, 2, 3 };
//         int idx = 0;
//         while (idx < faceOrder.Length)
//         {
//             int faceIdx = faceOrder[idx];

//             if (robotStartPose != null)
//             {
//                 // robotStartPose.ApplyPoseWithUpbox(faceIdx, poseNames[idx]);
//                 Debug.Log($"[{faceIdx}번 면] 자세 적용: {poseNames[idx]} 다중면!!!!!!!!!!");
//             }
//             else
//             {
//                 Debug.LogWarning("robotStartPose가 할당되지 않았습니다.");
//             }
//             yield return new WaitForSeconds(0.5f);

//             selectedFaceIndex = faceIdx;
//             MotionStart();
//             Debug.Log($"[{faceIdx}번 면] 모션 시작");

//             while (isMotionActive)
//                 yield return null;

//             idx++;
//         }
//         Debug.Log("[MultiFaceMotionWithAvoidance] 모든 면 모션 완료");
//     }

//     public List<Vector3> GenerateZigzagPath(
//         Vector3 R1, Vector3 R2, Vector3 R3, Vector3 R4,
//         Vector3 start, Vector3 end,
//         int numHorizontalPoints, float verticalStep,
//         Vector3 normal, float normalOffset,
//         int faceIndex
//     )
//     {
//         Vector3[] quadPoints = { R1, R2, R3, R4 };
//         int startIdx = -1;
//         for (int i = 0; i < 4; i++)
//             if ((quadPoints[i] - start).sqrMagnitude < 1e-6f) startIdx = i;
//         if (startIdx == -1) { Debug.LogError("시작점이 쿼드 꼭지점과 일치하지 않음"); return null; }

//         Vector3 widthDir = Vector3.zero, heightDir = Vector3.zero;
//         float widthLen = 0f, heightLen = 0f;
//         if (startIdx == 0)
//         {
//             widthDir = (R2 - R1).normalized; widthLen = (R2 - R1).magnitude;
//             heightDir = (R4 - R1).normalized; heightLen = (R4 - R1).magnitude;
//         }
//         else if (startIdx == 1)
//         {
//             widthDir = (R1 - R2).normalized; widthLen = (R1 - R2).magnitude;
//             heightDir = (R3 - R2).normalized; heightLen = (R3 - R2).magnitude;
//         }
//         else if (startIdx == 2)
//         {
//             widthDir = (R4 - R3).normalized; widthLen = (R4 - R3).magnitude;
//             heightDir = (R2 - R3).normalized; heightLen = (R2 - R3).magnitude;
//         }
//         else if (startIdx == 3)
//         {
//             widthDir = (R3 - R4).normalized; widthLen = (R3 - R4).magnitude;
//             heightDir = (R1 - R4).normalized; heightLen = (R1 - R4).magnitude;
//         }

//         if (faceIndex > faceWidthDirs.Count)
//         {
//             faceWidthDirs.Add(widthDir);
//             faceHeightDirs.Add(heightDir);
//         }
//         else
//         {
//             faceWidthDirs[faceIndex - 1] = widthDir;
//             faceHeightDirs[faceIndex - 1] = heightDir;
//         }

//         List<Vector3> path = new List<Vector3>();
//         Vector3 curr = start;
//         float movedHeight = 0f;
//         bool toRight = true;
//         bool didExtra = false;

//         Vector3 offset = normal.normalized * normalOffset;
//         Debug.Log($"[GenerateZigzagPath] 시작: start={start}, end={end}, widthLen={widthLen}, heightLen={heightLen}, numHorizontalPoints={numHorizontalPoints}, verticalStep={verticalStep}");

//         while (true)
//         {
//             for (int i = 0; i < numHorizontalPoints; i++)
//             {
//                 float t = (float)i / (numHorizontalPoints - 1);
//                 Vector3 point = curr + widthDir * widthLen * (toRight ? t : (1 - t));
//                 path.Add(point + offset);
//                 // Debug.Log($"[GenerateZigzagPath] 줄 {(toRight ? "→" : "←")}, 세로 {movedHeight:F3}, 가로 idx={i}, point={point + offset}");
//             }

//             float nextMovedHeight = movedHeight + verticalStep;
//             if (nextMovedHeight > heightLen)
//             {
//                 float remain = heightLen - movedHeight;
//                 if (!didExtra && remain > 1e-4f)
//                 {
//                     curr += heightDir * remain;
//                     toRight = !toRight;
//                     for (int i = 0; i < numHorizontalPoints; i++)
//                     {
//                         float t = (float)i / (numHorizontalPoints - 1);
//                         Vector3 point = curr + widthDir * widthLen * (toRight ? t : (1 - t));
//                         path.Add(point + offset);
//                         // Debug.Log($"[GenerateZigzagPath] 마지막 줄 {(toRight ? "→" : "←")}, 세로 {movedHeight + remain:F3}, 가로 idx={i}, point={point + offset}");

//                     }
//                     didExtra = true;
//                 }
//                 // Debug.Log($"[GenerateZigzagPath] heightLen 도달, break. 총 path.Count={path.Count}");

//                 break;
//             }
//             curr += heightDir * verticalStep;
//             movedHeight = nextMovedHeight;
//             toRight = !toRight;
//         }
//         // --- 안전지대 추가 ---
//         // 끝나는 점의 안전지대
//         Vector3 safeZoneEnd = end + (widthDir.normalized * 1.5f) + (faceNormals[faceIndex - 1].normalized * normalOffset);
//         path.Add(safeZoneEnd);       

//         // 시작하는 점의 안전지대: 다음 면의 normal 벡터 사용
//         Vector3 nextNormal = faceIndex < faceNormals.Count ? faceNormals[faceIndex] : faceNormals[faceIndex - 1];
//         Vector3 safeZoneStart = start - widthDir * 1.5f + (nextNormal * normalOffset);
//         path.Insert(0, safeZoneStart);

//         Debug.Log($"[GenerateZigzagPathWithSafeZones] 안전지대 추가 완료: safeZoneStart={safeZoneStart}, safeZoneEnd={safeZoneEnd}");

//         return path;
//     }

//     [ContextMenu("Save All Face Motions To CSV")]
//     public void SaveAllFaceMotionsToCSV()
//     {
//         string dir = Application.dataPath + "/MotionData";
//         if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
//         Vector3 prevSafeZoneEnd = Vector3.zero;

//         for (int i = 0; i < allFaceMotions.Count; i++)
//         {
//             string path = $"{dir}/FaceMotion_{i + 1}.csv";

//             using (StreamWriter sw = new StreamWriter(path, false))
//             {
//                 foreach (var pos in allFaceMotions[i])
//                 {
//                     // 면의 normal vector 가져오기
//                     Vector3 normal = faceNormals[i];

//                     // 면의 가로 방향(widthDir)과 세로 방향(heightDir) 계산
//                     Vector3 widthDir = faceWidthDirs[i];
//                     Vector3 heightDir = faceHeightDirs[i];

//                     // 새로운 좌표축 정의: x축 = widthDir, y축 = heightDir, z축 = normal
//                     Vector3 xAxis = widthDir;
//                     Vector3 yAxis = heightDir;
//                     Vector3 zAxis = normal;

//                     // xz 평면에서 z 벡터와 30도 차이나도록 yaw 방향 계산
//                     Vector3 yawDirection = Quaternion.AngleAxis(60, yAxis) * zAxis;

//                     // 회전값 계산
//                     Quaternion rotation = Quaternion.LookRotation(yawDirection, yAxis);

//                     // 오일러 각도 추출
//                     float roll = rotation.eulerAngles.z;
//                     float pitch = rotation.eulerAngles.x;
//                     float yaw = rotation.eulerAngles.y;

//                     // CSV에 x, y, z, roll, pitch, yaw 저장 x, y, z, roll, pitch, yaw 값이 있어야 함
//                     sw.WriteLine($"{pos.x},{pos.y},{pos.z},{roll},{pitch},{yaw}");
//                 }
//             }
//             Debug.Log($"[MotionData] Saved: {path}");
//         }
//     }

//     [ContextMenu("Extract Selected Face Motion")]
//     public void ExtractSelectedFaceMotion()
//     {
//         int idx = selectedFaceIndex - 1;
//         if (idx < 0 || idx >= planeAndNV.cubes.Length)
//         {
//             Debug.LogError("잘못된 면 번호입니다.");
//             return;
//         }

//         foreach (Transform child in transform)
//         {
//             if (child.name.StartsWith("MotionPoint_") ||
//                 child.name.StartsWith("MotionLabel_") ||
//                 child.name.StartsWith("QuadAxesLabel_") ||
//                 child.name.StartsWith("ZigzagTarget_") ||
//                 child.name.StartsWith("ZigzagPath_Face_"))
//             {
//                 DestroyImmediate(child.gameObject);
//             }
//         }

//         allFaceMotions.Clear();
//         motions.Clear();

//         var cube = planeAndNV.cubes[idx];
//         Vector3[] points = { cube.R1, cube.R2, cube.R3, cube.R4 };
//         string[] pointNames = { "R1", "R2", "R3", "R4" };

//         Vector3 start = cube.R1;
//         Vector3 end = cube.R3;
//         string startName = "R1";
//         string endName = "R3";

//         motions.Add(new Motion_Making { start = start, end = end, faceIndex = idx + 1, startName = startName, endName = endName });

//         CreateSphereWithLabel(start, Color.red, $"MotionPoint_Start_{idx + 1}", $"{startName}", 0.03f);
//         CreateSphereWithLabel(end, Color.blue, $"MotionPoint_End_{idx + 1}", $"{endName}", 0.03f);

//         List<Vector3> zigzagPath = GenerateZigzagPath(
//             cube.R1, cube.R2, cube.R3, cube.R4,
//             start, end,
//             numHorizontalPoints, verticalStep,
//             cube.normal, zigzagNormalOffset,
//             selectedFaceIndex
//         );
//         allFaceMotions.Add(zigzagPath);

//         for (int z = 0; z < zigzagPath.Count; z++)
//         {
//             GameObject targetObjPoint = new GameObject($"ZigzagTarget_{idx + 1}_{z + 1}");
//             targetObjPoint.transform.position = zigzagPath[z];
//             targetObjPoint.transform.parent = this.transform;
//         }

//         CreateZigzagPathLine(zigzagPath, idx + 1);
//         SaveSelectedFaceMotionToCSV(idx, zigzagPath);
//     }

//     public void SaveSelectedFaceMotionToCSV(int idx, List<Vector3> path)
//     {
//         string dir = Application.dataPath + "/MotionData";
//         if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

//         string filePath = $"{dir}/FaceMotion_{idx + 1}.csv";
//         using (StreamWriter sw = new StreamWriter(filePath, false))
//         {
//             foreach (var pos in path)
//             {
//                 sw.WriteLine($"{pos.x},{pos.y},{pos.z}");
//             }
//         }
//         Debug.Log($"[MotionData] Saved: {filePath}");
//     }

//     [ContextMenu("Motion Start")]
//     public void MotionStart()
//     {
//         // Debug.Log("--- MotionStart() 함수가 호출되었습니다. ---");

//         // string resourcePath = $"MotionData/FaceMotion_{selectedFaceIndex}"; // 예: "MotionData/FaceMotion_1"
//         // TextAsset csvFile = Resources.Load<TextAsset>(resourcePath); //
//         // Debug.Log($"시도하는 리소스 경로: '{resourcePath}'");

//         // if (csvFile == null)
//         // {
//         //     Debug.LogError($"[MotionStart] Resources에서 CSV 파일 '{resourcePath}'를 찾을 수 없습니다. 파일이 Assets/Resources/MotionData/ 에 있는지 확인하세요.");
//         //     return;
//         // }

//         // string csvContent = csvFile.text; // TextAsset의 내용을 문자열로 가져옵니다.
        
//         string dir = Application.dataPath + "/MotionData";
//         if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

//         string filePath = Path.Combine(dir, $"FaceMotion_{selectedFaceIndex}.csv");

//         if (!File.Exists(filePath))
//         {
//             Debug.LogError($"CSV 파일이 존재하지 않습니다: {filePath}");
//             return;
//         }

//         List<Vector3> path = new List<Vector3>();
//         List<Quaternion> rotations = new List<Quaternion>(); // 회전값 저장용 리스트
//         using (StreamReader sr = new StreamReader(filePath))
//         // using (StringReader sr = new StringReader(csvContent))
//         {
//             string line;
//             while ((line = sr.ReadLine()) != null)
//             {
//                 var tokens = line.Split(',');
//                 if (tokens.Length >= 6) // x, y, z, roll, pitch, yaw 값이 있어야 함
//                 {
//                     try
//                     {
//                         float x = float.Parse(tokens[0]);
//                         float y = float.Parse(tokens[1]);
//                         float z = float.Parse(tokens[2]);
//                         float roll = float.Parse(tokens[3]);
//                         float pitch = float.Parse(tokens[4]);
//                         float yaw = float.Parse(tokens[5]);

//                         path.Add(new Vector3(x, y, z));
//                         rotations.Add(Quaternion.Euler(pitch, yaw, roll));
//                     }
//                     catch (System.Exception ex)
//                     {
//                         Debug.LogError($"CSV 데이터 파싱 실패: {line} - {ex.Message}");
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogWarning($"CSV 데이터 형식이 올바르지 않습니다: {line}");
//                 }
//             }
//         }

//         if (path.Count == 0 || rotations.Count == 0)
//         {
//             Debug.LogError("CSV에서 좌표 또는 회전값을 읽지 못했습니다.");
//             return;
//         }

//         currentPath = path;
//         currentRotations = rotations; // 회전값 리스트 저장
//         currentTargetIdx = 0;
//         prevTargetIdx = -1;
//         StartingMode = 1;
//         isMotionActive = true;
//         check_pose_correction = true;

//         Debug.Log($"총 {path.Count}개의 좌표와 회전값을 불러왔습니다.");


//         // MotionStart() XML 저장 초기화
//         string xmlDir = Application.dataPath + "/Motion_XML";
//         if (!Directory.Exists(xmlDir))
//             Directory.CreateDirectory(xmlDir);

//         // 폴더 내 motion_*.xml 파일 검사
//         var files = Directory.GetFiles(xmlDir, "motion_*.xml");
//         int maxIndex = 0;
//         foreach (var file in files)
//         {
//             string name = Path.GetFileNameWithoutExtension(file);
//             if (name.StartsWith("motion_"))
//             {
//                 if (int.TryParse(name.Substring(7), out int idx))
//                     if (idx > maxIndex) maxIndex = idx;
//             }
//         }
//         xmlFileIndex = maxIndex + 1; // 다음 파일 번호
//         globalPoseId = 1; // pose id 1부터 시작
//         xmlPoseBuffer.Clear();
//         Debug.Log($"[MotionStart] motion_{xmlFileIndex}.xml부터 저장 시작, pose id {globalPoseId}부터");

//     }

//     public float Normal_to_Pan(int faceIndex)
//     {
//         if (faceIndex < 1 || faceIndex > faceNormals.Count)
//         {
//             Debug.LogError($"[Normal_to_Pan] 잘못된 면 index: {faceIndex}");
//             return 0f;
//         }

//         Vector3 normal = faceNormals[faceIndex - 1]; // 면 index에 따라 normal 값을 가져옴
//         float[] angles = { 0f, 90f, 180f, -90f };
//         Vector3[] xDirs = {
//             Vector3.right,
//             Vector3.back,
//             Vector3.left,
//             Vector3.forward
//         };

//         Vector3 normalXZ = new Vector3(normal.x, 0, normal.z).normalized;
//         Debug.Log($"[Plane_Motion_Planning] 면 {faceIndex}의 normal xz 평면 투영값: {normalXZ}");

//         float minDot = float.PositiveInfinity;
//         int bestIdx = 0;
//         for (int i = 0; i < 4; i++)
//         {
//             Vector3 xDirXZ = new Vector3(xDirs[i].x, 0, xDirs[i].z).normalized;
//             float dot = Vector3.Dot(normalXZ, xDirXZ);
//             if (dot < minDot)
//             {
//                 minDot = dot;
//                 bestIdx = i;
//             }
//         }
//         float panAngle = angles[bestIdx];
//         Debug.Log($"[Plane_Motion_Planning] 면 {faceIndex}의 선택된 pan 각도: {panAngle}도");
//         return panAngle;
//     }

//     public GameObject GetUpboxGameObject()
//     {
//         if (bioIK == null || bioIK.Segments == null)
//             return null;

//         foreach (var segment in bioIK.Segments)
//         {
//             if (segment.Joint != null && segment.Joint.gameObject.name == "upbox")
//             {
//                 return segment.Joint.gameObject;
//             }
//         }
//         return null;
//     }

//     public void RotateUpboxDirectly(float angle)
//     {
//         if (bioIK != null)
//             bioIK.autoIK = false;

//         GameObject upboxObj = GetUpboxGameObject();
//         if (upboxObj != null)
//         {
//             upboxObj.transform.localRotation = Quaternion.Euler(0, 0, angle);
//             Debug.Log($"[Plane_Motion_Planning] IK OFF, upbox를 {angle}도로 직접 회전");
//         }
//         else
//         {
//             Debug.LogWarning("[Plane_Motion_Planning] upbox GameObject를 찾지 못했습니다.");
//         }

//         bioIK.autoIK = true;
//     }

//     void LogBioIKJointStatus(string contextMessage)
//     {
//         if (bioIK == null)
//         {
//             Debug.LogWarning($"[LogBioIKJointStatus - {contextMessage}] BioIK component is not assigned.");
//             return;
//         }
//         if (bioIK.Segments == null)
//         {
//             Debug.LogWarning($"[LogBioIKJointStatus - {contextMessage}] BioIK.Segments is null.");
//             return;
//         }

//         Debug.Log($"---------- [BioIK Joint Status at: {contextMessage}] ----------");
//         Debug.Log($"Current bioIK.autoIK state: {bioIK.autoIK}");

//         foreach (var segment in bioIK.Segments)
//         {
//             if (segment.Joint != null)
//             {
//                 string jointName = segment.Joint.gameObject.name;
//                 bool isJointComponentEnabled = segment.Joint.enabled;
//                 string status = $"Joint: {jointName}, Component.enabled: {isJointComponentEnabled}";

//                 if (isJointComponentEnabled)
//                 {
//                     if (segment.Joint.X != null)
//                     {
//                         status += $"\n  L X-Axis: Motion.Enabled={segment.Joint.X.Enabled}, Limits=[{segment.Joint.X.GetLowerLimit():F2}, {segment.Joint.X.GetUpperLimit():F2}], TargetVal={segment.Joint.X.GetTargetValue():F2}, CurrentVal={segment.Joint.X.GetCurrentValue():F2}";
//                     }
//                     else
//                     {
//                         status += "\n  L X-Axis: Motion object is null";
//                     }

//                     if (segment.Joint.Y != null)
//                     {
//                         status += $"\n  L Y-Axis: Motion.Enabled={segment.Joint.Y.Enabled}, Limits=[{segment.Joint.Y.GetLowerLimit():F2}, {segment.Joint.Y.GetUpperLimit():F2}], TargetVal={segment.Joint.Y.GetTargetValue():F2}, CurrentVal={segment.Joint.Y.GetCurrentValue():F2}";
//                     }
//                     else
//                     {
//                         status += "\n  L Y-Axis: Motion object is null";
//                     }

//                     if (segment.Joint.Z != null)
//                     {
//                         status += $"\n  L Z-Axis: Motion.Enabled={segment.Joint.Z.Enabled}, Limits=[{segment.Joint.Z.GetLowerLimit():F2}, {segment.Joint.Z.GetUpperLimit():F2}], TargetVal={segment.Joint.Z.GetTargetValue():F2}, CurrentVal={segment.Joint.Z.GetCurrentValue():F2}";
//                     }
//                     else
//                     {
//                         status += "\n  L Z-Axis: Motion object is null";
//                     }
//                 }
//                 Debug.Log(status);
//             }
//         }
//         Debug.Log($"-------------------- [End of Status for: {contextMessage}] --------------------");
//     }

//     private void VisualizeQuadAxes(Vector3 start, Vector3 end, Vector3 normal, int quadIndex)
//     {
//         if (quadIndex < 1 || quadIndex > faceWidthDirs.Count || quadIndex > faceHeightDirs.Count)
//         {
//             Debug.LogError($"[VisualizeQuadAxes] 잘못된 면 index: {quadIndex}");
//             Debug.Log($"[Debug Info] quadIndex: {quadIndex}, faceWidthDirs.Count: {faceWidthDirs.Count}, faceHeightDirs.Count: {faceHeightDirs.Count}");

//             return;
//         }

//         Vector3 widthDir = faceWidthDirs[quadIndex - 1]; // 전역 리스트에서 가로 방향 가져오기
//         Vector3 heightDir = faceHeightDirs[quadIndex - 1]; // 전역 리스트에서 세로 방향 가져오기

//         float axisLength = 0.5f;
//         float normalOffset = 0.1f;
//         Vector3 center = (start + end) * 0.5f + normal * normalOffset;

//         CreateArrow(center, widthDir, axisLength, Color.red, $"QuadAxis_Width_{quadIndex}");
//         CreateArrow(center, heightDir, axisLength, Color.green, $"QuadAxis_Height_{quadIndex}");
//         CreateArrow(center, normal, axisLength, Color.blue, $"QuadAxis_Normal_{quadIndex}");

//         GameObject labelObj = new GameObject($"QuadAxesLabel_{quadIndex}");
//         labelObj.transform.position = center + normal * (axisLength * 1.1f);
//         labelObj.transform.parent = this.transform;
//         var text = labelObj.AddComponent<TextMesh>();
//         text.text = $"면{quadIndex}\nR:가로\nG:세로\nB:법선";
//         text.fontSize = 32;
//         text.characterSize = 0.07f;
//         text.color = Color.white;
//         text.anchor = TextAnchor.MiddleCenter;
//     }

//     private void CreateArrow(Vector3 start, Vector3 dir, float length, Color color, string name)
//     {
//         Vector3 end = start + dir * length;
//         Vector3 headBase = end - dir * (length * 0.2f);
//         GameObject body = new GameObject(name + "_Body");
//         body.transform.parent = this.transform;
//         var lr = body.AddComponent<LineRenderer>();
//         lr.positionCount = 2;
//         lr.SetPosition(0, start);
//         lr.SetPosition(1, headBase);
//         lr.startWidth = 0.03f;
//         lr.endWidth = 0.03f;
//         lr.material = new Material(Shader.Find("Sprites/Default"));
//         lr.startColor = color;
//         lr.endColor = color;
//     }

//     private void CreateSphereWithLabel(Vector3 pos, Color color, string sphereName, string label, float scale = 0.2f)
//     {
//         GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//         sphere.transform.position = pos;
//         sphere.transform.localScale = Vector3.one * scale;
//         var mat = new Material(Shader.Find("Standard"));
//         mat.color = color;
//         var renderer = sphere.GetComponent<MeshRenderer>();
//         renderer.material = mat;
//         renderer.sharedMaterial = mat;
//         sphere.name = sphereName;
//         sphere.transform.parent = this.transform;

// #if UNITY_EDITOR
//         UnityEditor.SceneView.lastActiveSceneView.pivot = pos;
//         UnityEditor.SceneView.lastActiveSceneView.Repaint();
// #endif

//         GameObject textObj = new GameObject($"MotionLabel_{label}");
//         textObj.transform.position = pos + Vector3.up * scale * 2f;
//         textObj.transform.parent = this.transform;
//         var text = textObj.AddComponent<TextMesh>();
//         text.text = label;
//         text.fontSize = 80;
//         text.characterSize = scale * 0.7f;
//         text.color = color;
//         text.anchor = TextAnchor.MiddleCenter;
//     }
// }

// #if UNITY_EDITOR
// [CustomEditor(typeof(Plane_Motion_Planning))]
// public class PlaneMotionPlanningEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();

//         Plane_Motion_Planning script = (Plane_Motion_Planning)target;
//         if (GUILayout.Button("Make Motions"))
//         {
//             script.MakeMotions();
//         }
//         if (GUILayout.Button("Multi Face Motion With Avoidance"))
//         {
//             script.StartCoroutine(script.MultiFaceMotionWithAvoidance());
//         }
//     }
// }
// #endif
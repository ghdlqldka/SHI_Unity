// BlastingTask/main.cs
using _SHI_BA;
using BioIK;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static _SHI_BA.BA_Motion;
using static UnityEngine.Rendering.DebugUI;

namespace _SHI_BA
{
    public class BA_Main : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FFC300><b>[BA_Main]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected BA_GeneratedData _generatedData = new BA_GeneratedData();
        public BA_GeneratedData GeneratedData
        {
            get { return _generatedData; }
        }




        [Space(10)]
        // #region 변수 선언
        [Header("핵심 컴포넌트 참조")]
        public Plane_and_NV planeAndNV;
        public BA_BioIK bioIK;
        public Gauntry_or_6Joint_or_all jointController;
        public RobotStartPose robotStartPose;
        public Make_XML makeXmlInstance;
        public GameObject targetObj;
        public GameObject TCP;
        public Toggle_Obstacle_Avoiding toggleObstacleAvoiding;
        private RaycastVisualizer raycastVisualizer;

        [Header("Path Point Visualization")]
        [Tooltip("경로의 각 지점에 표시할 프리팹을 여기에 할당하세요.")]
        public GameObject targetPrefab;

        [Header("XML 전송 컴포넌트")]
        public XMLSender xmlSender;

        [Header("경로 생성 설정")]
        [Tooltip("가로 방향 점 개수 (최소 2)")]
        public int numHorizontalPoints = 2;
        [Tooltip("세로 방향 이동 거리(m)")]
        private float verticalStep = 0.08f;
        [Tooltip("경로와 작업면 사이의 거리(m)")]
        public float zigzagNormalOffset = 0.6f;

        [Header("장애물 회피 설정")]
        [Tooltip("장애물 Collider를 이 배율만큼 키워서 안전거리를 확보합니다.")]
        private float obstacleScaleFactor = 1.1f;
        [Tooltip("장애물 표면에서 이 거리(m)만큼 경로를 띄웁니다.")]
        private float obstacleOffset = 0.00f;

        [Header("UI 자동 연동 설정")]
        public bool useMotionVerticalAutoSync = true;
        public float manualMotionVerticalValue = 8f;
        public bool useMotionBPSAutoSync = true;
        public float manualMotionBPSValue = 60f;

        // [Header("모션 실행 설정 (UI)")]
        // public TMP_InputField inputSelectedFaceIndex;

        [Header("다중 면 순차 실행 설정")]
        [Tooltip("순차적으로 작업할 면의 순서")]
        private int[] faceOrder;
        [Tooltip("각 면 순서에 맞춰 적용할 로봇 자세 이름")]
        private string[] poseNames = {
            "weaving1", "curved", "curved", "curved", "curved", "curved",
            "ready_front", "ready_top2", "ready_top", "ready_diag", "ready_deep"
        };

        [Header("장애물 설정")]
        public List<Collider> obstacleColliders = new List<Collider>();

        [Header("시뮬레이션 상태 관리")]
        public UI_Simulation_status simulationStatus;

        [Header("충돌 감지 설정")]
        public LayerMask detectionLayer;

        [Header("Path Generation Dependencies")]
        public CubeStruct cuboid;


        // --- 관리 모듈 ---
        private MotionPathGenerator pathGenerator;
#if false // @#####
        private MotionVisualizer visualizer;
#endif
        public XmlPathRecorder xmlRecorder;
        public RobotMotionExecutor motionExecutor;

        // --- 데이터 저장소 ---
        // public List<BA_MotionPath> generatedPaths;

        // --- 내부 상태 변수 ---
        private float previousMotionVerticalValue = -1f;
        private float previousMotionBPSValue = -1f;
        private bool isLogicStarted = false;
        // #endregion

        private void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

            Debug.Assert(TCP != null);
            Debug.Assert(planeAndNV != null);

            // ▼▼▼ [Step 2] 관리 모듈 초기화 로직을 수정합니다. ▼▼▼
            pathGenerator = new MotionPathGenerator(TCP.transform);
#if false // @#####
            visualizer = new MotionVisualizer(this.transform, this.targetPrefab);
#endif
            xmlRecorder = new XmlPathRecorder(); // 생성자에서 인자 제거

            // RobotMotionExecutor 생성 시 xmlSender를 전달하지 않습니다. (역할 분리)
            motionExecutor = new RobotMotionExecutor(this, bioIK, targetObj, TCP.transform, jointController, robotStartPose, xmlRecorder, toggleObstacleAvoiding);

#if false // @#####
            // RaycastVisualizer 연결
            RaycastVisualizer raycastVisualizer = this.GetComponent<RaycastVisualizer>();
            Debug.Assert(raycastVisualizer != null);
            raycastVisualizer.executor = this.motionExecutor;
#endif

#if false //
            // 기존에 생성된 로컬 XML 파일이 있다면 삭제
            if (xmlRecorder != null)
            {
                // 이 기능은 이제 XMLSender로 옮겨가거나, 필요 시점에 수동으로 호출하는 것이 좋습니다.
                // xmlRecorder.ClearExistingXmlFiles(); 
            }
#endif
            toggleObstacleAvoiding.SetShapeDistanceObjectives(false);
        }

        private void Start()
        {
            if (this.cuboid != null)
            {
                pathGenerator.cuboid = this.cuboid;
            }
            else
            {
                Debug.LogError("'main' 스크립트의 Cuboid 필드에 CubeStruct 오브젝트가 할당되지 않았습니다!");
            }
            jointController.EnableOnlyGauntry();
        }

        // #region 실제 명령
        void Update()
        {
            if (useMotionVerticalAutoSync)
            {
                UpdateVerticalStepFromUI();
            }
            if (useMotionBPSAutoSync)
            { 
                UpdateNormalOffsetFromUI();
            }
        }

        [ContextMenu("1. 경로 생성 및 시각화")]
        public void GenerateAndVisualizePaths()
        {
            // [A] 초기화 및 준비 단계
            if (planeAndNV.JsonLoader.IsDataLoaded == false)
            {
                Debug.LogWarning("[MainController] 면 데이터가 아직 로드되지 않았습니다. 잠시 후 다시 시도해주세요.");
                return;
            }

            // visualizer.ClearAllVisuals();
            pathGenerator.ClearPathData();

            var config = new PathGenerationConfig
            {
                NumHorizontalPoints = this.numHorizontalPoints,
                VerticalStep = this.verticalStep,
                NormalOffset = this.zigzagNormalOffset,
                ObstacleOffset = obstacleOffset,
                ObstacleScaleFactor = obstacleScaleFactor
            };

            // --- [B] 경로 순서 재지정 로직 ---

            // 1. 순서를 담을 새로운 경로 리스트를 초기화합니다.
            var orderedPaths = new List<BA_MotionPath>();
            Debug.Log("경로 순서 재지정 시작: W1 면을 먼저 생성합니다.");
            int rFaceIndexOffset = 0;
            Vector3 robotInitPos = GetRobotInitialPosition(); // 로봇 초기 위치 가져오기

            // 2. W1 면의 경로를 먼저 생성합니다.
            if (planeAndNV.JsonLoader.W_Faces.TryGetValue("W1", out Cube w1Face))
            {
                var w1Points = new Vector3[] { w1Face.R1, w1Face.R2, w1Face.R3, w1Face.R4 };
                Vector3 startPoint = w1Points.OrderBy(p => Vector3.Distance(robotInitPos, p)).First();
                Vector3 endPoint = w1Points.OrderByDescending(p => Vector3.Distance(startPoint, p)).First();

                // 3. GenerateZigzagPathForFace 메서드를 호출합니다.
                //    (faceId는 1로 고정하고, 장애물 리스트는 비워둡니다.)
                BA_PathDataManager.Instance.w1Path = pathGenerator.GenerateZigzagPathForFace(w1Face, startPoint, endPoint, config, 1, new List<Collider>());

                if (BA_PathDataManager.Instance.w1Path != null)
                {
                    // 4. 생성된 W1 경로를 최종 리스트의 첫 번째로 추가하고, 오프셋을 설정합니다.
                    orderedPaths.Add(BA_PathDataManager.Instance.w1Path);
                    rFaceIndexOffset = BA_PathDataManager.Instance.w1Path.FaceIndex; // R 계열 면의 인덱스 시작 번호를 W1 다음으로 설정
                    Debug.Log($"W1 면 Zigzag 경로가 리스트의 첫 번째로 추가되었습니다 (FaceIndex: {BA_PathDataManager.Instance.w1Path.FaceIndex}). R 계열 시작 오프셋: {rFaceIndexOffset}");
                }

                ////// @@@@@@@@@@@@@@@@@
                BA_PathLineManager.Instance.LineRendererDic["BilgeKeel01"].PointList = BA_PathDataManager.Instance.w1Path.PointList;
                ////// @@@@@@@@@@@@@@@@@
            }
            else
            {
                Debug.LogWarning("W1 면을 찾을 수 없어 W1 경로 생성을 건너뜁니다. R 계열 면만 생성됩니다.");
            }

            // 4. 기존 R 계열 면들의 경로를 생성합니다.
            robotInitPos = GetRobotInitialPosition();
            BA_PathDataManager.Instance.rPaths = pathGenerator.GenerateAllPaths(planeAndNV.JsonLoader.R_Faces, config, robotInitPos, obstacleColliders, rFaceIndexOffset);

            // 5. 생성된 R 계열 경로들을 최종 리스트에 추가합니다.
            if (BA_PathDataManager.Instance.rPaths != null && BA_PathDataManager.Instance.rPaths.Count > 0)
            {
                orderedPaths.AddRange(BA_PathDataManager.Instance.rPaths);
            }

            ////// @@@@@@@@@@@@@@@@@
            BA_PathLineManager.Instance.LineRendererDic["Curve01"].PointList = BA_PathDataManager.Instance.rPaths[0].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Curve02"].PointList = BA_PathDataManager.Instance.rPaths[1].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Curve03"].PointList = BA_PathDataManager.Instance.rPaths[2].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Curve04"].PointList = BA_PathDataManager.Instance.rPaths[3].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Curve05"].PointList = BA_PathDataManager.Instance.rPaths[4].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Plane01"].PointList = BA_PathDataManager.Instance.rPaths[5].PointList;
            // BA_PathLineManager.Instance.LineRendererDic["StiffBot01"].PointList = GeneratedData.rPaths[6].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Plane02"].PointList = BA_PathDataManager.Instance.rPaths[7].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Plane03"].PointList = BA_PathDataManager.Instance.rPaths[8].PointList;
            BA_PathLineManager.Instance.LineRendererDic["Plane04"].PointList = BA_PathDataManager.Instance.rPaths[9].PointList;
            ////// @@@@@@@@@@@@@@@@@






            // 6. 최종적으로, 순서가 재정렬된 경로 리스트를 `generatedPaths`에 할당합니다.
            BA_PathDataManager.Instance.generatedPaths = orderedPaths;

            // [C] 최종 경로 시각화
            if (BA_PathDataManager.Instance.generatedPaths == null || BA_PathDataManager.Instance.generatedPaths.Count == 0)
            {
                Debug.LogError("[MainController] 경로 생성에 실패했습니다.");
                return;
            }

#if false // @#####
            foreach (var path in GeneratedData.generatedPaths)
            {
                visualizer.VisualizePath(path);
            }
#endif

            Debug.Log($"[MainController] 총 {BA_PathDataManager.Instance.generatedPaths.Count}개의 면에 대한 경로 생성 및 시각화 완료. (W1 경로가 가장 먼저 시작됩니다)");
        }

        [ContextMenu("2. 선택된 면 모션 실행")]
        public void ExecuteMotionForSelectedFace(int faceIndex)
        {
            if (motionExecutor.IsExecuting)
            {
                Debug.LogWarning("[MainController] 현재 다른 모션이 실행 중입니다.");
                return;
            }
            if (BA_PathDataManager.Instance.generatedPaths == null || BA_PathDataManager.Instance.generatedPaths.Count == 0)
            {
                Debug.LogWarning("[MainController] 먼저 경로를 생성해주세요.");
                // 경로가 없으면 생성 후, 사용자가 다시 버튼을 누르도록 유도
                GenerateAndVisualizePaths();
                return;
            }

#if false //
    // UI에서 면 번호(1부터 시작)를 가져옴
    if (!int.TryParse(inputSelectedFaceIndex.text, out int faceIndex) || faceIndex < 1)
    {
        Debug.LogError($"[MainController] 유효하지 않은 면 번호입니다: {inputSelectedFaceIndex.text}");
        return;
    }
#endif

            // 면 번호에 해당하는 경로 찾기
            var pathToExecute = BA_PathDataManager.Instance.generatedPaths.FirstOrDefault(p => p.FaceIndex == faceIndex);

            if (pathToExecute != null)
            {
                // 1. poseNames 배열의 유효한 인덱스인지 확인합니다.
                //    (UI는 1부터 시작, 배열은 0부터 시작하므로 faceIndex - 1 로 접근)
                if (faceIndex - 1 < 0 || faceIndex - 1 >= poseNames.Length)
                {
                    Debug.LogError($"[MainController] 면 번호 {faceIndex}에 해당하는 poseName이 배열에 없습니다. poseNames 배열 길이를 확인해주세요.");
                    return;
                }

                // 2. poseNames 배열에서 올바른 포즈 이름을 가져옵니다.
                string poseName = poseNames[faceIndex - 1];
                // Debug.Log($"[MainController] 🎯 면 {faceIndex} 실행 - 적용된 자세: '{poseName}' (poseNames[{faceIndex - 1}])");

                // 3. 하드코딩된 이름 대신, 배열에서 가져온 poseName 변수를 사용하여 코루틴을 시작합니다.
                StartCoroutine(ExecuteSingleMotionCoroutine(pathToExecute, poseName, this.detectionLayer));

                // =========================================================================
            }
            else
            {
                Debug.LogError($"[MainController] 면 번호 {faceIndex}에 해당하는 경로를 찾을 수 없습니다.");
            }
        }

        [ContextMenu("3. 모든 면 순차 실행")]
        public void ExecuteAllMotionsSequentially()
        {
            if (motionExecutor.IsExecuting)
            {
                Debug.LogWarning("[MainController] 현재 다른 모션이 실행 중입니다.");
                return;
            }
            if (BA_PathDataManager.Instance.generatedPaths == null || BA_PathDataManager.Instance.generatedPaths.Count == 0)
            {
                Debug.LogWarning("[MainController] 먼저 경로를 생성해주세요.");
                return;
            }

            int numberOfPaths = BA_PathDataManager.Instance.generatedPaths.Count;
            faceOrder = new int[numberOfPaths];
            for (int i = 0; i < numberOfPaths; i++)
            {
                faceOrder[i] = BA_PathDataManager.Instance.generatedPaths[i].FaceIndex;
            }

            StartCoroutine(SequentialMotionCoroutine()); // 수정된 코루틴 호출
        }

        [ContextMenu("4. 시각화 요소 모두 제거")]
        public void ClearAllGeneratedDataAndVisuals()
        {
#if false // @#####
            visualizer.ClearAllVisuals();
#endif
            if (BA_PathDataManager.Instance.generatedPaths != null)
            {
                BA_PathDataManager.Instance.generatedPaths.Clear();
            }
            Debug.Log("[MainController] 모든 경로 데이터와 시각화 요소를 제거했습니다.");
        }
        /// <summary>
        /// 현재 씬을 다시 로드하여 처음부터 모든 프로세스를 다시 시작합니다.
        /// </summary>
        [ContextMenu("5. 처음부터 다시 시작 (Restart Scene)")]
        public void RestartScene()
        {
            // 현재 활성화된 씬의 이름을 가져옵니다.
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // 현재 씬을 다시 로드합니다.
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);

            Debug.Log($"[MainController] 씬을 다시 시작합니다: {currentSceneName}");
        }

        [ContextMenu("Test Weaving Path Generation for W1")]
        public void TestWeavingPathGeneration()
        {
            if (planeAndNV == null || pathGenerator == null)
            {
                Debug.LogError("테스트를 실행하려면 Plane_and_NV와 TCP 오브젝트가 인스펙터에 연결되어 있어야 합니다.");
                return;
            }
            if (!planeAndNV.JsonLoader.IsDataLoaded)
            {
                Debug.LogWarning("Plane_and_NV 데이터가 아직 로드되지 않았습니다. 먼저 로드합니다.");
                planeAndNV.LoadDataFromJSONInEditor();
            }

            if (planeAndNV.JsonLoader.W_Faces.TryGetValue("W1", out Cube w1Face))
            {
                var testConfig = new PathGenerationConfig { NormalOffset = 0.6f };

                Debug.Log("--- 'W1' 면 위빙 경로 생성 및 시각화 테스트 시작 ---");

                // 1. 경로 데이터 생성
                BA_WeavingPath w1Path = pathGenerator.GenerateWeavingPath(w1Face, testConfig);

                // 2. 생성된 경로 데이터를 시각화 모듈에 전달
                if (w1Path != null)
                {
#if false // @#####
                    visualizer.ClearAllVisuals(); // 기존 시각화 요소 삭제
                    visualizer.VisualizePath(w1Path);
#endif
                }
                else
                {
                    Debug.LogError("W1 경로 생성에 실패했습니다.");
                }

                Debug.Log("--- 테스트 완료 ---");
            }
            else
            {
                Debug.LogError("'W1' 면 데이터를 찾을 수 없습니다.");
            }
        }
        // #endregion

        // ==================================================================
        //                         코루틴 및 Private 메서드
        // ==================================================================
        // #region 코루틴 및 내부 함수
        /// <summary>
        /// [수정됨] 한 면의 실행이 끝날 때마다 해당 경로 데이터를 리스트에서 삭제하는 코루틴.
        /// </summary>
        /// <summary>
        /// [V2 수정] 모든 면의 모션을 순차적으로 실행하고, 모든 작업이 끝난 후에 XML을 생성하고 전송합니다.
        /// </summary>
        private IEnumerator SequentialMotionCoroutine()
        {
            Debug.Log("[MainController] 전체 모션 데이터 생성 및 순차 실행을 시작합니다.");

            // ▼▼▼ [Step 3] 코루틴의 전체 로직을 아래와 같이 수정합니다. ▼▼▼

            // 1. 시작 전, 이전 데이터를 모두 초기화합니다.
            xmlRecorder.ClearAll();

            var pathsToExecute = new List<BA_MotionPath>(BA_PathDataManager.Instance.generatedPaths);

            // 2. 모든 경로(면)에 대해 모션을 순차적으로 실행하여 Pose 데이터를 누적합니다.
            foreach (var path in pathsToExecute)
            {
#if false // @#####
                visualizer.ClearAllVisuals();
                visualizer.VisualizePath(path);
#endif

                // 해당 면의 Pose 이름을 가져옵니다.
                string poseName = (path.FaceIndex - 1 < poseNames.Length) ? poseNames[path.FaceIndex - 1] : "default_pose";
                Debug.Log($"--- 면 {path.FaceIndex} (자세: {poseName}) 작업 시작 ---");

                // 각 면이 시작될 때마다 XmlPathRecorder의 Pose ID를 1로 리셋합니다.
                xmlRecorder.StartNewPathSegment();

                // 모션 실행 (내부적으로 xmlRecorder.RecordCurrentPose가 호출됩니다)
                motionExecutor.StartMotion(path, poseName);
                yield return new WaitUntil(() => !motionExecutor.IsExecuting);

                Debug.Log($"--- 면 {path.FaceIndex} 작업 완료 ---");
                yield return new WaitForSeconds(0.5f); // 면과 면 사이의 짧은 대기
            }

            // 3. 모든 모션 실행이 끝난 후, 누적된 모든 Pose 데이터를 하나의 큰 XML 문자열로 변환합니다.
            Debug.Log($"[MainController] 모든 모션 시뮬레이션 완료. 총 {xmlRecorder.AllPoses.Count}개의 Pose가 기록되었습니다.");
            string finalXmlString = makeXmlInstance.ConvertPoseListToXmlString(xmlRecorder.AllPoses);

            // 4. 생성된 XML 문자열을 XMLSender로 보내 분할 전송을 요청합니다.
            if (!string.IsNullOrEmpty(finalXmlString))
            {
                if (xmlSender != null)
                {
                    Debug.Log("[MainController] XMLSender에게 분할 전송을 요청합니다.");
                    // 이 메서드는 XMLSender에 새로 구현해야 합니다.
                    xmlSender.SendLargeXmlInChunks(finalXmlString, "motion");
                }
                else
                {
                    Debug.LogError("[MainController] XMLSender가 인스펙터에 할당되지 않아 전송할 수 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("[MainController] 생성된 XML 데이터가 없습니다. 전송을 건너뜁니다.");
            }

#if false // @#####
            visualizer.ClearAllVisuals();
#endif
            BA_PathDataManager.Instance.generatedPaths.Clear();
            Debug.Log("[MainController] 모든 프로세스가 완료되었습니다.");
            // ▲▲▲ [수정 완료] ▲▲▲
        }
        private IEnumerator ExecuteSingleMotionCoroutine(BA_MotionPath path, string poseName, LayerMask layerMask)
        {
            // 1. 기록 시작 전, 이전 데이터를 모두 초기화합니다.
            xmlRecorder.ClearAll();
            xmlRecorder.StartNewPathSegment();

            // 2. 로봇 모션을 실행하여 Pose 데이터를 xmlRecorder에 누적합니다.
            motionExecutor.StartMotion(path, poseName);
            yield return new WaitUntil(() => !motionExecutor.IsExecuting);

            // ▼▼▼ [핵심 수정] 모션 완료 후, 아래의 XML 생성 및 전송/저장 로직을 추가합니다. ▼▼▼

            // 3. 누적된 모든 Pose 데이터를 하나의 큰 XML 문자열로 변환합니다.
            Debug.Log("[MainController] 단일 면 모션 완료. XML 데이터 생성을 시작합니다.");
            string finalXmlString = makeXmlInstance.ConvertPoseListToXmlString(xmlRecorder.AllPoses);

            // 4. 생성된 XML 문자열을 XMLSender로 보내서 처리(저장 또는 전송)하도록 요청합니다.
            if (!string.IsNullOrEmpty(finalXmlString))
            {
                if (xmlSender != null)
                {
                    Debug.Log("[MainController] XMLSender에게 데이터 처리를 요청합니다.");
                    // SendLargeXmlInChunks 메서드가 에디터/WebGL 환경을 자동으로 감지하여 처리합니다.
                    xmlSender.SendLargeXmlInChunks(finalXmlString, $"motion_face_{path.FaceIndex}");
                }
                else
                {
                    Debug.LogError("[MainController] XMLSender가 인스펙터에 할당되지 않아 저장/전송할 수 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning("[MainController] 생성된 XML 데이터가 없습니다. 저장/전송을 건너뜁니다.");
            }
            // ▲▲▲ [수정 완료] ▲▲▲
        }


        private Vector3 GetRobotInitialPosition()
        {
            Vector3 robotInit = Vector3.zero;
            int count = 0;
            if (bioIK == null) return robotInit;

            foreach (var seg in bioIK.Segments)
            {
                if (seg.Joint == null) continue;
                string jointName = seg.Joint.gameObject.name;
                if (jointName == "x" || jointName == "y" || jointName == "z")
                {
                    robotInit += seg.Joint.transform.position;
                    count++;
                }
            }
            return (count > 0) ? robotInit / count : Vector3.zero;
        }

        private void UpdateVerticalStepFromUI()
        {
            float currentVal = manualMotionVerticalValue;
            if (Mathf.Abs(currentVal - previousMotionVerticalValue) > 0.001f)
            {
                verticalStep = currentVal / 100f;
                previousMotionVerticalValue = currentVal;
            }
        }


        private void UpdateNormalOffsetFromUI()
        {
            float currentVal = manualMotionBPSValue;
            if (Mathf.Abs(currentVal - previousMotionBPSValue) > 0.001f)
            {
                zigzagNormalOffset = currentVal / 100f;
                previousMotionBPSValue = currentVal;
            }
        }

        // #endregion
    }
}
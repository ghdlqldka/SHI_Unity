// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using BioIK;
// using _SHI_BA;
// using System.Linq; // Added to use LINQ methods like Where() and Take()

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// /// <summary>
// /// Executes a motion path along specific edges (1-4), including sophisticated obstacle avoidance.
// /// </summary>
// public class StiffnerEdgeMotion : MonoBehaviour
// {
//     [Header("Core Component References")]
//     public Plane_and_NV planeAndNvReference;
//     public BA_BioIK _bioIK;
//     public GameObject TCP;
//     public GameObject targetObj;
//     public Gauntry_or_6Joint_or_all jointController;
//     public RobotStartPose robotStartPose;
//     public Make_XML makeXmlInstance;
//     public Toggle_Obstacle_Avoiding toggleObstacleAvoiding;
//     public XMLSender xmlSender;

//     [Header("Collision Detection Components")]
//     public RaycastVisualizer raycastVisualizer;

//     [Header("Motion Execution Settings")]
//     public float yOffset = 0.6f;
//     [Header("Custom Rotation Object")]
//     [Tooltip("Assign the object to be rotated on the Z-axis (e.g., 'weaving2').")]
//     public GameObject weaving2;
    
//     // --- Internal Control Modules ---
//     private MotionPathGenerator pathGenerator;
//     private RobotMotionExecutor motionExecutor;
//     private XmlPathRecorder xmlRecorder;

//     // --- Generated Path Data ---
//     private List<Vector3> generatedPositions = new List<Vector3>();
//     private List<Quaternion> generatedRotations = new List<Quaternion>();

//     // --- State Management Variables ---
//     public bool IsExecuting { get; private set; }
//     private bool isCurrentlyAvoiding = false;
//     private Coroutine currentMotionCoroutine = null;
//     private int currentTargetIndex = 0;
//     private float currentWeavingAngle = 60f;
//     private Dictionary<GameObject, int> processedParentObstacles = new Dictionary<GameObject, int>();
//     private HashSet<Vector3> startPoints = new HashSet<Vector3>();
//     private HashSet<Vector3> endPoints = new HashSet<Vector3>();
//     private List<Vector3> startPointSafeZones = new List<Vector3>();
//     private List<Vector3> endPointSafeZones = new List<Vector3>();
//     private float currentAngleBps = 60f;
//     private List<float> segmentHalfAngles = new List<float>();
//     private List<int> segmentStartIndices = new List<int>();
//     private List<bool> isSegmentForward = new List<bool>();
//     private Vector3 lastProcessedPoint = Vector3.positiveInfinity;

//     private float spd = 400.0f;
//     private float acc = 200.0f;
//     private float onoff = 1.0f;
    
//     void Awake()
//     {
//         if (TCP != null) { pathGenerator = new MotionPathGenerator(TCP.transform); }
//         else { Debug.LogError("[StiffnerEdgeMotion] TCP object is not assigned. Path generator cannot be initialized."); }

//         xmlRecorder = new XmlPathRecorder();
//         motionExecutor = new RobotMotionExecutor(this, _bioIK, targetObj, TCP.transform, jointController, robotStartPose, xmlRecorder, toggleObstacleAvoiding);

//         var raycastVisualizerComponent = FindFirstObjectByType<RaycastVisualizer>();
//         if (raycastVisualizerComponent != null)
//         {
//             raycastVisualizerComponent.StiffnerEdgeMotion = this;
//         }
//         else
//         {
//             Debug.LogError("[StiffnerEdgeMotion] RaycastVisualizer is not assigned in the Inspector! Please drag and drop it to establish the reference.", this.gameObject);
//         }
//         Debug.Log("[StiffnerEdgeMotion] Modules are ready.");
//     }

//     [ContextMenu("Execute Edge 1-4 Weaving Motion")]
//     public void StartMotionExecution()
//     {
//         if (planeAndNvReference == null || !planeAndNvReference.IsDataLoaded)
//         {
//             Debug.LogError("[Motion Start Failed] Data from Plane_and_NV has not been loaded yet. Please try again shortly.");
//             return;
//         }

//         if (IsExecuting)
//         {
//             Debug.LogWarning("Another motion is already in progress.");
//             return;
//         }
//         currentMotionCoroutine = StartCoroutine(ExecuteCustomEdgeMotion());
//     }

//     /// <summary>
//     /// The main coroutine that executes the motion along the custom edge path. Can be resumed from a specific index.
//     /// </summary>
//     private IEnumerator ExecuteCustomEdgeMotion(int startIndex = 0, bool resumeAfterAvoidance = false)
//     {
//         IsExecuting = true;

//         if (startIndex == 0)
//         {
//             if (raycastVisualizer != null)
//             {
//                 raycastVisualizer.enabled = false;
//                 Debug.Log("<color=yellow>[StiffnerEdgeMotion] Initialization phase. Disabling Raycast collision detection.</color>");
//             }
//             GenerateCustomPath();
//             if (generatedPositions.Count == 0)
//             {
//                 Debug.LogError("Motion stopped because no path was generated.");
//                 IsExecuting = false;
//                 yield break;
//             }

//             xmlRecorder.StartNewPathSegment();

//             Debug.Log("Step 1: Setting initial pose ('ready_top2')");
//             jointController.EnableOnlyA1toA6();
//             _bioIK.autoIK = false;
//             yield return StartCoroutine(robotStartPose.PosingSequenceCoroutine("ready_top2", Vector3.down));
//             _bioIK.autoIK = true;

//             Debug.Log("Step 2: Setting Pan/Tilt angles directly");
//             motionExecutor.RotateUpboxDirectly(90f);
//             motionExecutor.RotatedownDirectly(60f);

//             Debug.Log("Step 3: Moving Gantry to the path's starting point");
//             jointController.EnableOnlyGauntry();
//             yield return new WaitForSeconds(0.5f);

//             targetObj.transform.position = generatedPositions[0];
//             targetObj.transform.rotation = generatedRotations[0];
//             yield return StartCoroutine(motionExecutor.WaitUntilStagnationOrTimeout(150.0f));

//             Debug.Log("Step 4: Aligning TCP");
//             jointController.EnableOnlyA1toA6();
//             yield return StartCoroutine(motionExecutor.WaitUntilRotationAligned(5.0f, 3.0f));
//         }

//         jointController.EnableOnlyGauntry();
//         Debug.Log($"<color=lime>Step 5: Starting main path execution (Start Index: {startIndex})</color>");

//         for (int i = startIndex; i < generatedPositions.Count; i++)
//         {
//             currentTargetIndex = i;

//             if (weaving2 != null)
//             {
//                 int currentSegmentIndex = -1;
//                 for (int k = segmentStartIndices.Count - 1; k >= 0; k--)
//                 {
//                     if (i >= segmentStartIndices[k])
//                     {
//                         currentSegmentIndex = k;
//                         break;
//                     }
//                 }

//                 if (currentSegmentIndex != -1)
//                 {
//                     float angleForSegment = segmentHalfAngles[currentSegmentIndex];
//                     bool isForward = isSegmentForward[currentSegmentIndex];
//                     float rotationZ = isForward ? angleForSegment : -angleForSegment;
//                     weaving2.transform.localEulerAngles = new Vector3(
//                         weaving2.transform.localEulerAngles.x,
//                         weaving2.transform.localEulerAngles.y,
//                         rotationZ
//                     );
//                 }
//             }

//             if (i == startIndex + 1)
//             {
//                 if (raycastVisualizer != null && !raycastVisualizer.enabled)
//                 {
//                     raycastVisualizer.enabled = true;
//                     Debug.Log("<color=yellow>[StiffnerEdgeMotion] Arrived at the first point. Enabling Raycast for the rest of the path.</color>");
//                 }
//             }

//             targetObj.transform.SetPositionAndRotation(generatedPositions[i], generatedRotations[i]);
//             yield return StartCoroutine(motionExecutor.WaitUntilStagnationOrTimeout(150.0f));

//             xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2 != null ? weaving2.transform.localEulerAngles.z : 0, spd, acc, onoff);
//             Debug.Log($"Path point {i + 1}/{generatedPositions.Count} reached and XML recorded.");

//             Vector3 arrivedPoint = generatedPositions[i];
//             bool isStartPoint = startPointSafeZones.Contains(arrivedPoint);
//             bool isEndPoint = endPointSafeZones.Contains(arrivedPoint);

//             if (isStartPoint || isEndPoint)
//             {
//                 if (arrivedPoint != lastProcessedPoint)
//                 {
//                     if (raycastVisualizer != null) raycastVisualizer.enabled = false;
//                     Debug.Log($"<color=lime>--- Special Point Reached: {(isStartPoint ? "Start" : "End")} ---</color>");
                    
//                     processedParentObstacles.Clear();
//                     isCurrentlyAvoiding = false;
                    
//                     float newAngleBps = isStartPoint ? 60f : 120f;
//                     currentAngleBps = newAngleBps; // Update the global angle state
                    
//                     Quaternion newTargetRotation = GetRotationForAngle(newAngleBps);
//                     for (int j = currentTargetIndex; j < generatedRotations.Count; j++)
//                     {
//                         generatedRotations[j] = newTargetRotation;
//                     }

//                     jointController.ReinitializeBioIK();
//                     yield return new WaitForSeconds(0.1f);
//                     jointController.EnableOnlyA5A6();
//                     targetObj.transform.rotation = newTargetRotation;
//                     yield return StartCoroutine(motionExecutor.WaitUntilRotationAligned(5.0f, 3.0f));
//                     jointController.EnableOnlyGauntry();
                    
//                     lastProcessedPoint = arrivedPoint;
//                     if (raycastVisualizer != null) raycastVisualizer.enabled = true;
//                 }
//             }
//             else
//             {
//                 lastProcessedPoint = Vector3.positiveInfinity;
//             }
//             yield return null;
//         }

//         xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, 0, spd, acc, onoff);
//         string finalXmlString = makeXmlInstance.ConvertPoseListToXmlString(xmlRecorder.AllPoses);

//         if (!string.IsNullOrEmpty(finalXmlString))
//         {
//             if (xmlSender != null)
//             {
//                 xmlSender.SendLargeXmlInChunks(finalXmlString, "edge_1_4_motion");
//             }
//             else
//             {
//                 Debug.LogError("[XML Save Failed] XMLSender is not assigned.");
//             }
//         }
        
//         Debug.Log("<color=green>--- All path motions completed ---</color>");
//         IsExecuting = false;
//         currentMotionCoroutine = null;
//     }

//     private Quaternion GetRotationForAngle(float angle)
//     {
//         Vector3 faceNormal = Vector3.up;
//         Vector3 heightDir = Vector3.right;
//         return Quaternion.LookRotation(Quaternion.AngleAxis(angle, heightDir) * faceNormal, heightDir);
//     }
    
//     /// <summary>
//     /// Generates the motion path using only edges 1-4 from the Plane_and_NV data.
//     /// </summary>
//     private void GenerateCustomPath()
//     {
//         // Clear previous path data
//         generatedPositions.Clear();
//         generatedRotations.Clear();
//         startPoints.Clear();
//         endPoints.Clear();
//         startPointSafeZones.Clear();
//         endPointSafeZones.Clear();
//         segmentHalfAngles.Clear();
//         segmentStartIndices.Clear();
//         isSegmentForward.Clear();

//         // **MODIFIED**: Filter for edges 1 to 4 only.
//         var edgesToProcess = planeAndNvReference.Edge_List
//             .Where(e => e.Name == "R7_Edge_1" || e.Name == "R7_Edge_2" || e.Name == "R7_Edge_3" || e.Name == "R7_Edge_4")
//             .OrderBy(e => e.Name) // Ensure consistent order
//             .ToList();
//         var r7FaceData = planeAndNvReference.R_Faces.FirstOrDefault(f => f.Name == "R7");
//         if (r7FaceData.Name == null)
//         {
//             Debug.LogError("[GenerateCustomPath] R7 면 데이터를 찾을 수 없습니다.");
//             return;
//         }
//         var intersectingEdges = planeAndNvReference.Edge_List
//             .Where(e => e.Name.CompareTo("R7_Edge_5") >= 0 && e.Name.CompareTo("R7_Edge_8") <= 0)
//             .ToList();
//         Plane r7Plane = new Plane(r7FaceData.R1, r7FaceData.R2, r7FaceData.R4);
//         var projectedEdges = new List<Plane_and_NV.Edge>();

//         foreach (var edge in intersectingEdges)
//         {
//             Vector3 projectedStart = r7Plane.ClosestPointOnPlane(edge.Start);
//             Vector3 projectedEnd = r7Plane.ClosestPointOnPlane(edge.End);
//             projectedEdges.Add(new Plane_and_NV.Edge { Name = edge.Name + "_Projected", Start = projectedStart, End = projectedEnd });
//         }
//         if (edgesToProcess.Count < 2)
//         {
//             Debug.LogError("Not enough edge data (1-4) to generate a path.");
//             return;
//         }
//         else
//         {
//             Debug.Log($"Loaded {edgesToProcess.Count} edges for processing (1-4).");
//         }

//         Quaternion targetRotation = GetRotationForAngle(60f);

//         // **MODIFIED**: Loop through the filtered list of edges.
//         for (int i = 0; i < edgesToProcess.Count - 1; i++)
//         {
//             var currentEdge = edgesToProcess[i];
//             var nextEdge = edgesToProcess[i + 1];
            
//             Vector3 intermediateStart = (currentEdge.Start + nextEdge.Start) / 2f + Vector3.up * yOffset;
//             float angle = Vector3.Angle(currentEdge.Start - intermediateStart, nextEdge.Start - intermediateStart);
//             float halfAngle = angle / 2f;
//             Vector3 startPoint = (currentEdge.Start + nextEdge.Start) / 2f + Vector3.up * yOffset;
//             Vector3 endPoint = (currentEdge.End + nextEdge.End) / 2f + Vector3.up * yOffset;
            
//             startPoints.Add(startPoint);
//             endPoints.Add(endPoint);

//             int numberOfWaypoints = 10;

//             // --- 1. Forward path generation (start -> end) ---
//             List<Vector3> forwardPath = new List<Vector3>();
//             Vector3 forwardDirection = (endPoint - startPoint).normalized;
//             Vector3 safeStartPoint = startPoint - forwardDirection * 0.75f;
//             Vector3 safeEndPoint = endPoint + forwardDirection * 0.75f;
//             startPointSafeZones.Add(safeStartPoint);
//             endPointSafeZones.Add(safeEndPoint);

//             forwardPath.Add(safeStartPoint);
//             forwardPath.Add(startPoint);
//             for (int j = 1; j <= numberOfWaypoints; j++)
//             {
//                 forwardPath.Add(Vector3.Lerp(startPoint, endPoint, (float)j / (numberOfWaypoints + 1)));
//             }
//             forwardPath.Add(endPoint);
//             forwardPath.Add(safeEndPoint);

//             segmentStartIndices.Add(generatedPositions.Count);
//             segmentHalfAngles.Add(halfAngle);
//             isSegmentForward.Add(true);
//             generatedPositions.AddRange(forwardPath);
//             for (int k = 0; k < forwardPath.Count; k++) generatedRotations.Add(targetRotation);

//             // --- 2. Return path generation (end -> start) ---
//             List<Vector3> returnPath = new List<Vector3>();
//             Vector3 returnDirection = (startPoint - endPoint).normalized;
//             returnPath.Add(endPoint);
//             for (int j = 1; j <= numberOfWaypoints; j++)
//             {
//                 returnPath.Add(Vector3.Lerp(endPoint, startPoint, (float)j / (numberOfWaypoints + 1)));
//             }
//             returnPath.Add(startPoint);
//             returnPath.Add(startPoint + returnDirection * 0.5f);
            
//             segmentStartIndices.Add(generatedPositions.Count);
//             segmentHalfAngles.Add(halfAngle);
//             isSegmentForward.Add(false);
//             generatedPositions.AddRange(returnPath);
//             for (int k = 0; k < returnPath.Count; k++) generatedRotations.Add(targetRotation);
//         }

//         Debug.Log($"Path generation complete. Total points: {generatedPositions.Count}");
//     }

//     #region Obstacle Avoidance

//     /// <summary>
//     /// Receives a collision signal from the RaycastVisualizer.
//     /// </summary>
//     public void OnObstacleDetected(GameObject hitObject)
//     {
//         if (isCurrentlyAvoiding || !IsExecuting) return;

//         ObstacleIdentifier identifier = hitObject.GetComponent<ObstacleIdentifier>();
//         if (identifier == null) return;

//         GameObject parentObstacle;
//         if (identifier.type == ObstacleIdentifier.ObstacleType.Child)
//         {
//             parentObstacle = identifier.parentObstacle;
//             if (parentObstacle == null) return;
//         }
//         else
//         {
//             parentObstacle = hitObject;
//         }

//         int currentTriggerCount = processedParentObstacles.GetValueOrDefault(parentObstacle, 0);

//         if (identifier.type == ObstacleIdentifier.ObstacleType.Child)
//         {
//             if (currentTriggerCount == 2 && processedParentObstacles.GetValueOrDefault(parentObstacle, 0) != 3)
//             {
//                 PerformAngleSwitchAndPathModification(3);
//                 processedParentObstacles[parentObstacle] = 3;
//             }
//         }
//         else
//         {
//             if (currentTriggerCount == 1)
//             {
//                 xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2.transform.localEulerAngles.z, spd, acc, onoff);
//                 processedParentObstacles[parentObstacle] = 2;
//             }
//             else if (currentTriggerCount == 0)
//             {
//                 PerformAngleSwitchAndPathModification(1);
//                 processedParentObstacles[parentObstacle] = 1;
//             }
//         }
//     }

//     /// <summary>
//     /// A wrapper function to stop the current motion and start the collision response coroutine.
//     /// </summary>
//     private void PerformAngleSwitchAndPathModification(int triggerStage)
//     {
//         isCurrentlyAvoiding = true;
//         if (currentMotionCoroutine != null) StopCoroutine(currentMotionCoroutine);
//         currentMotionCoroutine = StartCoroutine(CollisionResponseSequence(triggerStage));
//     }

//     /// <summary>
//     /// The avoidance coroutine, adapted from RobotMotionExecutor's logic.
//     /// </summary>
//     private IEnumerator CollisionResponseSequence(int triggerStage)
//     {
//         xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2.transform.localEulerAngles.z, spd, acc, onoff);

//         if (triggerStage == 1 || triggerStage == 3)
//         {
//             ToggleAndApplyWeavingAngle();
//             jointController.EnableOnlyA5A6();
//             targetObj.transform.rotation = generatedRotations[currentTargetIndex];
//             yield return StartCoroutine(motionExecutor.WaitUntilRotationAligned(5.0f, 2.0f));

//             if (triggerStage == 3)
//             {
//                 xmlRecorder.RecordCurrentPose_weaving_fixed(this.makeXmlInstance, weaving2.transform.localEulerAngles.z, spd, acc, onoff);
//             }
//         }

//         isCurrentlyAvoiding = false;
//         jointController.EnableOnlyGauntry();
//         currentMotionCoroutine = StartCoroutine(ExecuteCustomEdgeMotion(currentTargetIndex, true));
//     }

//     /// <summary>
//     /// Toggles the weaving angle and applies the new rotation to all future points in the path.
//     /// </summary>
//     private void ToggleAndApplyWeavingAngle()
//     {
//         currentWeavingAngle = (currentWeavingAngle == 60f) ? 120f : 60f;
//         Quaternion newRotation = GetRotationForAngle(currentWeavingAngle);
//         for (int i = currentTargetIndex; i < generatedRotations.Count; i++)
//         {
//             generatedRotations[i] = newRotation;
//         }
//     }
//     #endregion

//     void OnDrawGizmos()
//     {
//         if (planeAndNvReference == null || !planeAndNvReference.IsDataLoaded) return;
        
//         var edgesToDraw = planeAndNvReference.Edge_List.Take(4).ToList();
//         if (edgesToDraw.Count < 2) return;

//         for (int i = 0; i < edgesToDraw.Count - 1; i++)
//         {
//             var currentEdge = edgesToDraw[i];
//             var nextEdge = edgesToDraw[i + 1];
//             Vector3 intermediateStart = (currentEdge.Start + nextEdge.Start) / 2f + Vector3.up * yOffset;
            
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawLine(intermediateStart, currentEdge.Start);
//             Gizmos.DrawLine(intermediateStart, nextEdge.Start);
//         }
//     }
// }
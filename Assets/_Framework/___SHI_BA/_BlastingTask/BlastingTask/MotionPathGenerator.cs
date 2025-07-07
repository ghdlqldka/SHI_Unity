//// // using UnityEngine;
//// // using System.Collections.Generic;
//// // using System.Linq;
//// // using System;
//// // using System.Text;

//// // public enum PathType
//// // {
//// //     Zigzag, // GenerateZigzagPathForFace 메서드로 생성된 경로
//// //     Snake,  // GeneratePath_TrueSnakeMethod 메서드로 생성된 경로
//// //     Unknown // 기타 또는 알 수 없는 경로
//// // }
//// // public class MotionPathGenerator
//// // {
//// //     public struct PathGenerationConfig {
//// //         public int NumHorizontalPoints;
//// //         public float VerticalStep;
//// //         public float NormalOffset;
//// //         public float ObstacleOffset;
//// //         public float ObstacleScaleFactor;
//// //     }
//// //     public void ClearPathData()
//// //     {
//// //         faceNormals.Clear();
//// //         faceWidthDirs.Clear();
//// //         faceHeightDirs.Clear();
//// //     }
//// //     public class MotionPath
//// //     {
//// //         public PathType Type { get; }
//// //         public float NormalOffset { get; }
//// //         public int FaceIndex { get; }
//// //         public List<Vector3> Positions { get; }
//// //         public List<Quaternion> Rotations { get; }
//// //         public Vector3 FaceNormal { get; }
//// //         public Vector3 StartPoint { get; }
//// //         public Vector3 EndPoint { get; }
//// //         public Vector3 WidthDir { get; }
//// //         public Vector3 HeightDir { get; }
//// //         public MotionPath(int faceIndex, PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir)
//// //         {
//// //             FaceIndex = faceIndex;
//// //             Type = type;
//// //             NormalOffset = normalOffset;
//// //             Positions = positions;
//// //             Rotations = rotations;
//// //             FaceNormal = faceNormal;
//// //             StartPoint = startPoint;
//// //             EndPoint = endPoint;
//// //             WidthDir = widthDir;
//// //             HeightDir = heightDir;
//// //         }
//// //     }
//// //     public class MotionPath_Weaving : MotionPath
//// //     {
//// //         public float WeavingAngle { get; }

//// //         public MotionPath_Weaving(int faceIndex, PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir, float weavingAngle)
//// //             : base(faceIndex, type, normalOffset, positions, rotations, faceNormal, startPoint, endPoint, widthDir, heightDir)
//// //         {
//// //             WeavingAngle = weavingAngle;
//// //         }
//// //     }

//// //     private List<Vector3> faceNormals = new List<Vector3>();
//// //     private List<Vector3> faceWidthDirs = new List<Vector3>();
//// //     private List<Vector3> faceHeightDirs = new List<Vector3>();
//// //     private Transform tcpTransform;
//// //     public MotionPathGenerator(Transform tcpTransform)
//// //     {
//// //         if (tcpTransform == null)
//// //         {
//// //             Debug.LogError("[MotionPathGenerator] 생성자에 유효한 TCP Transform이 전달되지 않았습니다.");
//// //         }
//// //         this.tcpTransform = tcpTransform;
//// //     }


//// //     public List<MotionPath> GenerateAllPaths(Plane_and_NV.Cube[] cubes, PathGenerationConfig config, Vector3 robotInitPosition, List<Collider> obstacles, int faceIndexOffset = 0)
//// //     {
//// //         return ExecuteWithScaledObstacles(obstacles, config.ObstacleScaleFactor, () =>
//// //         {
//// //             // faceNormals.Clear();
//// //             // faceWidthDirs.Clear();
//// //             // faceHeightDirs.Clear();

//// //             var allPaths = new List<MotionPath>();
//// //             if (cubes == null || cubes.Length == 0) return allPaths;

//// //             // foreach (var cube in cubes)
//// //             // {
//// //             //     faceNormals.Add(-Vector3.Cross(cube.R2 - cube.R1, cube.R4 - cube.R1).normalized);
//// //             // }

//// //             Vector3 previousActualEndPoint = robotInitPosition;

//// //             for (int i = 0; i < cubes.Length; i++)
//// //             {
//// //                 var currentCube = cubes[i];
//// //                 Vector3 currentFaceNormal = -Vector3.Cross(currentCube.R2 - currentCube.R1, currentCube.R4 - currentCube.R1).normalized;
//// //                 faceNormals.Add(currentFaceNormal); // 다른 곳에서 참조할 수 있도록 리스트에도 추가

//// //                 var currentPoints = new Vector3[] { currentCube.R1, currentCube.R2, currentCube.R3, currentCube.R4 };
//// //                 Vector3 startPoint;

//// //                 if (i == 0)
//// //                 {
//// //                     var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(robotInitPosition, p)).Take(2).ToList();
//// //                     startPoint = twoClosest.OrderBy(p => p.y).First();
//// //                 }
//// //                 else
//// //                 {
//// //                     // [수정] "공유 꼭짓점" 로직을 잠시 비활성화하고, 단순 최단거리로만 테스트
//// //                     startPoint = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).First();
//// //                 }

//// //                 // ========================= [3번 면 디버그 로그 추가] =========================
//// //                 // if (i == 2) // 3번 면(인덱스 2)의 시작점을 정할 때
//// //                 // {
//// //                 //     Debug.LogWarning("--- [3번 면 시작점 결정 로직 디버그] ---");
//// //                 //     Debug.Log($"[3번 면] 이전 면(2번)의 실제 끝점 (기준점): {previousActualEndPoint.ToString("F4")}");
//// //                 //     Debug.Log($"[3번 면] 3번 면의 모든 꼭짓점: \nR1:{currentPoints[0].ToString("F4")} \nR2:{currentPoints[1].ToString("F4")} \nR3:{currentPoints[2].ToString("F4")} \nR4:{currentPoints[3].ToString("F4")}");

//// //                 //     // 각 꼭짓점과의 거리를 직접 출력
//// //                 //     Debug.Log($"[3번 면] 거리 계산 -> R1: {Vector3.Distance(previousActualEndPoint, currentPoints[0]):F4}, R2: {Vector3.Distance(previousActualEndPoint, currentPoints[1]):F4}, R3: {Vector3.Distance(previousActualEndPoint, currentPoints[2]):F4}, R4: {Vector3.Distance(previousActualEndPoint, currentPoints[3]):F4}");

//// //                 //     Debug.Log($"[3번 면] 최종 선택된 시작점: {startPoint.ToString("F4")}");
//// //                 //     Debug.LogWarning("------------------------------------");
//// //                 // }
//// //                 // =======================================================================

//// //                 Vector3 endPoint = currentPoints.OrderByDescending(p => Vector3.Distance(startPoint, p)).First();
//// //                 // === faceIndex에 따라 verticalStep 하드코딩 적용 ===
//// //                 var currentFaceConfig = config;
//// //                 // if (i >= 0 && i <= 4)
//// //                 //     currentFaceConfig.VerticalStep = 0.2f;
//// //                 // else if (i >= 5 && i <= 7)
//// //                 //     currentFaceConfig.VerticalStep = 0.4f;
//// //                 // else if (i == 8)
//// //                 //     currentFaceConfig.VerticalStep = 0.25f;
//// //                 // ==============================================
//// //                 // var currentFaceConfig = config;
//// //                 List<Collider> relevantObstacles = new List<Collider>();
//// //                 if (IsFaceIntersectingObstacle(currentCube, obstacles))
//// //                 {
//// //                     currentFaceConfig.NumHorizontalPoints = 30;
//// //                     relevantObstacles = obstacles;
//// //                 }

//// //                 MotionPath path;
//// //                 //  = GenerateZigzagPathForFace(currentCube, startPoint, endPoint, currentFaceConfig, i, relevantObstacles);
//// //                 if (i == 10)
//// //                 { 
//// //                     // Debug.LogWarning($"[Face {i + 1}] 최종 '실시간 U턴' 방식으로 경로를 생성합니다.");
//// //                     path = GeneratePath_TrueSnakeMethod(currentCube, startPoint, endPoint, currentFaceConfig, i + faceIndexOffset, relevantObstacles);
//// //                 }
//// //                 else
//// //                 {
//// //                     path = GenerateZigzagPathForFace(currentCube, startPoint, endPoint, currentFaceConfig, i + faceIndexOffset, relevantObstacles);
//// //                 }

//// //                 if (path != null && path.Positions.Count > 1)
//// //                 {
//// //                     allPaths.Add(path);
//// //                     previousActualEndPoint = path.EndPoint;
//// //                 }
//// //             }
//// //             return allPaths;
//// //         });
//// //     }

//// //     private MotionPath GenerateZigzagPathForFace(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, Vector3 faceNormal, List<Collider> obstacles)
//// //     {
//// //         // Vector3 faceNormal = faceNormals[faceListIndex];
//// //         var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
//// //         var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
//// //         if (adjacentPoints.Count != 2) return null;

//// //         Vector3 edge1 = adjacentPoints[0] - start;
//// //         Vector3 edge2 = adjacentPoints[1] - start;
//// //         Vector3 heightDir, widthDir;

//// //         if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.95f)
//// //         {
//// //             // Debug.Log($"[Face {faceListIndex + 1}] 수평면으로 판별됨. '가장 짧은 모서리'를 기준으로 세로 방향을 결정합니다.");

//// //             if (edge1.magnitude < edge2.magnitude)
//// //             {
//// //                 heightDir = edge1.normalized;
//// //             }
//// //             else
//// //             {
//// //                 heightDir = edge2.normalized;
//// //             }
//// //             // ===============================================================
//// //         }
//// //         else
//// //         {
//// //             if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
//// //             {
//// //                 heightDir = edge1.normalized;
//// //             }
//// //             else
//// //             {
//// //                 heightDir = edge2.normalized;
//// //             }
//// //         }

//// //         widthDir = Vector3.Cross(heightDir, faceNormal).normalized;

//// //         Vector3 otherEdge;
//// //         if (Vector3.Dot(heightDir, edge1.normalized) > 0.999f)
//// //         {
//// //             otherEdge = edge2;
//// //         }
//// //         else
//// //         {
//// //             otherEdge = edge1;
//// //         }

//// //         if (Vector3.Dot(widthDir, otherEdge) < 0)
//// //         {
//// //             widthDir = -widthDir;
//// //         }


//// //         if (Vector3.Dot(widthDir, otherEdge) < 0) widthDir = -widthDir;
//// //         float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
//// //         float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();

//// //         if (faceListIndex >= faceWidthDirs.Count) { faceWidthDirs.Add(widthDir); faceHeightDirs.Add(heightDir); }
//// //         else { faceWidthDirs[faceListIndex] = widthDir; faceHeightDirs[faceListIndex] = heightDir; }

//// //         List<Vector3> workPoints = new List<Vector3>();
//// //         Vector3 curr = start;
//// //         float movedHeight = 0f;
//// //         bool toRight = true;
//// //         while (movedHeight <= heightLen + 1e-4f)
//// //         {
//// //             for (int j = 0; j < config.NumHorizontalPoints; j++)
//// //             {
//// //                 float t = (config.NumHorizontalPoints > 1) ? (float)j / (config.NumHorizontalPoints - 1) : 0;
//// //                 workPoints.Add(curr + widthDir * widthLen * (toRight ? t : (1 - t)));
//// //             }
//// //             if (movedHeight >= heightLen) break;
//// //             movedHeight += config.VerticalStep;
//// //             if (movedHeight > heightLen) movedHeight = heightLen;
//// //             curr = start + heightDir * movedHeight;
//// //             toRight = !toRight;
//// //         }

//// //         // if (faceListIndex == 6)
//// //         // {
//// //         //     config.NormalOffset = 0.24f;
//// //         // }
//// //         // Debug.Log("오프셋");
//// //         // Debug.Log(faceListIndex + 1);
//// //         // Debug.Log(config.NormalOffset);
//// //         // Debug.Log("----");
//// //         List<Vector3> finalPath = workPoints.Select(p => p + faceNormal * config.NormalOffset).ToList();
//// //         if (finalPath.Count < 2) return null;

//// //         float safeDistance = (faceListIndex >= 0 && faceListIndex <= 4) ? 0.2f : 1.7f;
//// //         finalPath.Insert(0, finalPath[0] + (finalPath[0] - finalPath[1]).normalized * safeDistance);
//// //         finalPath.Add(finalPath.Last() + (finalPath.Last() - finalPath[finalPath.Count - 2]).normalized * safeDistance);

//// //         List<Quaternion> rotations = new List<Quaternion>();
//// //         int angle_bps = 60;
//// //         if (faceListIndex == 7) // 스티프너면 전용 bps(26cm)
//// //         {
//// //             angle_bps = 120;
//// //         }

//// //         Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
//// //         for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

//// //         return new MotionPath(faceListIndex + 1, PathType.Zigzag, config.NormalOffset, finalPath, rotations, faceNormal, start, finalPath[finalPath.Count - 2], widthDir, heightDir);
//// //     }

//// //     /// <summary>
//// //     /// [수정됨] W 계열 면에 대한 위빙 경로를 생성하고, 'MotionPath_Weaving' 타입으로 반환합니다.
//// //     /// </summary>
//// //     public MotionPath_Weaving GenerateWeavingPath(Plane_and_NV.Cube wFace, PathGenerationConfig config)
//// //     {
//// //         if (tcpTransform == null) { /* ... 오류 처리 ... */ return null; }
//// //         if (string.IsNullOrEmpty(wFace.Name) || !wFace.Name.StartsWith("W")) { /* ... 오류 처리 ... */ return null; }

//// //         Debug.Log($"<color=yellow>[MotionPathGenerator] '{wFace.Name}' 면에 대한 위빙 경로 생성을 시작합니다.</color>");

//// //         var allPoints = new List<Vector3> { wFace.R1, wFace.R2, wFace.R3, wFace.R4 };

//// //         // 1. 시작점과 끝점 결정
//// //         Vector3 startPointOnFace = allPoints.OrderBy(p => Vector3.Distance(tcpTransform.position, p)).First();
//// //         Vector3 endPointOnFace = allPoints.OrderByDescending(p => Vector3.Distance(startPointOnFace, p)).First();

//// //         // 2. 경로의 기준 방향 벡터(widthDir, heightDir) 계산
//// //         var adjacentPoints = allPoints.Where(p => p != startPointOnFace && p != endPointOnFace).ToList();
//// //         if (adjacentPoints.Count != 2) return null; // 유효성 검사

//// //         Vector3 edge1 = adjacentPoints[0] - startPointOnFace;
//// //         Vector3 edge2 = adjacentPoints[1] - startPointOnFace;
//// //         Vector3 heightDir, widthDir;

//// //         // 더 수직에 가까운 모서리를 heightDir로 설정
//// //         if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
//// //         {
//// //             heightDir = edge1.normalized;
//// //             widthDir = (endPointOnFace - adjacentPoints[1]).normalized;
//// //         }
//// //         else
//// //         {
//// //             heightDir = edge2.normalized;
//// //             widthDir = (endPointOnFace - adjacentPoints[0]).normalized;
//// //         }

//// //         // 3. 실제 경로점 계산 (중점 + 오프셋)
//// //         Vector3 pathStartPoint = ((startPointOnFace + adjacentPoints.OrderBy(p=>Vector3.Distance(startPointOnFace, p)).First()) / 2f) + wFace.normal * config.NormalOffset;
//// //         Vector3 pathEndPoint = ((endPointOnFace + adjacentPoints.OrderBy(p=>Vector3.Distance(endPointOnFace, p)).First())/2f) + wFace.normal * config.NormalOffset;

//// //         float calculatedWeavingAngle = CalculateWeavingAngleForFace(wFace, config.NormalOffset);

//// //         // 4. 경로 리스트 생성 및 안전지대 추가
//// //         List<Vector3> positions = new List<Vector3> { pathStartPoint, pathEndPoint };
//// //         float safeDistance = 0.5f;
//// //         if (positions.Count >= 2)
//// //         {
//// //             positions.Insert(0, positions[0] + (positions[0] - positions[1]).normalized * safeDistance);
//// //             positions.Add(positions.Last() + (positions.Last() - positions[positions.Count - 2]).normalized * safeDistance);
//// //         }

//// //         // 5. [수정] AngleAxis를 이용한 회전값 계산
//// //         float angle_bps = 60f; // 블라스팅 각도 (예시값, 필요시 config에서 받아오도록 수정 가능)
//// //         Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * wFace.normal, heightDir);

//// //         List<Quaternion> rotations = new List<Quaternion>();
//// //         for (int i = 0; i < positions.Count; i++)
//// //         {
//// //             rotations.Add(targetRotation);
//// //         }

//// //         int faceIndex;
//// //         try { faceIndex = int.Parse(wFace.Name.Substring(1)); }
//// //         catch { faceIndex = -1; }

//// //         var path = new MotionPath_Weaving(
//// //             faceIndex: faceIndex, 
//// //             type: PathType.Unknown, 
//// //             normalOffset: config.NormalOffset, 
//// //             positions: positions, 
//// //             rotations: rotations, 
//// //             faceNormal: wFace.normal, 
//// //             startPoint: pathStartPoint,
//// //             endPoint: pathEndPoint,
//// //             widthDir: widthDir,
//// //             heightDir: heightDir,
//// //             weavingAngle: calculatedWeavingAngle
//// //         );

//// //         Debug.Log($"[MotionPathGenerator] '{wFace.Name}' 경로 최종 생성 완료. (총 포인트: {path.Positions.Count}, 블라스팅 각도: {angle_bps}도)");

//// //         return path;
//// //     }

//// //        /// <summary>
//// //     /// [수정됨] 주어진 면과 옵셋을 기준으로 위빙 각도를 계산하고, 계산 과정을 시각화합니다.
//// //     /// </summary>
//// //     private float CalculateWeavingAngleForFace(Plane_and_NV.Cube face, float offset)
//// //     {
//// //         // 1. 삼각형의 첫 번째 꼭짓점(P1)은 R1으로 고정합니다.
//// //         Vector3 p1_arcStart = face.R1;

//// //         // 2. [수정] R2, R3, R4 중에서 R1과 가장 가까운 점을 찾아 두 번째 꼭짓점(P2)으로 설정합니다.
//// //         var candidatePoints = new Dictionary<string, Vector3>
//// //         {
//// //             { "R2", face.R2 },
//// //             { "R3", face.R3 },
//// //             { "R4", face.R4 }
//// //         };

//// //         string closestPointName = "";
//// //         Vector3 p2_arcEnd = Vector3.zero;
//// //         float minDistance = float.MaxValue;

//// //         foreach (var candidate in candidatePoints)
//// //         {
//// //             float distance = Vector3.Distance(p1_arcStart, candidate.Value);
//// //             if (distance < minDistance)
//// //             {
//// //                 minDistance = distance;
//// //                 p2_arcEnd = candidate.Value;
//// //                 closestPointName = candidate.Key;
//// //             }
//// //         }
//// //         // ------------------------------------------------------------------------------------

//// //         // 3. 삼각형의 세 번째 꼭짓점(P3, 원호의 중심)을 계산합니다.
//// //         Vector3 midPoint = (p1_arcStart + p2_arcEnd) / 2f;
//// //         Vector3 p3_arcCenter = midPoint + face.normal * offset;

//// //         // 4. P3에서 P1, P2를 향하는 두 변(벡터)을 계산합니다.
//// //         Vector3 vectorToStart = p1_arcStart - p3_arcCenter;
//// //         Vector3 vectorToEnd = p2_arcEnd - p3_arcCenter;

//// //         // 5. 두 변 사이의 각도(P3에서의 내각)를 계산합니다.
//// //         float centerAngle = Vector3.Angle(vectorToStart, vectorToEnd);

//// //         // 6. 최종 위빙 각도는 계산된 중심각의 절반입니다.
//// //         float weavingAngle = centerAngle / 2f;

//// //         // --- [디버깅 로그 및 시각화 코드 수정] ---
//// //         Debug.Log($"--- '{face.Name}' 위빙 각도 계산 시작 ---");
//// //         Debug.Log($"[Input] 시작 꼭짓점 R1: {p1_arcStart.ToString("F3")}");
//// //         Debug.Log($"[Input] 끝점 후보: R2, R3, R4");
//// //         Debug.Log($"<color=green>[Selection] R1과 가장 가까운 점: {closestPointName} ({p2_arcEnd.ToString("F3")})</color>");
//// //         Debug.Log($"[Calc] R1-{closestPointName}의 중점: {midPoint.ToString("F3")}");
//// //         Debug.Log($"[Calc] 원호 중심(Center): {p3_arcCenter.ToString("F3")}");

//// //         // Scene 뷰에 삼각형을 그립니다.
//// //         Debug.DrawLine(p3_arcCenter, p1_arcStart, Color.magenta, 10.0f); // 중심 -> R1 (보라색)
//// //         Debug.DrawLine(p3_arcCenter, p2_arcEnd, Color.yellow, 10.0f);    // 중심 -> 가장 가까운 점 (노란색)
//// //         Debug.DrawLine(p1_arcStart, p2_arcEnd, Color.cyan, 10.0f);      // 밑변 (청록색)
//// //         Debug.Log($"[Visual] Scene에 계산용 삼각형을 표시했습니다.");

//// //         Debug.Log($"[Result] 삼각형 중심각: {centerAngle:F2}도, 최종 위빙 각도: {weavingAngle:F2}도");
//// //         Debug.Log("------------------------------------");

//// //         return weavingAngle;
//// //     }

//// //     /// <summary>
//// //     /// [수정된 기능] 장애물 높이를 기반으로 안전 점프 높이를 정확하게 계산합니다.
//// //     /// </summary>
//// //     private float CalculateSafeJumpHeight(Vector3 startPoint, Vector3 faceNormal, Vector3 widthDir, float widthLen, Vector3 heightDir, float heightLen, List<Collider> obstacles, PathGenerationConfig config)
//// //     {
//// //         float maxHeight = 0f;
//// //         if (obstacles == null || obstacles.Count == 0) return 0.2f;

//// //         int sampleResolution = 20;
//// //         for (int i = 0; i <= sampleResolution; i++)
//// //         {
//// //             for (int j = 0; j <= sampleResolution; j++)
//// //             {
//// //                 float u = (float)i / sampleResolution;
//// //                 float v = (float)j / sampleResolution;

//// //                 Vector3 pointOnPlane = startPoint + u * widthDir * widthLen + v * heightDir * heightLen;
//// //                 Ray ray = new Ray(pointOnPlane + faceNormal * 10f, -faceNormal);

//// //                 if (Physics.Raycast(ray, out RaycastHit hit, 20f))
//// //                 {
//// //                     if (obstacles.Contains(hit.collider))
//// //                     {
//// //                         float height = 10f - hit.distance;
//// //                         if (height > maxHeight)
//// //                         {
//// //                             maxHeight = height;
//// //                         }
//// //                     }
//// //                 }
//// //             }
//// //         }

//// //         if (maxHeight > 0)
//// //         {
//// //             Debug.Log($"장애물 최고 높이 감지: {maxHeight:F4}m. 점프 높이를 재계산합니다.");
//// //             return maxHeight - config.NormalOffset + 0.1f;
//// //         }

//// //         return 0.2f;
//// //     }

//// //     /// <summary>
//// //     /// [수정됨] 장애물이 있는 면에서 '실시간 U턴' 방식으로 경로를 생성하고, 안전지대 이동 시 장애물을 회피합니다.
//// //     /// </summary>
//// //     private MotionPath GeneratePath_TrueSnakeMethod(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceListIndex, List<Collider> obstacles)
//// //     {
//// //         Vector3 faceNormal = faceNormals[faceListIndex];
//// //         var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
//// //         var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
//// //         if (adjacentPoints.Count != 2) return null;
//// //         Vector3 edge1 = adjacentPoints[0] - start;
//// //         Vector3 edge2 = adjacentPoints[1] - start;
//// //         Vector3 heightDir, widthDir;
//// //         if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) { heightDir = edge1.normalized; } else { heightDir = edge2.normalized; }
//// //         widthDir = Vector3.Cross(heightDir, faceNormal).normalized;
//// //         Vector3 otherEdge = (heightDir == edge1.normalized) ? edge2 : edge1;
//// //         if (Vector3.Dot(widthDir, otherEdge) < 0) widthDir = -widthDir;
//// //         float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
//// //         float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();
//// //         if (faceListIndex >= faceWidthDirs.Count) { faceWidthDirs.Add(widthDir); faceHeightDirs.Add(heightDir); }
//// //         else { faceWidthDirs[faceListIndex] = widthDir; faceHeightDirs[faceListIndex] = heightDir; }

//// //         int gridResX = 25; 
//// //         int gridResY = 10; 
//// //         bool[,] isWall = new bool[gridResX, gridResY];
//// //         bool[,] isVisited = new bool[gridResX, gridResY];
//// //         float cellWidth = widthLen / gridResX;
//// //         float cellHeight = heightLen / gridResY;

//// //         for (int gy = 0; gy < gridResY; gy++) {
//// //             for (int gx = 0; gx < gridResX; gx++) {
//// //                 if (obstacles != null && obstacles.Count > 0) {
//// //                     Vector3 cellCenter = start + widthDir * (gx * cellWidth + cellWidth / 2f) + heightDir * (gy * cellHeight + cellHeight / 2f);
//// //                     if (Physics.CheckSphere(cellCenter, Mathf.Min(cellWidth, cellHeight) / 2f, LayerMask.GetMask("Obstacle"))) {
//// //                         isWall[gx, gy] = true;
//// //                     }
//// //                 }
//// //             }
//// //         }

//// //         // --- 2. 안전 점프 높이 계산 ---
//// //         float safeJumpHeight = CalculateSafeJumpHeight(start, faceNormal, widthDir, widthLen, heightDir, heightLen, obstacles, config);
//// //         // Debug.Log($"[Face {faceListIndex + 1}] 계산된 안전 점프 높이: {safeJumpHeight}");

//// //         // --- 3. 실시간 경로 생성 ---
//// //         List<Vector3> finalPath = new List<Vector3>();
//// //         Vector3 normalOffsetVector = faceNormal * config.NormalOffset;

//// //         while (true) {
//// //             Vector2Int? nextStartCell = FindNextStartCell(isVisited, isWall, gridResX, gridResY);
//// //             if (nextStartCell == null) break;

//// //             if (finalPath.Count > 0) {
//// //                 Vector3 prevEnd = finalPath.Last();
//// //                 Vector3 nextStartPos = start + widthDir * (nextStartCell.Value.x * cellWidth) + heightDir * (nextStartCell.Value.y * cellHeight) + normalOffsetVector;
//// //                 finalPath.Add(prevEnd + faceNormal * safeJumpHeight);
//// //                 finalPath.Add(nextStartPos + faceNormal * safeJumpHeight);
//// //             }

//// //             Vector2Int currentCell = nextStartCell.Value;
//// //             int xDir = 1;

//// //             while(true) {
//// //                 if(!isVisited[currentCell.x, currentCell.y]){
//// //                     finalPath.Add(start + widthDir * (currentCell.x * cellWidth + cellWidth / 2f) + heightDir * (currentCell.y * cellHeight + cellHeight / 2f) + normalOffsetVector);
//// //                     isVisited[currentCell.x, currentCell.y] = true;
//// //                 }
//// //                 Vector2Int nextHorizontalCell = new Vector2Int(currentCell.x + xDir, currentCell.y);
//// //                 if (nextHorizontalCell.x >= 0 && nextHorizontalCell.x < gridResX && !isWall[nextHorizontalCell.x, nextHorizontalCell.y] && !isVisited[nextHorizontalCell.x, nextHorizontalCell.y]) {
//// //                     currentCell = nextHorizontalCell;
//// //                     continue;
//// //                 }
//// //                 Vector2Int nextVerticalCell = new Vector2Int(currentCell.x, currentCell.y + 1);
//// //                  if (nextVerticalCell.y < gridResY && !isWall[nextVerticalCell.x, nextVerticalCell.y] && !isVisited[nextVerticalCell.x, nextVerticalCell.y]) {
//// //                     currentCell = nextVerticalCell;
//// //                     xDir *= -1;
//// //                     continue;
//// //                 }
//// //                 break; 
//// //             }
//// //         }

//// //         if (finalPath.Count < 2) return null;

//// //         // --- 4. 최종 경로에 안전지대 추가 및 장애물 회피 로직 적용 ---
//// //         Vector3 entryDirection = (finalPath[0] - finalPath[1]).normalized;
//// //         finalPath.Insert(0, finalPath[0] + entryDirection * 1.5f);

//// //         Vector3 lastWorkPoint = finalPath.Last();

//// //         var adjacentToEndPoints = allPoints.Where(p => p != start && p != end).ToList();
//// //         Vector3 exitTangent;
//// //         if (adjacentToEndPoints.Count == 2) {
//// //              Vector3 edgeVecAtEnd1 = (end - adjacentToEndPoints[0]).normalized;
//// //              Vector3 edgeVecAtEnd2 = (end - adjacentToEndPoints[1]).normalized;
//// //              if (Mathf.Abs(Vector3.Dot(edgeVecAtEnd1, widthDir)) > Mathf.Abs(Vector3.Dot(edgeVecAtEnd2, widthDir))) {
//// //                  exitTangent = edgeVecAtEnd1;
//// //              } else {
//// //                  exitTangent = edgeVecAtEnd2;
//// //              }
//// //         } else {
//// //             exitTangent = (lastWorkPoint - finalPath[finalPath.Count-2]).normalized;
//// //         }

//// //         Vector3 exitSafePoint = end + exitTangent * 1.5f + normalOffsetVector;

//// //         Vector3 liftPoint = lastWorkPoint + faceNormal * safeJumpHeight;
//// //         Vector3 horizontalMove = exitSafePoint - lastWorkPoint;
//// //         horizontalMove -= Vector3.Dot(horizontalMove, faceNormal) * faceNormal;
//// //         Vector3 approachPoint = liftPoint + horizontalMove;

//// //         finalPath.Add(liftPoint);
//// //         finalPath.Add(approachPoint);
//// //         finalPath.Add(exitSafePoint);

//// //         // --- 5. 회전값 계산 및 최종 반환 ---
//// //         List<Quaternion> rotations = new List<Quaternion>();
//// //         Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(60, heightDir) * faceNormal, heightDir);
//// //         for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

//// //         return new MotionPath(faceListIndex + 1, PathType.Snake, config.NormalOffset, finalPath, rotations, faceNormal, start, exitSafePoint, widthDir, heightDir);
//// //     }

//// //     private Vector2Int? FindNextStartCell(bool[,] isVisited, bool[,] isWall, int width, int height)
//// //     {
//// //         for (int y = 0; y < height; y++) {
//// //             for (int x = 0; x < width; x++) {
//// //                 if (!isVisited[x, y] && !isWall[x, y]) {
//// //                     return new Vector2Int(x, y);
//// //                 }
//// //             }
//// //         }
//// //         return null;
//// //     }

//// //     private bool IsFaceIntersectingObstacle(Plane_and_NV.Cube face, List<Collider> obstacles)
//// //     {
//// //         if (obstacles == null || obstacles.Count == 0) return false;
//// //         Vector3 center = (face.R1 + face.R2 + face.R3 + face.R4) / 4.0f;
//// //         Vector3 halfExtents = new Vector3(Vector3.Distance(face.R1, face.R2) / 2, Vector3.Distance(face.R1, face.R4) / 2, 0.1f);
//// //         Quaternion orientation = Quaternion.LookRotation(face.normal, (face.R4 - face.R1).normalized);
//// //         Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);
//// //         foreach (var hit in hits) {
//// //             if (obstacles.Contains(hit)) return true;
//// //         }
//// //         return false;
//// //     }

//// //     private List<MotionPath> ExecuteWithScaledObstacles(List<Collider> obstacles, float scaleFactor, Func<List<MotionPath>> generationAction)
//// //     {
//// //         var originalBoxSizes = new Dictionary<BoxCollider, Vector3>();
//// //         var originalSphereRadii = new Dictionary<SphereCollider, float>();
//// //         var originalCapsuleDimensions = new Dictionary<CapsuleCollider, (float radius, float height)>();
//// //         if (obstacles != null) {
//// //             foreach (var obsCollider in obstacles) {
//// //                 if (obsCollider == null) continue;
//// //                 if (obsCollider is BoxCollider box) originalBoxSizes[box] = box.size;
//// //                 else if (obsCollider is SphereCollider sphere) originalSphereRadii[sphere] = sphere.radius;
//// //                 else if (obsCollider is CapsuleCollider capsule) originalCapsuleDimensions[capsule] = (capsule.radius, capsule.height);
//// //             }
//// //         }
//// //         try {
//// //             foreach (var pair in originalBoxSizes) { pair.Key.size = pair.Value * scaleFactor; }
//// //             foreach (var pair in originalSphereRadii) { pair.Key.radius = pair.Value * scaleFactor; }
//// //             foreach (var pair in originalCapsuleDimensions) { pair.Key.radius = pair.Value.radius * scaleFactor; pair.Key.height = pair.Value.height * scaleFactor; }
//// //             Physics.SyncTransforms();
//// //             return generationAction();
//// //         }
//// //         finally {
//// //             foreach (var pair in originalBoxSizes) if(pair.Key != null) pair.Key.size = pair.Value;
//// //             foreach (var pair in originalSphereRadii) if (pair.Key != null) pair.Key.radius = pair.Value;
//// //             foreach (var pair in originalCapsuleDimensions) { if (pair.Key != null) { pair.Key.radius = pair.Value.radius; pair.Key.height = pair.Value.height; } }
//// //             Physics.SyncTransforms();
//// //         }
//// //     }
//// // }

//// using UnityEngine;
//// using System.Collections.Generic;
//// using System.Linq;
//// using System;
//// using System.Text;

//// public enum PathType
//// {
////     Zigzag, // GenerateZigzagPathForFace 메서드로 생성된 경로
////     Snake,  // GeneratePath_TrueSnakeMethod 메서드로 생성된 경로
////     Unknown // 기타 또는 알 수 없는 경로
//// }

//// public class MotionPathGenerator
//// {
////     public struct PathGenerationConfig
////     {
////         public int NumHorizontalPoints;
////         public float VerticalStep;
////         public float NormalOffset;
////         public float ObstacleOffset;
////         public float ObstacleScaleFactor;
////     }

////     public class MotionPath
////     {
////         public PathType Type { get; }
////         public float NormalOffset { get; }
////         public int FaceIndex { get; }
////         public List<Vector3> Positions { get; }
////         public List<Quaternion> Rotations { get; }
////         public Vector3 FaceNormal { get; }
////         public Vector3 StartPoint { get; }
////         public Vector3 EndPoint { get; }
////         public Vector3 WidthDir { get; }
////         public Vector3 HeightDir { get; }
////         public MotionPath(int faceIndex, PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir)
////         {
////             FaceIndex = faceIndex;
////             Type = type;
////             NormalOffset = normalOffset;
////             Positions = positions;
////             Rotations = rotations;
////             FaceNormal = faceNormal;
////             StartPoint = startPoint;
////             EndPoint = endPoint;
////             WidthDir = widthDir;
////             HeightDir = heightDir;
////         }
////     }
////     public class MotionPath_Weaving : MotionPath
////     {
////         public float WeavingAngle { get; }

////         public MotionPath_Weaving(int faceIndex, PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir, float weavingAngle)
////             : base(faceIndex, type, normalOffset, positions, rotations, faceNormal, startPoint, endPoint, widthDir, heightDir)
////         {
////             WeavingAngle = weavingAngle;
////         }
////     }

////     private List<Vector3> faceNormals = new List<Vector3>();
////     private List<Vector3> faceWidthDirs = new List<Vector3>();
////     private List<Vector3> faceHeightDirs = new List<Vector3>();
////     private Transform tcpTransform;

////     public MotionPathGenerator(Transform tcpTransform)
////     {
////         if (tcpTransform == null)
////         {
////             Debug.LogError("[MotionPathGenerator] 생성자에 유효한 TCP Transform이 전달되지 않았습니다.");
////         }
////         this.tcpTransform = tcpTransform;
////     }

////     public void ClearPathData()
////     {
////         faceNormals.Clear();
////         faceWidthDirs.Clear();
////         faceHeightDirs.Clear();
////     }

////     public List<MotionPath> GenerateAllPaths(Plane_and_NV.Cube[] cubes, PathGenerationConfig config, Vector3 robotInitPosition, List<Collider> obstacles, int faceIndexOffset = 0)
////     {
////         return ExecuteWithScaledObstacles(obstacles, config.ObstacleScaleFactor, () =>
////         {
////             var allPaths = new List<MotionPath>();
////             if (cubes == null || cubes.Length == 0) return allPaths;

////             Vector3 previousActualEndPoint = robotInitPosition;

////             for (int i = 0; i < cubes.Length; i++)
////             {
////                 var currentCube = cubes[i];
////                 var currentPoints = new Vector3[] { currentCube.R1, currentCube.R2, currentCube.R3, currentCube.R4 };
////                 Vector3 currentFaceNormal = -Vector3.Cross(currentCube.R2 - currentCube.R1, currentCube.R4 - currentCube.R1).normalized;
////                 faceNormals.Add(currentFaceNormal);

////                 Vector3 startPoint;

////                 if (i == 0)
////                 {
////                     var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(robotInitPosition, p)).Take(2).ToList();
////                     startPoint = twoClosest.OrderBy(p => p.y).First();
////                 }
////                 else
////                 {
////                     startPoint = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).First();
////                 }

////                 Vector3 endPoint = currentPoints.OrderByDescending(p => Vector3.Distance(startPoint, p)).First();

////                 var currentFaceConfig = config;
////                 List<Collider> relevantObstacles = new List<Collider>();
////                 if (IsFaceIntersectingObstacle(currentCube, obstacles))
////                 {
////                     currentFaceConfig.NumHorizontalPoints = 30;
////                     relevantObstacles = obstacles;
////                 }

////                 MotionPath path;

////                 if (i == 9)
////                 {
////                     path = GeneratePath_TrueSnakeMethod(currentCube, startPoint, endPoint, currentFaceConfig, i + faceIndexOffset, relevantObstacles);
////                 }
////                 else
////                 {
////                     path = GenerateZigzagPathForFace(currentCube, startPoint, endPoint, currentFaceConfig, i + faceIndexOffset, currentFaceNormal, relevantObstacles);
////                 }

////                 if (path != null && path.Positions.Count > 1)
////                 {
////                     allPaths.Add(path);
////                     previousActualEndPoint = path.EndPoint;
////                 }
////             }
////             return allPaths;
////         });
////     }

////     private MotionPath GenerateZigzagPathForFace(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, Vector3 faceNormal, List<Collider> obstacles)
////     {
////         var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
////         var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
////         if (adjacentPoints.Count != 2) return null;

////         Vector3 edge1 = adjacentPoints[0] - start;
////         Vector3 edge2 = adjacentPoints[1] - start;
////         Vector3 heightDir, widthDir;

////         if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.95f)
////         {
////             if (edge1.magnitude < edge2.magnitude)
////             {
////                 heightDir = edge1.normalized;
////             }
////             else
////             {
////                 heightDir = edge2.normalized;
////             }
////         }
////         else
////         {
////             if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
////             {
////                 heightDir = edge1.normalized;
////             }
////             else
////             {
////                 heightDir = edge2.normalized;
////             }
////         }

////         widthDir = Vector3.Cross(heightDir, faceNormal).normalized;

////         Vector3 otherEdge;
////         if (Vector3.Dot(heightDir, edge1.normalized) > 0.999f)
////         {
////             otherEdge = edge2;
////         }
////         else
////         {
////             otherEdge = edge1;
////         }

////         if (Vector3.Dot(widthDir, otherEdge) < 0)
////         {
////             widthDir = -widthDir;
////         }

////         float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
////         float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();

////         if (faceId-1 >= faceWidthDirs.Count) { faceWidthDirs.Add(widthDir); faceHeightDirs.Add(heightDir); }
////         else { faceWidthDirs[faceId-1] = widthDir; faceHeightDirs[faceId-1] = heightDir; }

////         List<Vector3> workPoints = new List<Vector3>();
////         Vector3 curr = start;
////         float movedHeight = 0f;
////         bool toRight = true;
////         while (movedHeight <= heightLen + 1e-4f)
////         {
////             for (int j = 0; j < config.NumHorizontalPoints; j++)
////             {
////                 float t = (config.NumHorizontalPoints > 1) ? (float)j / (config.NumHorizontalPoints - 1) : 0;
////                 workPoints.Add(curr + widthDir * widthLen * (toRight ? t : (1 - t)));
////             }
////             if (movedHeight >= heightLen) break;
////             movedHeight += config.VerticalStep;
////             if (movedHeight > heightLen) movedHeight = heightLen;
////             curr = start + heightDir * movedHeight;
////             toRight = !toRight;
////         }

////         List<Vector3> finalPath = workPoints.Select(p => p + faceNormal * config.NormalOffset).ToList();
////         if (finalPath.Count < 2) return null;

////         float safeDistance = (faceId-1 >= 0 && faceId-1 <= 4) ? 0.2f : 1.7f;
////         finalPath.Insert(0, finalPath[0] + (finalPath[0] - finalPath[1]).normalized * safeDistance);
////         finalPath.Add(finalPath.Last() + (finalPath.Last() - finalPath[finalPath.Count - 2]).normalized * safeDistance);

////         List<Quaternion> rotations = new List<Quaternion>();
////         int angle_bps = 60;
////         if (faceId == 7) // 면 번호 7에 대한 특수 처리
////         {
////             angle_bps = 120;
////         }

////         Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
////         for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

////         return new MotionPath(faceId, PathType.Zigzag, config.NormalOffset, finalPath, rotations, faceNormal, start, finalPath[finalPath.Count - 2], widthDir, heightDir);
////     }

////     public MotionPath_Weaving GenerateWeavingPath(Plane_and_NV.Cube wFace, PathGenerationConfig config)
////     {
////         if (tcpTransform == null) { return null; }
////         if (string.IsNullOrEmpty(wFace.Name) || !wFace.Name.StartsWith("W")) { return null; }

////         Debug.Log($"<color=yellow>[MotionPathGenerator] '{wFace.Name}' 면에 대한 위빙 경로 생성을 시작합니다.</color>");

////         faceNormals.Add(-Vector3.Cross(wFace.R2 - wFace.R1, wFace.R4 - wFace.R1).normalized);

////         var allPoints = new List<Vector3> { wFace.R1, wFace.R2, wFace.R3, wFace.R4 };

////         Vector3 startPointOnFace = allPoints.OrderBy(p => Vector3.Distance(tcpTransform.position, p)).First();
////         Vector3 endPointOnFace = allPoints.OrderByDescending(p => Vector3.Distance(startPointOnFace, p)).First();

////         var adjacentPoints = allPoints.Where(p => p != startPointOnFace && p != endPointOnFace).ToList();
////         if (adjacentPoints.Count != 2) return null;

////         Vector3 edge1 = adjacentPoints[0] - startPointOnFace;
////         Vector3 edge2 = adjacentPoints[1] - startPointOnFace;
////         Vector3 heightDir, widthDir;

////         if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
////         {
////             heightDir = edge1.normalized;
////             widthDir = (endPointOnFace - adjacentPoints[1]).normalized;
////         }
////         else
////         {
////             heightDir = edge2.normalized;
////             widthDir = (endPointOnFace - adjacentPoints[0]).normalized;
////         }

////         faceWidthDirs.Add(widthDir); 
////         faceHeightDirs.Add(heightDir);

////         Vector3 pathStartPoint = ((startPointOnFace + adjacentPoints.OrderBy(p=>Vector3.Distance(startPointOnFace, p)).First()) / 2f) + wFace.normal * config.NormalOffset;
////         Vector3 pathEndPoint = ((endPointOnFace + adjacentPoints.OrderBy(p=>Vector3.Distance(endPointOnFace, p)).First())/2f) + wFace.normal * config.NormalOffset;

////         float calculatedWeavingAngle = CalculateWeavingAngleForFace(wFace, config.NormalOffset);

////         List<Vector3> positions = new List<Vector3> { pathStartPoint, pathEndPoint };
////         float safeDistance = 0.5f;
////         if (positions.Count >= 2)
////         {
////             positions.Insert(0, positions[0] + (positions[0] - positions[1]).normalized * safeDistance);
////             positions.Add(positions.Last() + (positions.Last() - positions[positions.Count - 2]).normalized * safeDistance);
////         }

////         float angle_bps = 60f;
////         Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * wFace.normal, heightDir);

////         List<Quaternion> rotations = new List<Quaternion>();
////         for (int i = 0; i < positions.Count; i++)
////         {
////             rotations.Add(targetRotation);
////         }

////         int faceIndex;
////         try { faceIndex = int.Parse(wFace.Name.Substring(1)); }
////         catch { faceIndex = -1; }

////         var path = new MotionPath_Weaving(
////             faceIndex: faceIndex, 
////             type: PathType.Unknown, 
////             normalOffset: config.NormalOffset, 
////             positions: positions, 
////             rotations: rotations, 
////             faceNormal: wFace.normal, 
////             startPoint: pathStartPoint,
////             endPoint: pathEndPoint,
////             widthDir: widthDir,
////             heightDir: heightDir,
////             weavingAngle: calculatedWeavingAngle
////         );

////         Debug.Log($"[MotionPathGenerator] '{wFace.Name}' 경로 최종 생성 완료. (총 포인트: {path.Positions.Count}, 블라스팅 각도: {angle_bps}도)");

////         return path;
////     }

////     private float CalculateWeavingAngleForFace(Plane_and_NV.Cube face, float offset)
////     {
////         Vector3 p1_arcStart = face.R1;
////         var candidatePoints = new Dictionary<string, Vector3>
////         {
////             { "R2", face.R2 },
////             { "R3", face.R3 },
////             { "R4", face.R4 }
////         };

////         string closestPointName = "";
////         Vector3 p2_arcEnd = Vector3.zero;
////         float minDistance = float.MaxValue;

////         foreach (var candidate in candidatePoints)
////         {
////             float distance = Vector3.Distance(p1_arcStart, candidate.Value);
////             if (distance < minDistance)
////             {
////                 minDistance = distance;
////                 p2_arcEnd = candidate.Value;
////                 closestPointName = candidate.Key;
////             }
////         }

////         Vector3 midPoint = (p1_arcStart + p2_arcEnd) / 2f;
////         Vector3 p3_arcCenter = midPoint + face.normal * offset;

////         Vector3 vectorToStart = p1_arcStart - p3_arcCenter;
////         Vector3 vectorToEnd = p2_arcEnd - p3_arcCenter;

////         float centerAngle = Vector3.Angle(vectorToStart, vectorToEnd);
////         float weavingAngle = centerAngle / 2f;

////         Debug.Log($"--- '{face.Name}' 위빙 각도 계산 시작 ---");
////         Debug.Log($"[Input] 시작 꼭짓점 R1: {p1_arcStart.ToString("F3")}");
////         Debug.Log($"[Input] 끝점 후보: R2, R3, R4");
////         Debug.Log($"<color=green>[Selection] R1과 가장 가까운 점: {closestPointName} ({p2_arcEnd.ToString("F3")})</color>");
////         Debug.Log($"[Calc] R1-{closestPointName}의 중점: {midPoint.ToString("F3")}");
////         Debug.Log($"[Calc] 원호 중심(Center): {p3_arcCenter.ToString("F3")}");

////         Debug.DrawLine(p3_arcCenter, p1_arcStart, Color.magenta, 10.0f);
////         Debug.DrawLine(p3_arcCenter, p2_arcEnd, Color.yellow, 10.0f);
////         Debug.DrawLine(p1_arcStart, p2_arcEnd, Color.cyan, 10.0f);
////         Debug.Log($"[Visual] Scene에 계산용 삼각형을 표시했습니다.");

////         Debug.Log($"[Result] 삼각형 중심각: {centerAngle:F2}도, 최종 위빙 각도: {weavingAngle:F2}도");
////         Debug.Log("------------------------------------");

////         return weavingAngle;
////     }

////     private float CalculateSafeJumpHeight(Vector3 startPoint, Vector3 faceNormal, Vector3 widthDir, float widthLen, Vector3 heightDir, float heightLen, List<Collider> obstacles, PathGenerationConfig config)
////     {
////         float maxHeight = 0f;
////         if (obstacles == null || obstacles.Count == 0) return 0.2f;

////         int sampleResolution = 20;
////         for (int i = 0; i <= sampleResolution; i++)
////         {
////             for (int j = 0; j <= sampleResolution; j++)
////             {
////                 float u = (float)i / sampleResolution;
////                 float v = (float)j / sampleResolution;

////                 Vector3 pointOnPlane = startPoint + u * widthDir * widthLen + v * heightDir * heightLen;
////                 Ray ray = new Ray(pointOnPlane + faceNormal * 10f, -faceNormal);

////                 if (Physics.Raycast(ray, out RaycastHit hit, 20f))
////                 {
////                     if (obstacles.Contains(hit.collider))
////                     {
////                         float height = 10f - hit.distance;
////                         if (height > maxHeight)
////                         {
////                             maxHeight = height;
////                         }
////                     }
////                 }
////             }
////         }

////         if (maxHeight > 0)
////         {
////             Debug.Log($"장애물 최고 높이 감지: {maxHeight:F4}m. 점프 높이를 재계산합니다.");
////             return maxHeight - config.NormalOffset + 0.1f;
////         }

////         return 0.2f;
////     }

////     private MotionPath GeneratePath_TrueSnakeMethod(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
////     {
////         Vector3 faceNormal = faceNormals[faceId];
////         var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
////         var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
////         if (adjacentPoints.Count != 2) return null;
////         Vector3 edge1 = adjacentPoints[0] - start;
////         Vector3 edge2 = adjacentPoints[1] - start;
////         Vector3 heightDir, widthDir;
////         if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) { heightDir = edge1.normalized; } else { heightDir = edge2.normalized; }
////         widthDir = Vector3.Cross(heightDir, faceNormal).normalized;
////         Vector3 otherEdge = (heightDir == edge1.normalized) ? edge2 : edge1;
////         if (Vector3.Dot(widthDir, otherEdge) < 0) widthDir = -widthDir;
////         float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
////         float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();
////         if (faceId >= faceWidthDirs.Count) { faceWidthDirs.Add(widthDir); faceHeightDirs.Add(heightDir); }
////         else { faceWidthDirs[faceId] = widthDir; faceHeightDirs[faceId] = heightDir; }

////         int gridResX = 25; 
////         int gridResY = 10; 
////         bool[,] isWall = new bool[gridResX, gridResY];
////         bool[,] isVisited = new bool[gridResX, gridResY];
////         float cellWidth = widthLen / gridResX;
////         float cellHeight = heightLen / gridResY;

////         for (int gy = 0; gy < gridResY; gy++) {
////             for (int gx = 0; gx < gridResX; gx++) {
////                 if (obstacles != null && obstacles.Count > 0) {
////                     Vector3 cellCenter = start + widthDir * (gx * cellWidth + cellWidth / 2f) + heightDir * (gy * cellHeight + cellHeight / 2f);
////                     if (Physics.CheckSphere(cellCenter, Mathf.Min(cellWidth, cellHeight) / 2f, LayerMask.GetMask("Obstacle"))) {
////                         isWall[gx, gy] = true;
////                     }
////                 }
////             }
////         }

////         float safeJumpHeight = CalculateSafeJumpHeight(start, faceNormal, widthDir, widthLen, heightDir, heightLen, obstacles, config);

////         List<Vector3> finalPath = new List<Vector3>();
////         Vector3 normalOffsetVector = faceNormal * config.NormalOffset;

////         while (true) {
////             Vector2Int? nextStartCell = FindNextStartCell(isVisited, isWall, gridResX, gridResY);
////             if (nextStartCell == null) break;

////             if (finalPath.Count > 0) {
////                 Vector3 prevEnd = finalPath.Last();
////                 Vector3 nextStartPos = start + widthDir * (nextStartCell.Value.x * cellWidth) + heightDir * (nextStartCell.Value.y * cellHeight) + normalOffsetVector;
////                 finalPath.Add(prevEnd + faceNormal * safeJumpHeight);
////                 finalPath.Add(nextStartPos + faceNormal * safeJumpHeight);
////             }

////             Vector2Int currentCell = nextStartCell.Value;
////             int xDir = 1;

////             while(true) {
////                 if(!isVisited[currentCell.x, currentCell.y]){
////                     finalPath.Add(start + widthDir * (currentCell.x * cellWidth + cellWidth / 2f) + heightDir * (currentCell.y * cellHeight + cellHeight / 2f) + normalOffsetVector);
////                     isVisited[currentCell.x, currentCell.y] = true;
////                 }
////                 Vector2Int nextHorizontalCell = new Vector2Int(currentCell.x + xDir, currentCell.y);
////                 if (nextHorizontalCell.x >= 0 && nextHorizontalCell.x < gridResX && !isWall[nextHorizontalCell.x, nextHorizontalCell.y] && !isVisited[nextHorizontalCell.x, nextHorizontalCell.y]) {
////                     currentCell = nextHorizontalCell;
////                     continue;
////                 }
////                 Vector2Int nextVerticalCell = new Vector2Int(currentCell.x, currentCell.y + 1);
////                  if (nextVerticalCell.y < gridResY && !isWall[nextVerticalCell.x, nextVerticalCell.y] && !isVisited[nextVerticalCell.x, nextVerticalCell.y]) {
////                     currentCell = nextVerticalCell;
////                     xDir *= -1;
////                     continue;
////                 }
////                 break; 
////             }
////         }

////         if (finalPath.Count < 2) return null;

////         Vector3 entryDirection = (finalPath[0] - finalPath[1]).normalized;
////         finalPath.Insert(0, finalPath[0] + entryDirection * 1.5f);

////         Vector3 lastWorkPoint = finalPath.Last();

////         var adjacentToEndPoints = allPoints.Where(p => p != start && p != end).ToList();
////         Vector3 exitTangent;
////         if (adjacentToEndPoints.Count == 2) {
////              Vector3 edgeVecAtEnd1 = (end - adjacentToEndPoints[0]).normalized;
////              Vector3 edgeVecAtEnd2 = (end - adjacentToEndPoints[1]).normalized;
////              if (Mathf.Abs(Vector3.Dot(edgeVecAtEnd1, widthDir)) > Mathf.Abs(Vector3.Dot(edgeVecAtEnd2, widthDir))) {
////                  exitTangent = edgeVecAtEnd1;
////              } else {
////                  exitTangent = edgeVecAtEnd2;
////              }
////         } else {
////             exitTangent = (lastWorkPoint - finalPath[finalPath.Count-2]).normalized;
////         }

////         Vector3 exitSafePoint = end + exitTangent * 1.5f + normalOffsetVector;

////         Vector3 liftPoint = lastWorkPoint + faceNormal * safeJumpHeight;
////         Vector3 horizontalMove = exitSafePoint - lastWorkPoint;
////         horizontalMove -= Vector3.Dot(horizontalMove, faceNormal) * faceNormal;
////         Vector3 approachPoint = liftPoint + horizontalMove;

////         finalPath.Add(liftPoint);
////         finalPath.Add(approachPoint);
////         finalPath.Add(exitSafePoint);

////         List<Quaternion> rotations = new List<Quaternion>();
////         Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(60, heightDir) * faceNormal, heightDir);
////         for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

////         return new MotionPath(faceId + 1, PathType.Snake, config.NormalOffset, finalPath, rotations, faceNormal, start, exitSafePoint, widthDir, heightDir);
////     }

////     private Vector2Int? FindNextStartCell(bool[,] isVisited, bool[,] isWall, int width, int height)
////     {
////         for (int y = 0; y < height; y++) {
////             for (int x = 0; x < width; x++) {
////                 if (!isVisited[x, y] && !isWall[x, y]) {
////                     return new Vector2Int(x, y);
////                 }
////             }
////         }
////         return null;
////     }

////     private bool IsFaceIntersectingObstacle(Plane_and_NV.Cube face, List<Collider> obstacles)
////     {
////         if (obstacles == null || obstacles.Count == 0) return false;
////         Vector3 center = (face.R1 + face.R2 + face.R3 + face.R4) / 4.0f;
////         Vector3 halfExtents = new Vector3(Vector3.Distance(face.R1, face.R2) / 2, Vector3.Distance(face.R1, face.R4) / 2, 0.1f);
////         Quaternion orientation = Quaternion.LookRotation(face.normal, (face.R4 - face.R1).normalized);
////         Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);
////         foreach (var hit in hits) {
////             if (obstacles.Contains(hit)) return true;
////         }
////         return false;
////     }

////     private List<MotionPath> ExecuteWithScaledObstacles(List<Collider> obstacles, float scaleFactor, Func<List<MotionPath>> generationAction)
////     {
////         var originalBoxSizes = new Dictionary<BoxCollider, Vector3>();
////         var originalSphereRadii = new Dictionary<SphereCollider, float>();
////         var originalCapsuleDimensions = new Dictionary<CapsuleCollider, (float radius, float height)>();
////         if (obstacles != null) {
////             foreach (var obsCollider in obstacles) {
////                 if (obsCollider == null) continue;
////                 if (obsCollider is BoxCollider box) originalBoxSizes[box] = box.size;
////                 else if (obsCollider is SphereCollider sphere) originalSphereRadii[sphere] = sphere.radius;
////                 else if (obsCollider is CapsuleCollider capsule) originalCapsuleDimensions[capsule] = (capsule.radius, capsule.height);
////             }
////         }
////         try {
////             foreach (var pair in originalBoxSizes) { pair.Key.size = pair.Value * scaleFactor; }
////             foreach (var pair in originalSphereRadii) { pair.Key.radius = pair.Value * scaleFactor; }
////             foreach (var pair in originalCapsuleDimensions) { pair.Key.radius = pair.Value.radius * scaleFactor; pair.Key.height = pair.Value.height * scaleFactor; }
////             Physics.SyncTransforms();
////             return generationAction();
////         }
////         finally {
////             foreach (var pair in originalBoxSizes) if(pair.Key != null) pair.Key.size = pair.Value;
////             foreach (var pair in originalSphereRadii) if (pair.Key != null) pair.Key.radius = pair.Value;
////             foreach (var pair in originalCapsuleDimensions) { if (pair.Key != null) { pair.Key.radius = pair.Value.radius; pair.Key.height = pair.Value.height; } }
////             Physics.SyncTransforms();
////         }
////     }
//// }

//using UnityEngine;
//using System.Collections.Generic;
//using System.Linq;
//using System;
//using System.Text;

//public enum BA_PathType
//{
//    Zigzag,
//    Snake,
//    Unknown
//}

//public class MotionPathGenerator
//{
//    public CubeStruct cuboid;

//    public struct PathGenerationConfig {
//        public int NumHorizontalPoints;
//        public float VerticalStep;
//        public float NormalOffset;
//        public float ObstacleOffset;
//        public float ObstacleScaleFactor;
//    }

//    public class MotionPath
//    {
//        public BA_PathType Type { get; }
//        public float NormalOffset { get; }
//        public int FaceIndex { get; }
//        public List<Vector3> Positions { get; }
//        public List<Quaternion> Rotations { get; }
//        public Vector3 FaceNormal { get; }
//        public Vector3 StartPoint { get; }
//        public Vector3 EndPoint { get; }
//        public Vector3 WidthDir { get; }
//        public Vector3 HeightDir { get; }
//        public MotionPath(int faceIndex, BA_PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir)
//        {
//            FaceIndex = faceIndex;
//            Type = type;
//            NormalOffset = normalOffset;
//            Positions = positions;
//            Rotations = rotations;
//            FaceNormal = faceNormal;
//            StartPoint = startPoint;
//            EndPoint = endPoint;
//            WidthDir = widthDir;
//            HeightDir = heightDir;
//        }
//    }
//    public class MotionPath_Weaving : MotionPath
//    {
//        public float WeavingAngle { get; }

//        public MotionPath_Weaving(int faceIndex, BA_PathType type, float normalOffset, List<Vector3> positions, List<Quaternion> rotations, Vector3 faceNormal, Vector3 startPoint, Vector3 endPoint, Vector3 widthDir, Vector3 heightDir, float weavingAngle)
//            : base(faceIndex, type, normalOffset, positions, rotations, faceNormal, startPoint, endPoint, widthDir, heightDir)
//        {
//            WeavingAngle = weavingAngle;
//        }
//    }

//    private List<Vector3> faceNormals = new List<Vector3>();
//    private List<Vector3> faceWidthDirs = new List<Vector3>();
//    private List<Vector3> faceHeightDirs = new List<Vector3>();
//    private Transform tcpTransform;

//    public MotionPathGenerator(Transform tcpTransform)
//    {
//        if (tcpTransform == null)
//        {
//            Debug.LogError("[MotionPathGenerator] 생성자에 유효한 TCP Transform이 전달되지 않았습니다.");
//        }
//        this.tcpTransform = tcpTransform;
//    }

//    public void ClearPathData()
//    {
//        faceNormals.Clear();
//        faceWidthDirs.Clear();
//        faceHeightDirs.Clear();
//    }

//    public List<MotionPath> GenerateAllPaths(Plane_and_NV.Cube[] cubes, PathGenerationConfig config, Vector3 startReferencePoint, List<Collider> obstacles, int faceIndexOffset = 0)
//    {
//        return ExecuteWithScaledObstacles(obstacles, config.ObstacleScaleFactor, () =>
//        {
//            var allPaths = new List<MotionPath>();
//            if (cubes == null || cubes.Length == 0) return allPaths;

//            // Vector3 previousActualEndPoint = robotInitPosition;
//            Vector3 previousActualEndPoint = startReferencePoint; 
//            // Debug.Log($"[MotionPathGenerator] 경로 생성 시작. 시작 기준점: {startReferencePoint.ToString("F4")}, 장애물 오프셋: {config.ObstacleOffset:F4}, 스케일 팩터: {config.ObstacleScaleFactor:F4}");
//            for (int i = 0; i < cubes.Length; i++)
//            {
//                var currentCube = cubes[i];
//                var currentPoints = new Vector3[] { currentCube.R1, currentCube.R2, currentCube.R3, currentCube.R4 };

//                // 법선 벡터를 계산하고 공유 리스트에 추가합니다.
//                Vector3 currentFaceNormal = -Vector3.Cross(currentCube.R2 - currentCube.R1, currentCube.R4 - currentCube.R1).normalized;
//                faceNormals.Add(currentFaceNormal);

//                Vector3 startPoint;
//                if (i == 0) // && faceIndexOffset == 0) // 오프셋이 없을 때만 (첫 호출일 때만) 로봇 위치 기준
//                {
//                    // var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(robotInitPosition, p)).Take(2).ToList();
//                    var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).Take(2).ToList();

//                    startPoint = twoClosest.OrderBy(p => p.y).First();
//                }
//                else if (i == 6) // i가 6일 때 startpoint를 R3로 설정
//                {
//                    startPoint = currentCube.R3;
//                }
//                // else if (i == 1 ) // 오프셋이 없을 때만 (첫 호출일 때만) 로봇 위치 기준
//                // {
//                //     // var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(robotInitPosition, p)).Take(2).ToList();
//                //     var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).Take(2).ToList();

//                //     startPoint = twoClosest.OrderBy(p => p.y).First();
//                // }
//                else
//                {
//                    startPoint = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).First();
//                }
//                Vector3 endPoint = currentPoints.OrderByDescending(p => Vector3.Distance(startPoint, p)).First();
//                // Debug.Log($"[MotionPathGenerator] 현재 면 {i + faceIndexOffset + 1}의 시작점: {startPoint.ToString("F4")}, 끝점: {endPoint.ToString("F4")}");
//                var currentFaceConfig = config;
//                List<Collider> relevantObstacles = new List<Collider>();
//                if (IsFaceIntersectingObstacle(currentCube, obstacles))
//                {
//                    //currentFaceConfig.NumHorizontalPoints = 30;
//                    relevantObstacles = obstacles;
//                }

//                MotionPath path;


//                int currentFaceId = i + faceIndexOffset + 1;

//                if (i == 9) // JSON 데이터상 10번째 R면 (인덱스 9)에 대한 특수 처리
//                {
//                    path = GeneratePath_TrueSnakeMethod(currentCube, startPoint, endPoint, currentFaceConfig, currentFaceId, relevantObstacles);
//                }
//                else
//                {
//                    path = GenerateZigzagPathForFace(currentCube, startPoint, endPoint, currentFaceConfig, currentFaceId, relevantObstacles);
//                }

//                //if (path != null && path.Positions.Count > 1)
//                //{
//                //    Debug.Log($"--- [Loop Index: {i}, Face ID: {currentFaceId}] 경로 생성 결과 ---");
//                //    Debug.Log($"[계산된 시작점] startPoint 변수: {startPoint.ToString("F4")}");
//                //    // 안전지대를 제외한 실제 작업 시작점은 Positions[1] 입니다.
//                //    Debug.Log($"[실제 경로 시작] path.Positions[1]: {path.Positions[1].ToString("F4")}");
//                //}
//                //else
//                //{
//                //    Debug.LogError($"--- [Loop Index: {i}, Face ID: {currentFaceId}] 경로 생성 실패! (path is null or has too few points) ---");
//                //}

//                if (path != null && path.Positions.Count > 1)
//                {
//                    allPaths.Add(path);
//                    previousActualEndPoint = path.EndPoint;
//                }
//            }
//            return allPaths;
//        });
//    }

//    private MotionPath GenerateZigzagPathForFace(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
//    {
//        // 함수 시작 시 디버깅 로그 (8번, 10번 면 대상)

//        int listIndex = faceId - 1;
//        if (listIndex < 0 || listIndex >= faceNormals.Count)
//        {
//            Debug.LogError($"GenerateZigzagPathForFace: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다.");
//            return null;
//        }
//        Vector3 faceNormal = faceNormals[listIndex];

//        var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
//        var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();

//        if (adjacentPoints.Count != 2)
//        {
//            Debug.LogError($"[Face {faceId}] 이웃 꼭짓점을 찾는 데 실패했습니다. (개수: {adjacentPoints.Count})");
//            return null;
//        }

//        Vector3 edge1 = adjacentPoints[0] - start;
//        Vector3 edge2 = adjacentPoints[1] - start;

//        // ★★★ 최종 수정 로직 적용 ★★★

//        Vector3 widthEdge, heightEdge;

//        // 1. 면의 기울기에 따라 '너비'와 '높이' 역할을 할 모서리(Edge)를 명확히 정의
//        if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.95f)
//        {
//            // 수평면: 긴 모서리를 widthEdge, 짧은 모서리를 heightEdge로 정의
//            if (edge1.magnitude > edge2.magnitude)
//            {
//                widthEdge = edge1;
//                heightEdge = edge2;
//            }
//            else
//            {
//                widthEdge = edge2;
//                heightEdge = edge1;
//            }
//        }
//        else
//        {
//            // 일반(기울어진)면: Y축 방향과 가까운 쪽을 heightEdge로 정의
//            if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
//            {
//                heightEdge = edge1;
//                widthEdge = edge2;
//            }
//            else
//            {
//                heightEdge = edge2;
//                widthEdge = edge1;
//            }
//        }

//        // 2. 정의된 Edge 벡터에서 방향과 길이를 직접 추출
//        Vector3 heightDir = heightEdge.normalized;
//        Vector3 widthDir = widthEdge.normalized;
//        float heightLen = heightEdge.magnitude;
//        float widthLen = widthEdge.magnitude;


//        // 3. widthDir의 방향성만 최종적으로 한 번 보정 (면의 안쪽을 향하도록)
//        if (Vector3.Dot(widthDir, widthEdge) < 0)
//        {
//            widthDir = -widthDir;
//        }

//        List<Vector3> workPoints = new List<Vector3>();
//        Vector3 curr = start;
//        float movedHeight = 0f;
//        bool toRight = true;

//        while (movedHeight <= heightLen + 1e-4f)
//        {
//            for (int j = 0; j < config.NumHorizontalPoints; j++)
//            {
//                float t = (config.NumHorizontalPoints > 1) ? (float)j / (config.NumHorizontalPoints - 1) : 0;
//                workPoints.Add(curr + widthDir * widthLen * (toRight ? t : (1 - t)));
//            }
//            if (movedHeight >= heightLen) break;

//            movedHeight += config.VerticalStep;
//            if (movedHeight > heightLen) movedHeight = heightLen;

//            curr = start + heightDir * movedHeight;
//            toRight = !toRight;
//        }

//        if (workPoints.Count == 0)
//        {
//            Debug.LogError($"[Face {faceId}] 작업점(workPoints)이 생성되지 않았습니다. heightLen 또는 widthLen이 0일 가능성이 있습니다.");
//            return null;
//        }

//        List<Vector3> finalPath = workPoints.Select(p => p + faceNormal * config.NormalOffset).ToList();
//        if (finalPath.Count < 2) return null;

//        float safeDistance = (listIndex >= 0 && listIndex <= 5) ? 0.2f : 1.7f;
//        finalPath.Insert(0, finalPath[0] + (finalPath[0] - finalPath[1]).normalized * safeDistance);
//        finalPath.Add(finalPath.Last() + (finalPath.Last() - finalPath[finalPath.Count - 2]).normalized * safeDistance);

//        List<Quaternion> rotations = new List<Quaternion>();
//        Quaternion targetRotation;

//        int angle_bps;
//        if (faceId == 8)
//        {
//            if (this.cuboid == null)
//            {
//                Debug.LogError("Cuboid(CubeStruct) 참조가 MotionPathGenerator에 설정되지 않았습니다! angle_bps의 기본값으로 60을 사용합니다.");
//                angle_bps = 60;
//            }
//            else
//            {
//                Vector3 centerPoint = this.cuboid.GetCenterPoint();
//                angle_bps = (start.z > centerPoint.z) ? 120 : 60;
//                Debug.Log($"[Face 8] start.z: {start.z:F3}, center.z: {centerPoint.z:F3} -> angle_bps: {angle_bps}");
//            }
//        }
//        else
//        {
//            angle_bps = 60;
//        }

//        if (faceId == 2)
//        {
//            Vector3 initialForwardOnXZ = new Vector3(faceNormal.x, 0, faceNormal.z).normalized;
//            if (initialForwardOnXZ == Vector3.zero)
//            {
//                initialForwardOnXZ = Vector3.forward;
//            }
//            Quaternion rotationAroundY = Quaternion.AngleAxis(60, Vector3.up);
//            Vector3 finalForward = rotationAroundY * initialForwardOnXZ;
//            targetRotation = Quaternion.LookRotation(finalForward, Vector3.up);
//        }
//        else
//        {
//            targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
//        }
//        for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

//        return new MotionPath(faceId, BA_PathType.Zigzag, config.NormalOffset, finalPath, rotations, faceNormal, start, finalPath[finalPath.Count - 2], widthDir, heightDir);
//    }

//    public MotionPath_Weaving GenerateWeavingPath(Plane_and_NV.Cube wFace, PathGenerationConfig config)
//    {
//        if (tcpTransform == null) { return null; }
//        if (string.IsNullOrEmpty(wFace.Name) || !wFace.Name.StartsWith("W")) { return null; }

//        Debug.Log($"<color=yellow>[MotionPathGenerator] '{wFace.Name}' 면에 대한 위빙 경로 생성을 시작합니다.</color>");

//        // W1 면의 법선 벡터를 계산하고 공유 리스트에 추가합니다.
//        Vector3 w1Normal = -Vector3.Cross(wFace.R2 - wFace.R1, wFace.R4 - wFace.R1).normalized;
//        faceNormals.Add(w1Normal);

//        var allPoints = new List<Vector3> { wFace.R1, wFace.R2, wFace.R3, wFace.R4 };
//        Vector3 startPointOnFace = allPoints.OrderBy(p => Vector3.Distance(tcpTransform.position, p)).First();
//        Vector3 endPointOnFace = allPoints.OrderByDescending(p => Vector3.Distance(startPointOnFace, p)).First();
//        var adjacentPoints = allPoints.Where(p => p != startPointOnFace && p != endPointOnFace).ToList();
//        if (adjacentPoints.Count != 2) return null;

//        Vector3 edge1 = adjacentPoints[0] - startPointOnFace;
//        Vector3 edge2 = adjacentPoints[1] - startPointOnFace;
//        Vector3 heightDir, widthDir;

//        if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
//        {
//            heightDir = edge1.normalized;
//            widthDir = (endPointOnFace - adjacentPoints[1]).normalized;
//        }
//        else
//        {
//            heightDir = edge2.normalized;
//            widthDir = (endPointOnFace - adjacentPoints[0]).normalized;
//        }

//        // W1 면의 방향 벡터들을 공유 리스트에 추가합니다.
//        faceWidthDirs.Add(widthDir); 
//        faceHeightDirs.Add(heightDir);

//        Vector3 pathStartPoint = ((startPointOnFace + adjacentPoints.OrderBy(p=>Vector3.Distance(startPointOnFace, p)).First()) / 2f) + wFace.normal * config.NormalOffset;
//        Vector3 pathEndPoint = ((endPointOnFace + adjacentPoints.OrderBy(p=>Vector3.Distance(endPointOnFace, p)).First())/2f) + wFace.normal * config.NormalOffset;

//        float calculatedWeavingAngle = CalculateWeavingAngleForFace(wFace, config.NormalOffset);

//        List<Vector3> positions = new List<Vector3> { pathStartPoint, pathEndPoint, pathStartPoint };
//        float safeDistance = 0.5f;
//        if (positions.Count >= 2)
//        {
//            positions.Insert(0, positions[0] + (positions[0] - positions[1]).normalized * safeDistance);
//            positions.Add(positions.Last() + (positions.Last() - positions[positions.Count - 2]).normalized * safeDistance);
//        }

//        float angle_bps = 60f;
//        Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * wFace.normal, heightDir);

//        List<Quaternion> rotations = new List<Quaternion>();
//        for (int i = 0; i < positions.Count; i++)
//        {
//            rotations.Add(targetRotation);
//        }

//        int faceIndex;
//        try { faceIndex = int.Parse(wFace.Name.Substring(1)); }
//        catch { faceIndex = -1; }

//        var path = new MotionPath_Weaving(
//            faceIndex: faceIndex, 
//            type: BA_PathType.Unknown, 
//            normalOffset: config.NormalOffset, 
//            positions: positions, 
//            rotations: rotations, 
//            faceNormal: wFace.normal, 
//            startPoint: pathStartPoint,
//            endPoint: pathStartPoint, // <--- 여기를 pathEndPoint 대신 pathStartPoint로 수정

//            // endPoint: pathEndPoint,
//            widthDir: widthDir,
//            heightDir: heightDir,
//            weavingAngle: calculatedWeavingAngle
//        );

//        Debug.Log($"[MotionPathGenerator] '{wFace.Name}' 경로 최종 생성 완료. (총 포인트: {path.Positions.Count}, 블라스팅 각도: {angle_bps}도)");

//        return path;
//    }

//    private float CalculateWeavingAngleForFace(Plane_and_NV.Cube face, float offset)
//    {
//        Vector3 p1_arcStart = face.R1;
//        var candidatePoints = new Dictionary<string, Vector3>
//        {
//            { "R2", face.R2 }, { "R3", face.R3 }, { "R4", face.R4 }
//        };

//        string closestPointName = "";
//        Vector3 p2_arcEnd = Vector3.zero;
//        float minDistance = float.MaxValue;

//        foreach (var candidate in candidatePoints)
//        {
//            float distance = Vector3.Distance(p1_arcStart, candidate.Value);
//            if (distance < minDistance)
//            {
//                minDistance = distance;
//                p2_arcEnd = candidate.Value;
//                closestPointName = candidate.Key;
//            }
//        }

//        Vector3 midPoint = (p1_arcStart + p2_arcEnd) / 2f;
//        Vector3 p3_arcCenter = midPoint + face.normal * offset;
//        Vector3 vectorToStart = p1_arcStart - p3_arcCenter;
//        Vector3 vectorToEnd = p2_arcEnd - p3_arcCenter;
//        float centerAngle = Vector3.Angle(vectorToStart, vectorToEnd);
//        float weavingAngle = centerAngle / 2f;

//        Debug.Log($"--- '{face.Name}' 위빙 각도 계산 시작 ---");
//        Debug.Log($"[Input] 시작 꼭짓점 R1: {p1_arcStart.ToString("F3")}");
//        Debug.Log($"[Input] 끝점 후보: R2, R3, R4");
//        Debug.Log($"<color=green>[Selection] R1과 가장 가까운 점: {closestPointName} ({p2_arcEnd.ToString("F3")})</color>");
//        Debug.Log($"[Calc] R1-{closestPointName}의 중점: {midPoint.ToString("F3")}");
//        Debug.Log($"[Calc] 원호 중심(Center): {p3_arcCenter.ToString("F3")}");
//        Debug.DrawLine(p3_arcCenter, p1_arcStart, Color.magenta, 10.0f);
//        Debug.DrawLine(p3_arcCenter, p2_arcEnd, Color.yellow, 10.0f);
//        Debug.DrawLine(p1_arcStart, p2_arcEnd, Color.cyan, 10.0f);
//        Debug.Log($"[Visual] Scene에 계산용 삼각형을 표시했습니다.");
//        Debug.Log($"[Result] 삼각형 중심각: {centerAngle:F2}도, 최종 위빙 각도: {weavingAngle:F2}도");
//        Debug.Log("------------------------------------");

//        return weavingAngle;
//    }

//    private float CalculateSafeJumpHeight(Vector3 startPoint, Vector3 faceNormal, Vector3 widthDir, float widthLen, Vector3 heightDir, float heightLen, List<Collider> obstacles, PathGenerationConfig config)
//    {
//        float maxHeight = 0f;
//        if (obstacles == null || obstacles.Count == 0) return 0.2f;

//        int sampleResolution = 20;
//        for (int i = 0; i <= sampleResolution; i++)
//        {
//            for (int j = 0; j <= sampleResolution; j++)
//            {
//                float u = (float)i / sampleResolution;
//                float v = (float)j / sampleResolution;
//                Vector3 pointOnPlane = startPoint + u * widthDir * widthLen + v * heightDir * heightLen;
//                Ray ray = new Ray(pointOnPlane + faceNormal * 10f, -faceNormal);

//                if (Physics.Raycast(ray, out RaycastHit hit, 20f))
//                {
//                    if (obstacles.Contains(hit.collider))
//                    {
//                        float height = 10f - hit.distance;
//                        if (height > maxHeight) maxHeight = height;
//                    }
//                }
//            }
//        }

//        if (maxHeight > 0)
//        {
//            Debug.Log($"장애물 최고 높이 감지: {maxHeight:F4}m. 점프 높이를 재계산합니다.");
//            return maxHeight - config.NormalOffset + 0.1f;
//        }

//        return 0.2f;
//    }

//    private MotionPath GeneratePath_TrueSnakeMethod(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
//    {
//        int listIndex = faceId - 1;
//        if (listIndex < 0 || listIndex >= faceNormals.Count)
//        {
//             Debug.LogError($"GeneratePath_TrueSnakeMethod: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다. 리스트 크기: {faceNormals.Count}");
//            return null;
//        }
//        Vector3 faceNormal = faceNormals[listIndex];

//        var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
//        var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
//        if (adjacentPoints.Count != 2) return null;
//        Vector3 edge1 = adjacentPoints[0] - start;
//        Vector3 edge2 = adjacentPoints[1] - start;
//        Vector3 heightDir, widthDir;
//        if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) { heightDir = edge1.normalized; } else { heightDir = edge2.normalized; }
//        widthDir = Vector3.Cross(heightDir, faceNormal).normalized;
//        Vector3 otherEdge = (heightDir == edge1.normalized) ? edge2 : edge1;
//        if (Vector3.Dot(widthDir, otherEdge) < 0) widthDir = -widthDir;
//        float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
//        float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();

//        if (listIndex >= faceWidthDirs.Count) { 
//            faceWidthDirs.Add(widthDir); 
//            faceHeightDirs.Add(heightDir); 
//        } else { 
//            faceWidthDirs[listIndex] = widthDir; 
//            faceHeightDirs[listIndex] = heightDir; 
//        }

//        int gridResX = 25; 
//        int gridResY = 10; 
//        bool[,] isWall = new bool[gridResX, gridResY];
//        bool[,] isVisited = new bool[gridResX, gridResY];
//        float cellWidth = widthLen / gridResX;
//        float cellHeight = heightLen / gridResY;

//        for (int gy = 0; gy < gridResY; gy++) {
//            for (int gx = 0; gx < gridResX; gx++) {
//                if (obstacles != null && obstacles.Count > 0) {
//                    Vector3 cellCenter = start + widthDir * (gx * cellWidth + cellWidth / 2f) + heightDir * (gy * cellHeight + cellHeight / 2f);
//                    if (Physics.CheckSphere(cellCenter, Mathf.Min(cellWidth, cellHeight) / 2f, LayerMask.GetMask("Obstacle"))) {
//                        isWall[gx, gy] = true;
//                    }
//                }
//            }
//        }

//        float safeJumpHeight = CalculateSafeJumpHeight(start, faceNormal, widthDir, widthLen, heightDir, heightLen, obstacles, config);
//        List<Vector3> finalPath = new List<Vector3>();
//        Vector3 normalOffsetVector = faceNormal * config.NormalOffset;

//        while (true) {
//            Vector2Int? nextStartCell = FindNextStartCell(isVisited, isWall, gridResX, gridResY);
//            if (nextStartCell == null) break;

//            if (finalPath.Count > 0) {
//                Vector3 prevEnd = finalPath.Last();
//                Vector3 nextStartPos = start + widthDir * (nextStartCell.Value.x * cellWidth) + heightDir * (nextStartCell.Value.y * cellHeight) + normalOffsetVector;
//                finalPath.Add(prevEnd + faceNormal * safeJumpHeight);
//                finalPath.Add(nextStartPos + faceNormal * safeJumpHeight);
//            }

//            Vector2Int currentCell = nextStartCell.Value;
//            int xDir = 1;

//            while(true) {
//                if(!isVisited[currentCell.x, currentCell.y]){
//                    finalPath.Add(start + widthDir * (currentCell.x * cellWidth + cellWidth / 2f) + heightDir * (currentCell.y * cellHeight + cellHeight / 2f) + normalOffsetVector);
//                    isVisited[currentCell.x, currentCell.y] = true;
//                }
//                Vector2Int nextHorizontalCell = new Vector2Int(currentCell.x + xDir, currentCell.y);
//                if (nextHorizontalCell.x >= 0 && nextHorizontalCell.x < gridResX && !isWall[nextHorizontalCell.x, nextHorizontalCell.y] && !isVisited[nextHorizontalCell.x, nextHorizontalCell.y]) {
//                    currentCell = nextHorizontalCell;
//                    continue;
//                }
//                Vector2Int nextVerticalCell = new Vector2Int(currentCell.x, currentCell.y + 1);
//                 if (nextVerticalCell.y < gridResY && !isWall[nextVerticalCell.x, nextVerticalCell.y] && !isVisited[nextVerticalCell.x, nextVerticalCell.y]) {
//                    currentCell = nextVerticalCell;
//                    xDir *= -1;
//                    continue;
//                }
//                break; 
//            }
//        }

//        if (finalPath.Count < 2) return null;

//        Vector3 entryDirection = (finalPath[0] - finalPath[1]).normalized;
//        finalPath.Insert(0, finalPath[0] + entryDirection * 1.5f);

//        Vector3 lastWorkPoint = finalPath.Last();

//        var adjacentToEndPoints = allPoints.Where(p => p != start && p != end).ToList();
//        Vector3 exitTangent;
//        if (adjacentToEndPoints.Count == 2) {
//             Vector3 edgeVecAtEnd1 = (end - adjacentToEndPoints[0]).normalized;
//             Vector3 edgeVecAtEnd2 = (end - adjacentToEndPoints[1]).normalized;
//             exitTangent = (Mathf.Abs(Vector3.Dot(edgeVecAtEnd1, widthDir)) > Mathf.Abs(Vector3.Dot(edgeVecAtEnd2, widthDir))) ? edgeVecAtEnd1 : edgeVecAtEnd2;
//        } else {
//            exitTangent = (lastWorkPoint - finalPath[finalPath.Count-2]).normalized;
//        }

//        Vector3 exitSafePoint = end + exitTangent * 1.5f + normalOffsetVector;
//        Vector3 liftPoint = lastWorkPoint + faceNormal * safeJumpHeight;
//        Vector3 horizontalMove = exitSafePoint - lastWorkPoint;
//        horizontalMove -= Vector3.Dot(horizontalMove, faceNormal) * faceNormal;
//        Vector3 approachPoint = liftPoint + horizontalMove;

//        finalPath.Add(liftPoint);
//        finalPath.Add(approachPoint);
//        finalPath.Add(exitSafePoint);

//        List<Quaternion> rotations = new List<Quaternion>();
//        Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(60, heightDir) * faceNormal, heightDir);
//        for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

//        return new MotionPath(faceId, BA_PathType.Snake, config.NormalOffset, finalPath, rotations, faceNormal, start, exitSafePoint, widthDir, heightDir);
//    }

//    private Vector2Int? FindNextStartCell(bool[,] isVisited, bool[,] isWall, int width, int height)
//    {
//        for (int y = 0; y < height; y++) {
//            for (int x = 0; x < width; x++) {
//                if (!isVisited[x, y] && !isWall[x, y]) {
//                    return new Vector2Int(x, y);
//                }
//            }
//        }
//        return null;
//    }

//    private bool IsFaceIntersectingObstacle(Plane_and_NV.Cube face, List<Collider> obstacles)
//    {
//        if (obstacles == null || obstacles.Count == 0) return false;
//        Vector3 center = (face.R1 + face.R2 + face.R3 + face.R4) / 4.0f;
//        Vector3 halfExtents = new Vector3(Vector3.Distance(face.R1, face.R2) / 2, Vector3.Distance(face.R1, face.R4) / 2, 0.1f);
//        Quaternion orientation = Quaternion.LookRotation(face.normal, (face.R4 - face.R1).normalized);
//        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);
//        foreach (var hit in hits) {
//            if (obstacles.Contains(hit)) return true;
//        }
//        return false;
//    }

//    private List<MotionPath> ExecuteWithScaledObstacles(List<Collider> obstacles, float scaleFactor, Func<List<MotionPath>> generationAction)
//    {
//        var originalBoxSizes = new Dictionary<BoxCollider, Vector3>();
//        var originalSphereRadii = new Dictionary<SphereCollider, float>();
//        var originalCapsuleDimensions = new Dictionary<CapsuleCollider, (float radius, float height)>();
//        if (obstacles != null) {
//            foreach (var obsCollider in obstacles) {
//                if (obsCollider == null) continue;
//                if (obsCollider is BoxCollider box) originalBoxSizes[box] = box.size;
//                else if (obsCollider is SphereCollider sphere) originalSphereRadii[sphere] = sphere.radius;
//                else if (obsCollider is CapsuleCollider capsule) originalCapsuleDimensions[capsule] = (capsule.radius, capsule.height);
//            }
//        }
//        try {
//            foreach (var pair in originalBoxSizes) { pair.Key.size = pair.Value * scaleFactor; }
//            foreach (var pair in originalSphereRadii) { pair.Key.radius = pair.Value * scaleFactor; }
//            foreach (var pair in originalCapsuleDimensions) { pair.Key.radius = pair.Value.radius * scaleFactor; pair.Key.height = pair.Value.height * scaleFactor; }
//            Physics.SyncTransforms();
//            return generationAction();
//        }
//        finally {
//            foreach (var pair in originalBoxSizes) if(pair.Key != null) pair.Key.size = pair.Value;
//            foreach (var pair in originalSphereRadii) if (pair.Key != null) pair.Key.radius = pair.Value;
//            foreach (var pair in originalCapsuleDimensions) { if (pair.Key != null) { pair.Key.radius = pair.Value.radius; pair.Key.height = pair.Value.height; } }
//            Physics.SyncTransforms();
//        }
//    }
//}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using static _SHI_BA.BA_Motion;


namespace _SHI_BA
{
    public class MotionPathGenerator
    {
        public CubeStruct cuboid;

        
        private List<Vector3> faceNormals = new List<Vector3>();
        private List<Vector3> faceWidthDirs = new List<Vector3>();
        private List<Vector3> faceHeightDirs = new List<Vector3>();
        private Transform tcpTransform;

        public MotionPathGenerator(Transform tcpTransform)
        {
            Debug.Assert(tcpTransform != null);
            this.tcpTransform = tcpTransform;
        }

        public void ClearPathData()
        {
            faceNormals.Clear();
            faceWidthDirs.Clear();
            faceHeightDirs.Clear();
        }

        public List<BA_MotionPath> GenerateAllPaths(Cube[] cubes, PathGenerationConfig config, Vector3 startReferencePoint, List<Collider> obstacles, int faceIndexOffset = 0)
        {
            return ExecuteWithScaledObstacles(obstacles, config.ObstacleScaleFactor, () =>
            {
                var allPaths = new List<BA_MotionPath>();
                if (cubes == null || cubes.Length == 0)
                {
                    return allPaths;
                }

                // Vector3 previousActualEndPoint = robotInitPosition;
                Vector3 previousActualEndPoint = startReferencePoint;
                // Debug.Log($"[MotionPathGenerator] 경로 생성 시작. 시작 기준점: {startReferencePoint.ToString("F4")}, 장애물 오프셋: {config.ObstacleOffset:F4}, 스케일 팩터: {config.ObstacleScaleFactor:F4}");
                for (int i = 0; i < cubes.Length; i++)
                {
                    var currentCube = cubes[i];
                    var currentPoints = new Vector3[] { currentCube.R1, currentCube.R2, currentCube.R3, currentCube.R4 };

                    // 법선 벡터를 계산하고 공유 리스트에 추가합니다.
                    Vector3 currentFaceNormal = -Vector3.Cross(currentCube.R2 - currentCube.R1, currentCube.R4 - currentCube.R1).normalized;
                    faceNormals.Add(currentFaceNormal);

                    Vector3 startPoint;
                    if (i == 0) // && faceIndexOffset == 0) // 오프셋이 없을 때만 (첫 호출일 때만) 로봇 위치 기준
                    {
                        // var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(robotInitPosition, p)).Take(2).ToList();
                        var twoClosest = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).Take(2).ToList();

                        startPoint = twoClosest.OrderBy(p => p.y).First();
                    }
                    else if (i == 6) // i가 6일 때 startpoint를 R3로 설정
                    {
                        startPoint = currentCube.R3;
                    }
                    else
                    {
                        startPoint = currentPoints.OrderBy(p => Vector3.Distance(previousActualEndPoint, p)).First();
                    }

                    Vector3 endPoint = currentPoints.OrderByDescending(p => Vector3.Distance(startPoint, p)).First();
                    // Debug.Log($"[MotionPathGenerator] 현재 면 {i + faceIndexOffset + 1}의 시작점: {startPoint.ToString("F4")}, 끝점: {endPoint.ToString("F4")}");
                    var currentFaceConfig = config;
                    List<Collider> relevantObstacles = new List<Collider>();
                    if (IsFaceIntersectingObstacle(currentCube, obstacles))
                    {
                        // currentFaceConfig.NumHorizontalPoints = 30;
                        relevantObstacles = obstacles;
                    }

                    BA_MotionPath path;


                    int currentFaceId = i + faceIndexOffset + 1;

                    if (i == 9) // JSON 데이터상 10번째 R면 (인덱스 9)에 대한 특수 처리
                    {
                        path = GeneratePath_TrueSnakeMethod(currentCube, startPoint, endPoint, currentFaceConfig, currentFaceId, relevantObstacles);
                    }
                    else
                    {
                        if (i == 6) { currentFaceConfig.NumHorizontalPoints = 12; }
                        else { currentFaceConfig.NumHorizontalPoints = config.NumHorizontalPoints; }
                        path = GenerateZigzagPathForFace(currentCube, startPoint, endPoint, currentFaceConfig, currentFaceId, relevantObstacles);
                    }

                    // if (path != null && path.Positions.Count > 1)
                    // {
                    //     Debug.Log($"--- [Loop Index: {i}, Face ID: {currentFaceId}] 경로 생성 결과 ---");
                    //     Debug.Log($"[계산된 시작점] startPoint 변수: {startPoint.ToString("F4")}");
                    //     // 안전지대를 제외한 실제 작업 시작점은 Positions[1] 입니다.
                    //     Debug.Log($"[실제 경로 시작] path.Positions[1]: {path.Positions[1].ToString("F4")}");
                    // }
                    // else
                    // {
                    //     Debug.LogError($"--- [Loop Index: {i}, Face ID: {currentFaceId}] 경로 생성 실패! (path is null or has too few points) ---");
                    // }

                    if (path != null && path.PointList.Count > 1)
                    {
                        allPaths.Add(path);
                        previousActualEndPoint = path.EndPoint;
                    }
                }
                return allPaths;
            });
        }

        public BA_MotionPath GenerateZigzagPathForFace(Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
        {
            // 함수 시작 시 디버깅 로그 (8번, 10번 면 대상)

            int angle_bps;

            // 함수 시작 시 디버깅 로그 (8번, 10번 면 대상)

            // int listIndex = faceId - 1;
            // if (listIndex < 0 || listIndex >= faceNormals.Count)
            // {
            //     Debug.LogError($"GenerateZigzagPathForFace: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다.");
            //     return null;
            // }
            // Vector3 faceNormal = faceNormals[listIndex];
            Vector3 faceNormal = -Vector3.Cross(cube.R2 - cube.R1, cube.R4 - cube.R1).normalized;


            var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
            var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();

            if (adjacentPoints.Count != 2)
            {
                Debug.LogError($"[Face {faceId}] 이웃 꼭짓점을 찾는 데 실패했습니다. (개수: {adjacentPoints.Count})");
                return null;
            }

            Vector3 edge1 = adjacentPoints[0] - start;
            Vector3 edge2 = adjacentPoints[1] - start;

            // ★★★ 최종 수정 로직 적용 ★★★

            Vector3 widthEdge, heightEdge;

            // 1. 면의 기울기에 따라 '너비'와 '높이' 역할을 할 모서리(Edge)를 명확히 정의
            if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.95f)
            {
                // 수평면: 긴 모서리를 widthEdge, 짧은 모서리를 heightEdge로 정의
                if (edge1.magnitude > edge2.magnitude)
                {
                    widthEdge = edge1;
                    heightEdge = edge2;
                }
                else
                {
                    widthEdge = edge2;
                    heightEdge = edge1;
                }
            }
            else
            {
                // 일반(기울어진)면: Y축 방향과 가까운 쪽을 heightEdge로 정의
                if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
                {
                    heightEdge = edge1;
                    widthEdge = edge2;
                }
                else
                {
                    heightEdge = edge2;
                    widthEdge = edge1;
                }
            }

            // 2. 정의된 Edge 벡터에서 방향과 길이를 직접 추출
            Vector3 heightDir = heightEdge.normalized;
            Vector3 widthDir = widthEdge.normalized;
            float heightLen = heightEdge.magnitude;
            float widthLen = widthEdge.magnitude;


            // 3. widthDir의 방향성만 최종적으로 한 번 보정 (면의 안쪽을 향하도록)
            if (Vector3.Dot(widthDir, widthEdge) < 0)
            {
                widthDir = -widthDir;
            }

            List<Vector3> workPoints = new List<Vector3>();
            Vector3 curr = start;
            float movedHeight = 0f;
            bool toRight = true;

            while (movedHeight <= heightLen + 1e-4f)
            {
                for (int j = 0; j < config.NumHorizontalPoints; j++)
                {
                    float t = (config.NumHorizontalPoints > 1) ? (float)j / (config.NumHorizontalPoints - 1) : 0;
                    workPoints.Add(curr + widthDir * widthLen * (toRight ? t : (1 - t)));
                }
                if (movedHeight >= heightLen) break;

                movedHeight += config.VerticalStep;
                if (movedHeight > heightLen) movedHeight = heightLen;

                curr = start + heightDir * movedHeight;
                toRight = !toRight;
            }

            if (workPoints.Count == 0)
            {
                Debug.LogError($"[Face {faceId}] 작업점(workPoints)이 생성되지 않았습니다. heightLen 또는 widthLen이 0일 가능성이 있습니다.");
                return null;
            }

            List<Vector3> finalPath = workPoints.Select(p => p + faceNormal * config.NormalOffset).ToList();
            if (finalPath.Count < 2) return null;

            float safeDistance = (faceId - 1 >= 0 && faceId -1 <= 5) ? 0.2f : 1.7f;
            finalPath.Insert(0, finalPath[0] + (finalPath[0] - finalPath[1]).normalized * safeDistance);
            finalPath.Add(finalPath.Last() + (finalPath.Last() - finalPath[finalPath.Count - 2]).normalized * safeDistance);

            List<Quaternion> rotations = new List<Quaternion>();
            Quaternion targetRotation;

            if (faceId == 8)
            {
                if (this.cuboid == null)
                {
                    Debug.LogError("Cuboid(CubeStruct) 참조가 MotionPathGenerator에 설정되지 않았습니다! angle_bps의 기본값으로 60을 사용합니다.");
                    angle_bps = 60;
                }
                else
                {
                    Vector3 centerPoint = this.cuboid.GetCenterPoint();
                    angle_bps = (start.z > centerPoint.z) ? 60 : 120;
                    Debug.Log($"[Face 8] start.z: {start.z:F3}, center.z: {centerPoint.z:F3} -> angle_bps: {angle_bps}");
                }
            }
            else
            {
                angle_bps = 60;
            }

            if (faceId == 2)
            {
                Vector3 initialForwardOnXZ = new Vector3(faceNormal.x, 0, faceNormal.z).normalized;
                if (initialForwardOnXZ == Vector3.zero)
                {
                    initialForwardOnXZ = Vector3.forward;
                }
                Quaternion rotationAroundY = Quaternion.AngleAxis(60, Vector3.up);
                Vector3 finalForward = rotationAroundY * initialForwardOnXZ;
                targetRotation = Quaternion.LookRotation(finalForward, Vector3.up);
            }
            else
            {
                targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
            }
            for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

            return new BA_MotionPath(faceId, PathType.Zigzag, config.NormalOffset, finalPath, rotations, faceNormal, start, finalPath[finalPath.Count - 2], widthDir, heightDir);
        }

        public BA_WeavingPath GenerateWeavingPath(Cube wFace, PathGenerationConfig config)
        {
            if (tcpTransform == null) { return null; }
            if (string.IsNullOrEmpty(wFace.Name) || !wFace.Name.StartsWith("W")) { return null; }

            Debug.Log($"<color=yellow>[MotionPathGenerator] '{wFace.Name}' 면에 대한 위빙 경로 생성을 시작합니다.</color>");

            // W1 면의 법선 벡터를 계산하고 공유 리스트에 추가합니다.
            Vector3 w1Normal = -Vector3.Cross(wFace.R2 - wFace.R1, wFace.R4 - wFace.R1).normalized;
            faceNormals.Add(w1Normal);

            var allPoints = new List<Vector3> { wFace.R1, wFace.R2, wFace.R3, wFace.R4 };
            Vector3 startPointOnFace = allPoints.OrderBy(p => Vector3.Distance(tcpTransform.position, p)).First();
            Vector3 endPointOnFace = allPoints.OrderByDescending(p => Vector3.Distance(startPointOnFace, p)).First();
            var adjacentPoints = allPoints.Where(p => p != startPointOnFace && p != endPointOnFace).ToList();
            if (adjacentPoints.Count != 2) return null;

            Vector3 edge1 = adjacentPoints[0] - startPointOnFace;
            Vector3 edge2 = adjacentPoints[1] - startPointOnFace;
            Vector3 heightDir, widthDir;

            if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up)))
            {
                heightDir = edge1.normalized;
                widthDir = (endPointOnFace - adjacentPoints[1]).normalized;
            }
            else
            {
                heightDir = edge2.normalized;
                widthDir = (endPointOnFace - adjacentPoints[0]).normalized;
            }

            // W1 면의 방향 벡터들을 공유 리스트에 추가합니다.
            faceWidthDirs.Add(widthDir);
            faceHeightDirs.Add(heightDir);

            Vector3 pathStartPoint = ((startPointOnFace + adjacentPoints.OrderBy(p => Vector3.Distance(startPointOnFace, p)).First()) / 2f) + wFace.normal * config.NormalOffset;
            Vector3 pathEndPoint = ((endPointOnFace + adjacentPoints.OrderBy(p => Vector3.Distance(endPointOnFace, p)).First()) / 2f) + wFace.normal * config.NormalOffset;

            float calculatedWeavingAngle = CalculateWeavingAngleForFace(wFace, config.NormalOffset);

            List<Vector3> positions = new List<Vector3> { pathStartPoint, pathEndPoint, pathStartPoint };
            float safeDistance = 0.5f;
            if (positions.Count >= 2)
            {
                positions.Insert(0, positions[0] + (positions[0] - positions[1]).normalized * safeDistance);
                positions.Add(positions.Last() + (positions.Last() - positions[positions.Count - 2]).normalized * safeDistance);
            }

            float angle_bps = 60f;
            Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * wFace.normal, heightDir);

            List<Quaternion> rotations = new List<Quaternion>();
            for (int i = 0; i < positions.Count; i++)
            {
                rotations.Add(targetRotation);
            }

            int faceIndex;
            try { faceIndex = int.Parse(wFace.Name.Substring(1)); }
            catch { faceIndex = -1; }

            var path = new BA_WeavingPath(
                faceIndex: faceIndex,
                type: PathType.Unknown,
                normalOffset: config.NormalOffset,
                positions: positions,
                rotations: rotations,
                faceNormal: wFace.normal,
                startPoint: pathStartPoint,
                endPoint: pathStartPoint, // <--- 여기를 pathEndPoint 대신 pathStartPoint로 수정

                // endPoint: pathEndPoint,
                widthDir: widthDir,
                heightDir: heightDir,
                weavingAngle: calculatedWeavingAngle
            );

            Debug.Log($"[MotionPathGenerator] '{wFace.Name}' 경로 최종 생성 완료. (총 포인트: {path.PointList.Count}, 블라스팅 각도: {angle_bps}도)");

            return path;
        }

        private float CalculateWeavingAngleForFace(Cube face, float offset)
        {
            Vector3 p1_arcStart = face.R1;
            var candidatePoints = new Dictionary<string, Vector3>
        {
            { "R2", face.R2 }, { "R3", face.R3 }, { "R4", face.R4 }
        };

            string closestPointName = "";
            Vector3 p2_arcEnd = Vector3.zero;
            float minDistance = float.MaxValue;

            foreach (var candidate in candidatePoints)
            {
                float distance = Vector3.Distance(p1_arcStart, candidate.Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    p2_arcEnd = candidate.Value;
                    closestPointName = candidate.Key;
                }
            }

            Vector3 midPoint = (p1_arcStart + p2_arcEnd) / 2f;
            Vector3 p3_arcCenter = midPoint + face.normal * offset;
            Vector3 vectorToStart = p1_arcStart - p3_arcCenter;
            Vector3 vectorToEnd = p2_arcEnd - p3_arcCenter;
            float centerAngle = Vector3.Angle(vectorToStart, vectorToEnd);
            float weavingAngle = centerAngle / 2f;

            Debug.Log($"--- '{face.Name}' 위빙 각도 계산 시작 ---");
            Debug.Log($"[Input] 시작 꼭짓점 R1: {p1_arcStart.ToString("F3")}");
            Debug.Log($"[Input] 끝점 후보: R2, R3, R4");
            Debug.Log($"<color=green>[Selection] R1과 가장 가까운 점: {closestPointName} ({p2_arcEnd.ToString("F3")})</color>");
            Debug.Log($"[Calc] R1-{closestPointName}의 중점: {midPoint.ToString("F3")}");
            Debug.Log($"[Calc] 원호 중심(Center): {p3_arcCenter.ToString("F3")}");
            Debug.DrawLine(p3_arcCenter, p1_arcStart, Color.magenta, 10.0f);
            Debug.DrawLine(p3_arcCenter, p2_arcEnd, Color.yellow, 10.0f);
            Debug.DrawLine(p1_arcStart, p2_arcEnd, Color.cyan, 10.0f);
            Debug.Log($"[Visual] Scene에 계산용 삼각형을 표시했습니다.");
            Debug.Log($"[Result] 삼각형 중심각: {centerAngle:F2}도, 최종 위빙 각도: {weavingAngle:F2}도");
            Debug.Log("------------------------------------");

            return weavingAngle;
        }

        private float CalculateSafeJumpHeight(Vector3 startPoint, Vector3 faceNormal, Vector3 widthDir, float widthLen, Vector3 heightDir, float heightLen, List<Collider> obstacles, PathGenerationConfig config)
        {
            float maxHeight = 0f;
            if (obstacles == null || obstacles.Count == 0) return 0.2f;

            int sampleResolution = 20;
            for (int i = 0; i <= sampleResolution; i++)
            {
                for (int j = 0; j <= sampleResolution; j++)
                {
                    float u = (float)i / sampleResolution;
                    float v = (float)j / sampleResolution;
                    Vector3 pointOnPlane = startPoint + u * widthDir * widthLen + v * heightDir * heightLen;
                    Ray ray = new Ray(pointOnPlane + faceNormal * 10f, -faceNormal);

                    if (Physics.Raycast(ray, out RaycastHit hit, 20f))
                    {
                        if (obstacles.Contains(hit.collider))
                        {
                            float height = 10f - hit.distance;
                            if (height > maxHeight) maxHeight = height;
                        }
                    }
                }
            }

            if (maxHeight > 0)
            {
                Debug.Log($"장애물 최고 높이 감지: {maxHeight:F4}m. 점프 높이를 재계산합니다.");
                return maxHeight - config.NormalOffset + 0.1f;
            }

            return 0.2f;
        }
        private BA_MotionPath GeneratePath_TrueSnakeMethod(Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
        {
            float angle_bps = 60.0f;
            // int listIndex = faceId - 1;
            // if (listIndex < 0 || listIndex >= faceNormals.Count)
            // {
            //     Debug.LogError($"GeneratePath_TrueSnakeMethod: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다. 리스트 크기: {faceNormals.Count}");
            //     return null;
            // }
            // Vector3 faceNormal = faceNormals[listIndex];
            Vector3 faceNormal = -Vector3.Cross(cube.R2 - cube.R1, cube.R4 - cube.R1).normalized;

            var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
            var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
            if (adjacentPoints.Count != 2) return null;
            Vector3 edge1 = adjacentPoints[0] - start;
            Vector3 edge2 = adjacentPoints[1] - start;
            Vector3 heightDir, widthDir;
            if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) { heightDir = edge1.normalized; } else { heightDir = edge2.normalized; }
            widthDir = Vector3.Cross(heightDir, faceNormal).normalized;
            Vector3 otherEdge = (heightDir == edge1.normalized) ? edge2 : edge1;
            if (Vector3.Dot(widthDir, otherEdge) < 0) widthDir = -widthDir;
            float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
            float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();
            Debug.Log($"[Face {faceId}] 너비 벡터: {widthDir}, 높이 벡터: {heightDir}, 너비 길이: {widthLen}, 높이 길이: {heightLen}");
            if (faceId - 1 >= faceWidthDirs.Count)
            {
                faceWidthDirs.Add(widthDir);
                faceHeightDirs.Add(heightDir);
            }
            else
            {
                faceWidthDirs[faceId - 1] = widthDir;
                faceHeightDirs[faceId - 1] = heightDir;
            }

            int gridResY = (config.VerticalStep > 0) ? Mathf.CeilToInt(heightLen / config.VerticalStep) : 1;
            if (gridResY == 0) gridResY = 1;
            float cellHeight = heightLen / gridResY;

            int gridResX = 50; 
            float cellWidth = widthLen / (gridResX > 1 ? (gridResX - 1) : 1); 

            // ▲▲▲ 수정 완료 ▲▲▲
            bool[,] isWall = new bool[gridResX, gridResY];
            bool[,] isVisited = new bool[gridResX, gridResY];
            // float cellWidth = widthLen / gridResX;
            // float cellHeight = heightLen / gridResY;

            for (int gy = 0; gy < gridResY; gy++)
            {
                for (int gx = 0; gx < gridResX; gx++)
                {
                    if (obstacles != null && obstacles.Count > 0)
                    {
                        Vector3 cellCenter = start + widthDir * (gx * cellWidth + cellWidth / 2f) + heightDir * (gy * cellHeight + cellHeight / 2f);
                        if (Physics.CheckSphere(cellCenter, Mathf.Min(cellWidth, cellHeight) / 2f, LayerMask.GetMask("Obstacle")))
                        {
                            isWall[gx, gy] = true;
                        }
                    }
                }
            }

            // [1] 핵심 웨이포인트를 기억할 리스트 생성
            var keyWaypoints = new HashSet<Vector3>();

            float safeJumpHeight = CalculateSafeJumpHeight(start, faceNormal, widthDir, widthLen, heightDir, heightLen, obstacles, config);
            var fullPath = new List<Vector3>(); // 최적화 전 전체 경로
            Vector3 normalOffsetVector = faceNormal * config.NormalOffset;

            while (true) {
                Vector2Int? nextStartCell = FindNextStartCell(isVisited, isWall, gridResX, gridResY);
                if (nextStartCell == null) break;

                if (fullPath.Count > 0) {
                    Vector3 prevEnd = fullPath.Last();
                    Vector3 nextStartPos = start + widthDir * (nextStartCell.Value.x * cellWidth) + heightDir * (nextStartCell.Value.y * cellHeight) + normalOffsetVector;
                    
                    Vector3 jumpStart = prevEnd + faceNormal * safeJumpHeight;
                    Vector3 jumpEnd = nextStartPos + faceNormal * safeJumpHeight;

                    // [2-a] 수직 점프 구간의 시작/끝 지점은 핵심 웨이포인트로 기억
                    keyWaypoints.Add(jumpStart);
                    keyWaypoints.Add(jumpEnd);

                    fullPath.Add(jumpStart);
                    fullPath.Add(jumpEnd);
                }

                Vector2Int currentCell = nextStartCell.Value;
                int xDir = 1;
                bool justMovedVertically = true; // [수정 제안] 수직 이동 직후인지 확인하는 플래그

                while(true)
                {
                    if(!isVisited[currentCell.x, currentCell.y])
                    {
                    Vector3 newPoint = start + widthDir * (currentCell.x * cellWidth + cellWidth / 2f) + heightDir * (currentCell.y * cellHeight + cellHeight / 2f) + normalOffsetVector;
                    fullPath.Add(newPoint);
                    isVisited[currentCell.x, currentCell.y] = true;

                    if(justMovedVertically)
                        {
                            keyWaypoints.Add(newPoint);
                            justMovedVertically = false;
                        }
                    }
                
                    Vector2Int nextHorizontalCell = new Vector2Int(currentCell.x + xDir, currentCell.y);
                    bool canMoveHorizontal = nextHorizontalCell.x >= 0 && nextHorizontalCell.x < gridResX && !isWall[nextHorizontalCell.x, nextHorizontalCell.y] && !isVisited[nextHorizontalCell.x, nextHorizontalCell.y];
                    
                    if (canMoveHorizontal)
                    {
                        currentCell = nextHorizontalCell;
                        continue;
                    }
                    
                    keyWaypoints.Add(fullPath.Last()); 

                    Vector2Int nextVerticalCell = new Vector2Int(currentCell.x, currentCell.y + 1);
                    bool canMoveVertical = nextVerticalCell.y < gridResY && !isWall[nextVerticalCell.x, nextVerticalCell.y] && !isVisited[nextVerticalCell.x, nextVerticalCell.y];
                    
                    if (canMoveVertical)
                    {
                        currentCell = nextVerticalCell;
                        xDir *= -1;
                        justMovedVertically = true;
                        continue;
                    }
                    break; 
                }
                // [2-d] 한 세그먼트의 마지막 지점은 핵심 웨이포인트
                keyWaypoints.Add(fullPath.Last());
            }
            
            if (fullPath.Count < 2) return null;

            // 안전지대 추가
            Vector3 entryDirection = (fullPath[0] - fullPath[1]).normalized;
            fullPath.Insert(0, fullPath[0] + entryDirection * 1.5f);
            Vector3 lastWorkPoint = fullPath.Last();
            var adjacentToEndPoints = allPoints.Where(p => p != start && p != end).ToList();
            Vector3 exitTangent;
            if (adjacentToEndPoints.Count == 2) {
                    Vector3 edgeVecAtEnd1 = (end - adjacentToEndPoints[0]).normalized;
                    Vector3 edgeVecAtEnd2 = (end - adjacentToEndPoints[1]).normalized;
                    exitTangent = (Mathf.Abs(Vector3.Dot(edgeVecAtEnd1, widthDir)) > Mathf.Abs(Vector3.Dot(edgeVecAtEnd2, widthDir))) ? edgeVecAtEnd1 : edgeVecAtEnd2;
            } else {
                exitTangent = (lastWorkPoint - fullPath[fullPath.Count-2]).normalized;
            }
            Vector3 exitSafePoint = end + exitTangent * 1.5f + normalOffsetVector;
            Vector3 liftPoint = lastWorkPoint + faceNormal * safeJumpHeight;
            Vector3 horizontalMove = exitSafePoint - lastWorkPoint;
            horizontalMove -= Vector3.Dot(horizontalMove, faceNormal) * faceNormal;
            Vector3 approachPoint = liftPoint + horizontalMove;
            fullPath.Add(liftPoint);
            fullPath.Add(approachPoint);
            fullPath.Add(exitSafePoint);

            // [3] 최종 필터링: 전체 경로에서 핵심 웨이포인트만 골라내기
            var optimizedFinalPath = fullPath.Where(p => keyWaypoints.Contains(p)).ToList();
            
            // 안전지대는 항상 포함
            optimizedFinalPath.Insert(0, fullPath.First());
            optimizedFinalPath.Add(fullPath.Last());
            // 중복 제거
            optimizedFinalPath = optimizedFinalPath.Distinct().ToList();


            // 디버깅 로그 출력
            Debug.Log($"<color=orange>--- 경로 최적화 전 (Face ID: {faceId}) ---</color>");
            Debug.Log($"총 포인트 개수: {fullPath.Count}");
            Debug.Log($"<color=cyan>--- 경로 최적화 후 (Face ID: {faceId}) ---</color>");
            Debug.Log($"총 포인트 개수: {optimizedFinalPath.Count}");

            // 최종 반환
            List<Quaternion> rotations = new List<Quaternion>();
            Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
            for (int i = 0; i < optimizedFinalPath.Count; i++) rotations.Add(targetRotation);

            return new BA_MotionPath(faceId, PathType.Snake, config.NormalOffset, optimizedFinalPath, rotations, faceNormal, start, exitSafePoint, widthDir, heightDir);
        }

        // private BA_MotionPath GeneratePath_TrueSnakeMethod(Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
        // {
        //     int listIndex = faceId - 1;
        //     if (listIndex < 0 || listIndex >= faceNormals.Count)
        //     {
        //         Debug.LogError($"GeneratePath_TrueSnakeMethod: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다. 리스트 크기: {faceNormals.Count}");
        //         return null;
        //     }
        //     Vector3 faceNormal = faceNormals[listIndex];

        //     var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
        //     var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
        //     if (adjacentPoints.Count != 2) return null;
        //     Vector3 edge1 = adjacentPoints[0] - start;
        //     Vector3 edge2 = adjacentPoints[1] - start;
        //     Vector3 heightDir, widthDir;
        //     if (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) { heightDir = edge1.normalized; } else { heightDir = edge2.normalized; }
        //     widthDir = Vector3.Cross(heightDir, faceNormal).normalized;
        //     Vector3 otherEdge = (heightDir == edge1.normalized) ? edge2 : edge1;
        //     if (Vector3.Dot(widthDir, otherEdge) < 0) widthDir = -widthDir;
        //     float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
        //     float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();

        //     if (listIndex >= faceWidthDirs.Count)
        //     {
        //         faceWidthDirs.Add(widthDir);
        //         faceHeightDirs.Add(heightDir);
        //     }
        //     else
        //     {
        //         faceWidthDirs[listIndex] = widthDir;
        //         faceHeightDirs[listIndex] = heightDir;
        //     }

        //     int gridResX = 25;
        //     int gridResY = 10;
        //     bool[,] isWall = new bool[gridResX, gridResY];
        //     bool[,] isVisited = new bool[gridResX, gridResY];
        //     float cellWidth = widthLen / gridResX;
        //     float cellHeight = heightLen / gridResY;

        //     for (int gy = 0; gy < gridResY; gy++)
        //     {
        //         for (int gx = 0; gx < gridResX; gx++)
        //         {
        //             if (obstacles != null && obstacles.Count > 0)
        //             {
        //                 Vector3 cellCenter = start + widthDir * (gx * cellWidth + cellWidth / 2f) + heightDir * (gy * cellHeight + cellHeight / 2f);
        //                 if (Physics.CheckSphere(cellCenter, Mathf.Min(cellWidth, cellHeight) / 2f, LayerMask.GetMask("Obstacle")))
        //                 {
        //                     isWall[gx, gy] = true;
        //                 }
        //             }
        //         }
        //     }

        //     float safeJumpHeight = CalculateSafeJumpHeight(start, faceNormal, widthDir, widthLen, heightDir, heightLen, obstacles, config);
        //     List<Vector3> finalPath = new List<Vector3>();
        //     Vector3 normalOffsetVector = faceNormal * config.NormalOffset;

        //     while (true)
        //     {
        //         Vector2Int? nextStartCell = FindNextStartCell(isVisited, isWall, gridResX, gridResY);
        //         if (nextStartCell == null) break;

        //         if (finalPath.Count > 0)
        //         {
        //             Vector3 prevEnd = finalPath.Last();
        //             Vector3 nextStartPos = start + widthDir * (nextStartCell.Value.x * cellWidth) + heightDir * (nextStartCell.Value.y * cellHeight) + normalOffsetVector;
        //             finalPath.Add(prevEnd + faceNormal * safeJumpHeight);
        //             finalPath.Add(nextStartPos + faceNormal * safeJumpHeight);
        //         }

        //         Vector2Int currentCell = nextStartCell.Value;
        //         int xDir = 1;

        //         while (true)
        //         {
        //             if (!isVisited[currentCell.x, currentCell.y])
        //             {
        //                 finalPath.Add(start + widthDir * (currentCell.x * cellWidth + cellWidth / 2f) + heightDir * (currentCell.y * cellHeight + cellHeight / 2f) + normalOffsetVector);
        //                 isVisited[currentCell.x, currentCell.y] = true;
        //             }
        //             Vector2Int nextHorizontalCell = new Vector2Int(currentCell.x + xDir, currentCell.y);
        //             if (nextHorizontalCell.x >= 0 && nextHorizontalCell.x < gridResX && !isWall[nextHorizontalCell.x, nextHorizontalCell.y] && !isVisited[nextHorizontalCell.x, nextHorizontalCell.y])
        //             {
        //                 currentCell = nextHorizontalCell;
        //                 continue;
        //             }
        //             Vector2Int nextVerticalCell = new Vector2Int(currentCell.x, currentCell.y + 1);
        //             if (nextVerticalCell.y < gridResY && !isWall[nextVerticalCell.x, nextVerticalCell.y] && !isVisited[nextVerticalCell.x, nextVerticalCell.y])
        //             {
        //                 currentCell = nextVerticalCell;
        //                 xDir *= -1;
        //                 continue;
        //             }
        //             break;
        //         }
        //     }

        //     if (finalPath.Count < 2) return null;

        //     Vector3 entryDirection = (finalPath[0] - finalPath[1]).normalized;
        //     finalPath.Insert(0, finalPath[0] + entryDirection * 1.5f);

        //     Vector3 lastWorkPoint = finalPath.Last();

        //     var adjacentToEndPoints = allPoints.Where(p => p != start && p != end).ToList();
        //     Vector3 exitTangent;
        //     if (adjacentToEndPoints.Count == 2)
        //     {
        //         Vector3 edgeVecAtEnd1 = (end - adjacentToEndPoints[0]).normalized;
        //         Vector3 edgeVecAtEnd2 = (end - adjacentToEndPoints[1]).normalized;
        //         exitTangent = (Mathf.Abs(Vector3.Dot(edgeVecAtEnd1, widthDir)) > Mathf.Abs(Vector3.Dot(edgeVecAtEnd2, widthDir))) ? edgeVecAtEnd1 : edgeVecAtEnd2;
        //     }
        //     else
        //     {
        //         exitTangent = (lastWorkPoint - finalPath[finalPath.Count - 2]).normalized;
        //     }

        //     Vector3 exitSafePoint = end + exitTangent * 1.5f + normalOffsetVector;
        //     Vector3 liftPoint = lastWorkPoint + faceNormal * safeJumpHeight;
        //     Vector3 horizontalMove = exitSafePoint - lastWorkPoint;
        //     horizontalMove -= Vector3.Dot(horizontalMove, faceNormal) * faceNormal;
        //     Vector3 approachPoint = liftPoint + horizontalMove;

        //     finalPath.Add(liftPoint);
        //     finalPath.Add(approachPoint);
        //     finalPath.Add(exitSafePoint);

        //     List<Quaternion> rotations = new List<Quaternion>();
        //     Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(60, heightDir) * faceNormal, heightDir);
        //     for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

        //     return new BA_MotionPath(faceId, PathType.Snake, config.NormalOffset, finalPath, rotations, faceNormal, start, exitSafePoint, widthDir, heightDir);
        // }

        private Vector2Int? FindNextStartCell(bool[,] isVisited, bool[,] isWall, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!isVisited[x, y] && !isWall[x, y])
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return null;
        }

        private bool IsFaceIntersectingObstacle(Cube face, List<Collider> obstacles)
        {
            if (obstacles == null || obstacles.Count == 0) return false;
            Vector3 center = (face.R1 + face.R2 + face.R3 + face.R4) / 4.0f;
            Vector3 halfExtents = new Vector3(Vector3.Distance(face.R1, face.R2) / 2, Vector3.Distance(face.R1, face.R4) / 2, 0.1f);
            Quaternion orientation = Quaternion.LookRotation(face.normal, (face.R4 - face.R1).normalized);
            Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);
            foreach (var hit in hits)
            {
                if (obstacles.Contains(hit)) return true;
            }
            return false;
        }

        private List<BA_MotionPath> ExecuteWithScaledObstacles(List<Collider> obstacles, float scaleFactor, Func<List<BA_MotionPath>> generationAction)
        {
            var originalBoxSizes = new Dictionary<BoxCollider, Vector3>();
            var originalSphereRadii = new Dictionary<SphereCollider, float>();
            var originalCapsuleDimensions = new Dictionary<CapsuleCollider, (float radius, float height)>();
            if (obstacles != null)
            {
                foreach (var obsCollider in obstacles)
                {
                    if (obsCollider == null) continue;
                    if (obsCollider is BoxCollider box) originalBoxSizes[box] = box.size;
                    else if (obsCollider is SphereCollider sphere) originalSphereRadii[sphere] = sphere.radius;
                    else if (obsCollider is CapsuleCollider capsule) originalCapsuleDimensions[capsule] = (capsule.radius, capsule.height);
                }
            }
            try
            {
                foreach (var pair in originalBoxSizes) { pair.Key.size = pair.Value * scaleFactor; }
                foreach (var pair in originalSphereRadii) { pair.Key.radius = pair.Value * scaleFactor; }
                foreach (var pair in originalCapsuleDimensions) { pair.Key.radius = pair.Value.radius * scaleFactor; pair.Key.height = pair.Value.height * scaleFactor; }
                Physics.SyncTransforms();
                return generationAction();
            }
            finally
            {
                foreach (var pair in originalBoxSizes) if (pair.Key != null) pair.Key.size = pair.Value;
                foreach (var pair in originalSphereRadii) if (pair.Key != null) pair.Key.radius = pair.Value;
                foreach (var pair in originalCapsuleDimensions) { if (pair.Key != null) { pair.Key.radius = pair.Value.radius; pair.Key.height = pair.Value.height; } }
                Physics.SyncTransforms();
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using static _SHI_BA.BA_Motion;


namespace _SHI_BA
{
    public struct PathGenerationConfig
    {
        public int NumHorizontalPoints;
        public float VerticalStep;
        public float NormalOffset;
        public float ObstacleOffset;
        public float ObstacleScaleFactor;

    }

    public class BA_MotionPathGenerator
    {
        public CubeStruct cuboid;

        private List<Vector3> faceNormals = new List<Vector3>();
        private List<Vector3> faceWidthDirs = new List<Vector3>();
        private List<Vector3> faceHeightDirs = new List<Vector3>();
        private Transform tcpTransform;

        public BA_MotionPathGenerator(Transform tcpTransform)
        {
            if (tcpTransform == null)
            {
                Debug.LogError("[BA_MotionPathGenerator] 생성자에 유효한 TCP Transform이 전달되지 않았습니다.");
            }
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
                if (cubes == null || cubes.Length == 0) return allPaths;

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

        // private MotionPath GenerateZigzagPathForFace(Plane_and_NV.Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
        // {

        //     // 공유 리스트의 인덱스는 0부터 시작하므로 faceId-1을 사용합니다.
        //     int listIndex = faceId - 1;
        //     if (listIndex < 0 || listIndex >= faceNormals.Count)
        //     {
        //         Debug.LogError($"GenerateZigzagPathForFace: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다. 리스트 크기: {faceNormals.Count}");
        //         return null;
        //     }
        //     Vector3 faceNormal = faceNormals[listIndex];
        //     // if (faceId == 2)
        //     // {
        //     //     Vector3 originalNormal = faceNormal; // 수정을 위해 원본 벡터를 저장합니다.

        //     //     // Y값을 0으로 만들어 XZ 평면에 투영하고 정규화합니다.
        //     //     faceNormal = new Vector3(originalNormal.x, 0, originalNormal.z).normalized;

        //     //     if (faceNormal == Vector3.zero)
        //     //     {
        //     //         faceNormal = Vector3.forward; 
        //     //     }

        //     //     // ============================ [시각화 코드 추가] ============================
        //     //     // 면의 중심점을 계산하여 레이의 시작점으로 사용합니다.
        //     //     Vector3 faceCenter = (cube.R1 + cube.R2 + cube.R3 + cube.R4) / 4.0f;
        //     //     float rayDuration = 10f; // 레이가 Scene에 표시될 시간 (초)
        //     //     float rayLength = 0.5f; // 레이의 길이

        //     //     // 1. 변경 전 Original Normal Vector를 노란색으로 표시
        //     //     Debug.DrawRay(faceCenter, originalNormal * rayLength, Color.yellow, rayDuration);
        //     //     Debug.Log($"[Face ID: {faceId}] Original Normal (노란색): {originalNormal.ToString("F3")}");

        //     //     // 2. 변경 후 Modified Normal Vector를 청록색(Cyan)으로 표시
        //     //     Debug.DrawRay(faceCenter, faceNormal * rayLength, Color.cyan, rayDuration);
        //     //     Debug.Log($"[Face ID: {faceId}] Modified Normal (청록색): {faceNormal.ToString("F3")}");
        //     //     // =======================================================================
        //     // }
        //     var allPoints = new List<Vector3> { cube.R1, cube.R2, cube.R3, cube.R4 };
        //     var adjacentPoints = allPoints.Where(p => (p - start).sqrMagnitude > 1e-6f && (p - end).sqrMagnitude > 1e-6f).ToList();
        //     if (adjacentPoints.Count != 2) return null;

        //     Vector3 edge1 = adjacentPoints[0] - start;
        //     Vector3 edge2 = adjacentPoints[1] - start;
        //     Vector3 heightDir, widthDir;

        //     if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.95f)
        //     {
        //         // 1. 사용자님의 로직: 긴 쪽 모서리를 'heightDir' 변수에 우선 할당합니다.
        //         heightDir = (edge1.magnitude > edge2.magnitude) ? edge1.normalized : edge2.normalized;

        //         // 2. 평소처럼 외적을 통해 'widthDir'를 계산합니다. (이 시점에서는 짧은 쪽 방향이 됩니다)
        //         widthDir = Vector3.Cross(heightDir, faceNormal).normalized;

        //         // ★★★ 요청하신 방향 교환(Swap) 로직 ★★★
        //         // 이제 heightDir(긴 쪽)와 widthDir(짧은 쪽)의 역할을 서로 맞바꿉니다.
        //         // 최종적으로 heightDir는 짧은 방향, widthDir는 긴 방향을 갖게 됩니다.
        //         Vector3 temp = heightDir;
        //         heightDir = widthDir;
        //         widthDir = temp;
        //         // heightDir = (edge1.magnitude < edge2.magnitude) ? edge1.normalized : edge2.normalized;

        //         // Debug.Log("<color=green>[수평면 경로 조정] heightDir(세로 스텝)와 widthDir(가로 진행)의 방향을 교환했습니다.</color>");
        //     }
        //     else
        //     {
        //         // 수평면이 아닐 경우의 기존 로직
        //         heightDir = (Mathf.Abs(Vector3.Dot(edge1.normalized, Vector3.up)) > Mathf.Abs(Vector3.Dot(edge2.normalized, Vector3.up))) ? edge1.normalized : edge2.normalized;
        //         widthDir = Vector3.Cross(heightDir, faceNormal).normalized;
        //     }

        //     widthDir = Vector3.Cross(heightDir, faceNormal).normalized;

        //     Vector3 otherEdge = (Vector3.Dot(heightDir, edge1.normalized) > 0.999f) ? edge2 : edge1;
        //     if (Vector3.Dot(widthDir, otherEdge) < 0)
        //     {
        //         widthDir = -widthDir;
        //     }
        //     Vector3 widthEdge, heightEdge;
        // if (edge1.magnitude > edge2.magnitude)
        // {
        //     widthEdge = edge1;
        //     heightEdge = edge2;
        // }
        // else
        // {
        //     widthEdge = edge2;
        //     heightEdge = edge1;
        // }
        // float heightMagnitude = heightEdge.magnitude;
        // float widthMagnitude = widthEdge.magnitude;

        // // 2. 방향(부호) 계산: 기존의 불안정한 방식을 부호 판별용으로만 사용
        // float widthDirectionSign = Mathf.Sign(Vector3.Dot(otherEdge, widthDir));

        // // heightDir 방향의 투영 값들 중 가장 큰 값의 부호를 가져옴
        // float heightProjectionMax = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();
        // float heightDirectionSign = Mathf.Sign(heightProjectionMax);
        // // 만약 Max() 결과가 0이면 부호를 1로 간주 (길이가 0이 되는 것을 방지)
        // if (heightDirectionSign == 0) heightDirectionSign = 1;

        // // 3. 최종 길이에 부호를 곱하여 할당
        // float heightLen = heightMagnitude * heightDirectionSign;
        // float widthLen = widthMagnitude * widthDirectionSign;

        // // 4. 디버깅 로그 추가
        // if (faceId == 8 || faceId == 10)
        // {
        //     Debug.Log($"<color=cyan>--- [Face ID: {faceId}] 최종 길이 계산 분석 (부호 * 크기) ---</color>");
        //     Debug.Log($"[Face {faceId}] 크기 계산 -> heightMagnitude: {heightMagnitude:F4}, widthMagnitude: {widthMagnitude:F4}");
        //     Debug.Log($"[Face {faceId}] 부호 계산 -> heightSign: {heightDirectionSign}, widthSign: {widthDirectionSign}");
        //     Debug.Log($"[Face {faceId}] 최종 길이 (부호 적용) -> heightLen: {heightLen:F4}, widthLen: {widthLen:F4}");
        //     Debug.Log("<color=cyan>----------------------------------------------------</color>");
        // }
        //     // float widthLen = Mathf.Abs(Vector3.Dot(otherEdge, widthDir));
        //     // float heightLen = allPoints.Select(p => Vector3.Dot(p - start, heightDir)).Max();
        //     // Vector3 widthEdge, heightEdge;
        //     // if (edge1.magnitude > edge2.magnitude)
        //     // {
        //     //     widthEdge = edge1;
        //     //     heightEdge = edge2;
        //     // }
        //     // else
        //     // {
        //     //     widthEdge = edge2;
        //     //     heightEdge = edge1;
        //     // }

        //     // // ★★★ 핵심 수정: 내적(Dot) 대신 벡터의 길이(magnitude)를 직접 사용 ★★★
        //     // float heightLen = heightEdge.magnitude;
        //     // float widthLen = widthEdge.magnitude;

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

        //     List<Vector3> workPoints = new List<Vector3>();
        //     Vector3 curr = start;
        //     float movedHeight = 0f;
        //     bool toRight = true;
        //     while (movedHeight <= heightLen + 1e-4f)
        //     {
        //         for (int j = 0; j < config.NumHorizontalPoints; j++)
        //         {
        //             float t = (config.NumHorizontalPoints > 1) ? (float)j / (config.NumHorizontalPoints - 1) : 0;
        //             workPoints.Add(curr + widthDir * widthLen * (toRight ? t : (1 - t)));
        //         }
        //         if (movedHeight >= heightLen) break;
        //         movedHeight += config.VerticalStep;
        //         if (movedHeight > heightLen) movedHeight = heightLen;
        //         curr = start + heightDir * movedHeight;
        //         toRight = !toRight;
        //     }
        //     // while (movedHeight <= heightLen + 1e-4f)
        //     // {
        //     //     // 한 줄의 수평 경로 생성
        //     //     for (int j = 0; j < config.NumHorizontalPoints; j++)
        //     //     {
        //     //         float t = (config.NumHorizontalPoints > 1) ? (float)j / (config.NumHorizontalPoints - 1) : 0;
        //     //         workPoints.Add(curr + widthDir * widthLen * (toRight ? t : (1 - t)));
        //     //     }

        //     //     // === [핵심 수정 로직] ===
        //     //     // 현재 줄을 생성한 후, 남은 전체 높이를 계산합니다.
        //     //     float remainingHeight = heightLen - movedHeight;

        //     //     // 만약 남은 높이가 0.08f 이하라면, 추가적인 줄을 만들지 않고 루프를 즉시 종료합니다.
        //     //     if (remainingHeight <= 0.09f)
        //     //     {
        //     //         Debug.Log($"[Path Gen] 남은 높이({remainingHeight:F4}m)가 0.08m 이하이므로 경로 생성을 중단합니다.");
        //     //         break;
        //     //     }
        //     //     // ========================

        //     //     // 다음 줄로 이동 준비
        //     //     movedHeight += config.VerticalStep;
        //     //     if (movedHeight > heightLen) movedHeight = heightLen;
        //     //     curr = start + heightDir * movedHeight;
        //     //     toRight = !toRight;
        //     // }
        //     List<Vector3> finalPath = workPoints.Select(p => p + faceNormal * config.NormalOffset).ToList();
        //     if (finalPath.Count < 2) return null;

        //     float safeDistance = (listIndex >= 0 && listIndex <= 5) ? 0.2f : 1.7f;
        //     finalPath.Insert(0, finalPath[0] + (finalPath[0] - finalPath[1]).normalized * safeDistance);
        //     finalPath.Add(finalPath.Last() + (finalPath.Last() - finalPath[finalPath.Count - 2]).normalized * safeDistance);
        //     if (faceId == 8)
        // {
        //     Vector3 actualStartPoint = finalPath.Count > 1 ? finalPath[1] : Vector3.zero;
        //     Vector3 actualEndPoint = finalPath.Count > 2 ? finalPath[finalPath.Count - 2] : Vector3.zero;

        //     Debug.Log($"<color=red>--- [Face ID: 8] 최종 경로 위치 분석 ---</color>");
        //     Debug.Log($"[Face 8] R1~R4 좌표: R1={cube.R1.ToString("F4")}, R2={cube.R2.ToString("F4")}, R3={cube.R3.ToString("F4")}, R4={cube.R4.ToString("F4")}");
        //     Debug.Log($"<color=yellow>[Face 8] 경로 시작점 (finalPath[1]): {actualStartPoint.ToString("F4")}</color>");
        //     Debug.Log($"<color=yellow>[Face 8] 경로 끝점 (finalPath[^2]): {actualEndPoint.ToString("F4")}</color>");
        //     Debug.Log($"-------------------------------------------------");
        // }
        //     List<Quaternion> rotations = new List<Quaternion>();
        //     // int angle_bps = (faceId == 8) ? 120 : 60;/////////////
        //     // Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
        //     Quaternion targetRotation; // targetRotation 변수를 먼저 선언

        //     int angle_bps;
        //     if (faceId == 8)
        //     {
        //         // cuboid 변수가 할당되었는지 확인
        //         if (this.cuboid == null)
        //         {
        //             Debug.LogError("Cuboid(CubeStruct) 참조가 MotionPathGenerator에 설정되지 않았습니다! angle_bps의 기본값으로 60을 사용합니다.");
        //             angle_bps = 60; // cuboid가 없을 경우의 기본값
        //         }
        //         else
        //         {
        //             // GetCenterPoint()를 호출하고 z값을 비교하여 angle_bps 결정
        //             Vector3 centerPoint = this.cuboid.GetCenterPoint();
        //             angle_bps = (start.z > centerPoint.z) ? 60 : 120;
        //             Debug.Log($"[Face 8] start.z: {start.z:F3}, center.z: {centerPoint.z:F3} -> angle_bps: {angle_bps}");
        //         }
        //     }
        //     else
        //     {
        //         // faceId가 8이 아닌 경우, 기존처럼 60을 사용합니다.
        //         angle_bps = 60;
        //     }

        //     // faceId가 2일 때의 특별 로직은 그대로 유지합니다.
        //     if (faceId == 2)
        //     {
        //         Vector3 initialForwardOnXZ = new Vector3(faceNormal.x, 0, faceNormal.z).normalized;
        //         if (initialForwardOnXZ == Vector3.zero)
        //         {
        //             initialForwardOnXZ = Vector3.forward;
        //         }
        //         Quaternion rotationAroundY = Quaternion.AngleAxis(60, Vector3.up); // faceId 2는 60도 고정
        //         Vector3 finalForward = rotationAroundY * initialForwardOnXZ;
        //         targetRotation = Quaternion.LookRotation(finalForward, Vector3.up);
        //     }
        //     else
        //     {
        //         // 위에서 계산된 angle_bps 값을 사용하여 회전값 계산
        //         targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle_bps, heightDir) * faceNormal, heightDir);
        //     }
        //     for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

        //     return new MotionPath(faceId, PathType.Zigzag, config.NormalOffset, finalPath, rotations, faceNormal, start, finalPath[finalPath.Count - 2], widthDir, heightDir);
        // }
        private BA_MotionPath GenerateZigzagPathForFace(Cube cube, Vector3 start, Vector3 end, PathGenerationConfig config, int faceId, List<Collider> obstacles)
        {
            // 함수 시작 시 디버깅 로그 (8번, 10번 면 대상)

            int listIndex = faceId - 1;
            if (listIndex < 0 || listIndex >= faceNormals.Count)
            {
                Debug.LogError($"GenerateZigzagPathForFace: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다.");
                return null;
            }
            Vector3 faceNormal = faceNormals[listIndex];

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

            float safeDistance = (listIndex >= 0 && listIndex <= 5) ? 0.2f : 1.7f;
            finalPath.Insert(0, finalPath[0] + (finalPath[0] - finalPath[1]).normalized * safeDistance);
            finalPath.Add(finalPath.Last() + (finalPath.Last() - finalPath[finalPath.Count - 2]).normalized * safeDistance);

            List<Quaternion> rotations = new List<Quaternion>();
            Quaternion targetRotation;

            int angle_bps;
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
            int listIndex = faceId - 1;
            if (listIndex < 0 || listIndex >= faceNormals.Count)
            {
                Debug.LogError($"GeneratePath_TrueSnakeMethod: 잘못된 인덱스({listIndex})로 faceNormals에 접근하려고 합니다. 리스트 크기: {faceNormals.Count}");
                return null;
            }
            Vector3 faceNormal = faceNormals[listIndex];

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

            if (listIndex >= faceWidthDirs.Count)
            {
                faceWidthDirs.Add(widthDir);
                faceHeightDirs.Add(heightDir);
            }
            else
            {
                faceWidthDirs[listIndex] = widthDir;
                faceHeightDirs[listIndex] = heightDir;
            }

            int gridResX = 25;
            int gridResY = 10;
            bool[,] isWall = new bool[gridResX, gridResY];
            bool[,] isVisited = new bool[gridResX, gridResY];
            float cellWidth = widthLen / gridResX;
            float cellHeight = heightLen / gridResY;

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

            float safeJumpHeight = CalculateSafeJumpHeight(start, faceNormal, widthDir, widthLen, heightDir, heightLen, obstacles, config);
            List<Vector3> finalPath = new List<Vector3>();
            Vector3 normalOffsetVector = faceNormal * config.NormalOffset;

            while (true)
            {
                Vector2Int? nextStartCell = FindNextStartCell(isVisited, isWall, gridResX, gridResY);
                if (nextStartCell == null) break;

                if (finalPath.Count > 0)
                {
                    Vector3 prevEnd = finalPath.Last();
                    Vector3 nextStartPos = start + widthDir * (nextStartCell.Value.x * cellWidth) + heightDir * (nextStartCell.Value.y * cellHeight) + normalOffsetVector;
                    finalPath.Add(prevEnd + faceNormal * safeJumpHeight);
                    finalPath.Add(nextStartPos + faceNormal * safeJumpHeight);
                }

                Vector2Int currentCell = nextStartCell.Value;
                int xDir = 1;

                while (true)
                {
                    if (!isVisited[currentCell.x, currentCell.y])
                    {
                        finalPath.Add(start + widthDir * (currentCell.x * cellWidth + cellWidth / 2f) + heightDir * (currentCell.y * cellHeight + cellHeight / 2f) + normalOffsetVector);
                        isVisited[currentCell.x, currentCell.y] = true;
                    }
                    Vector2Int nextHorizontalCell = new Vector2Int(currentCell.x + xDir, currentCell.y);
                    if (nextHorizontalCell.x >= 0 && nextHorizontalCell.x < gridResX && !isWall[nextHorizontalCell.x, nextHorizontalCell.y] && !isVisited[nextHorizontalCell.x, nextHorizontalCell.y])
                    {
                        currentCell = nextHorizontalCell;
                        continue;
                    }
                    Vector2Int nextVerticalCell = new Vector2Int(currentCell.x, currentCell.y + 1);
                    if (nextVerticalCell.y < gridResY && !isWall[nextVerticalCell.x, nextVerticalCell.y] && !isVisited[nextVerticalCell.x, nextVerticalCell.y])
                    {
                        currentCell = nextVerticalCell;
                        xDir *= -1;
                        continue;
                    }
                    break;
                }
            }

            if (finalPath.Count < 2) return null;

            Vector3 entryDirection = (finalPath[0] - finalPath[1]).normalized;
            finalPath.Insert(0, finalPath[0] + entryDirection * 1.5f);

            Vector3 lastWorkPoint = finalPath.Last();

            var adjacentToEndPoints = allPoints.Where(p => p != start && p != end).ToList();
            Vector3 exitTangent;
            if (adjacentToEndPoints.Count == 2)
            {
                Vector3 edgeVecAtEnd1 = (end - adjacentToEndPoints[0]).normalized;
                Vector3 edgeVecAtEnd2 = (end - adjacentToEndPoints[1]).normalized;
                exitTangent = (Mathf.Abs(Vector3.Dot(edgeVecAtEnd1, widthDir)) > Mathf.Abs(Vector3.Dot(edgeVecAtEnd2, widthDir))) ? edgeVecAtEnd1 : edgeVecAtEnd2;
            }
            else
            {
                exitTangent = (lastWorkPoint - finalPath[finalPath.Count - 2]).normalized;
            }

            Vector3 exitSafePoint = end + exitTangent * 1.5f + normalOffsetVector;
            Vector3 liftPoint = lastWorkPoint + faceNormal * safeJumpHeight;
            Vector3 horizontalMove = exitSafePoint - lastWorkPoint;
            horizontalMove -= Vector3.Dot(horizontalMove, faceNormal) * faceNormal;
            Vector3 approachPoint = liftPoint + horizontalMove;

            finalPath.Add(liftPoint);
            finalPath.Add(approachPoint);
            finalPath.Add(exitSafePoint);

            List<Quaternion> rotations = new List<Quaternion>();
            Quaternion targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(60, heightDir) * faceNormal, heightDir);
            for (int i = 0; i < finalPath.Count; i++) rotations.Add(targetRotation);

            return new BA_MotionPath(faceId, PathType.Snake, config.NormalOffset, finalPath, rotations, faceNormal, start, exitSafePoint, widthDir, heightDir);
        }

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
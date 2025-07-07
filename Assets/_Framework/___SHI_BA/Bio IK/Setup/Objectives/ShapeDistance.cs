// using UnityEngine;

// namespace BioIK
// {
//     // 이 목표는 세그먼트와 지정된 콜라이더 표면 사이의 거리를 유지하려고 시도합니다.
//     // 충돌 회피와 같은 기능을 구현하는 데 사용할 수 있습니다.
//     [AddComponentMenu("BioIK/Shape Distance")]
//     public class ShapeDistance : BioObjective
//     {
//         public bool IsEnabled = true;
//         [SerializeField] public Collider TargetCollider;
//         [SerializeField] public double SafetyMargin = 0.01; // 기본값을 0.01 (1cm) 정도로 작게 설정

//         // [NEW] 이 거리 안에 들어왔을 때만 회피 기능이 활성화됩니다.
//         [SerializeField] public double MaximumDistance = 0.5; // 50cm 안에 들어오면 회피 시작

//         public override ObjectiveType GetObjectiveType()
//         {
//             // 이전에 수정한 대로 ObjectiveType.ShapeDistance를 반환하는지 확인합니다.
//             return ObjectiveType.ShapeDistance;
//         }

//         public override void UpdateData()
//         {
//             // 이 목표는 IK 계산 중에 실시간으로 위치를 계산하므로,
//             // 매 프레임 데이터를 미리 업데이트할 필요는 없습니다.
//         }

//         public override double ComputeLoss(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, double[] configuration)
//         {
//             if (TargetCollider == null)
//             {
//                 return 0.0;
//             }

//             Vector3 currentSegmentPosition = new Vector3((float)WPX, (float)WPY, (float)WPZ);
//             Vector3 closestPointOnCollider = TargetCollider.ClosestPoint(currentSegmentPosition);
//             double realDistance = Vector3.Distance(currentSegmentPosition, closestPointOnCollider);

//             // [MODIFIED] 최대 인식 거리(MaximumDistance)보다 멀리 있으면 페널티를 0으로 처리합니다.
//             if (realDistance > MaximumDistance)
//             {
//                 return 0.0;
//             }

//             // [MODIFIED] 안전 마진을 고려한 유효 거리를 계산합니다.
//             double effectiveDistance = realDistance - SafetyMargin;

//             // [MODIFIED] 개선된 손실 함수 로직
//             if (effectiveDistance <= 0.0)
//             {
//                 // 안전 마진 안으로 들어왔거나 충돌한 경우, 여전히 매우 큰 페널티를 부과합니다.
//                 // Weight 값을 곱해 중요도를 반영합니다.
//                 return Weight * 1000.0; // 이전의 무한대 값 대신 큰 상수를 사용하여 안정성을 높일 수 있습니다.
//             }
//             else
//             {
//                 // 거리가 가까울수록 0에서 1까지 부드럽게 증가하는 페널티를 계산합니다.
//                 // (1 - 현재거리/최대거리) 형태는 거리가 멀수록 페널티가 0에 가깝고, 가까울수록 1에 가까워집니다.
//                 double normalizedDistance = effectiveDistance / (MaximumDistance - SafetyMargin);
//                 double penalty = (1.0 - normalizedDistance);

//                 // 최종 손실은 (페널티^2) * Weight 입니다. 제곱을 통해 가까울수록 페널티를 더 강하게 만듭니다.
//                 return Weight * penalty * penalty;
//             }
//         }

//         public override bool CheckConvergence(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, double[] configuration)
//         {
//             if (TargetCollider == null)
//             {
//                 return true;
//             }

//             Vector3 currentSegmentPosition = new Vector3((float)WPX, (float)WPY, (float)WPZ);
//             Vector3 closestPointOnCollider = TargetCollider.ClosestPoint(currentSegmentPosition);
//             double distanceToShape = Vector3.Distance(currentSegmentPosition, closestPointOnCollider);

//             return distanceToShape >= SafetyMargin;
//         }

//         public override double ComputeValue(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, double[] configuration)
//         {
//              if (TargetCollider == null)
//             {
//                 return 0.0;
//             }
//             Vector3 currentSegmentPosition = new Vector3((float)WPX, (float)WPY, (float)WPZ);
//             Vector3 closestPointOnCollider = TargetCollider.ClosestPoint(currentSegmentPosition);
//             return Vector3.Distance(currentSegmentPosition, closestPointOnCollider);
//         }
//     }
// }

using BioIK;
using UnityEngine;

namespace _SHI_BA
{
    // [AddComponentMenu("BioIK/Shape Distance")]
    public class ShapeDistance : BioIK.BioObjective
    {
        // 이 Objective의 활성화 여부를 외부에서 제어합니다.
        public bool IsEnabled = true;

        [SerializeField] public Collider TargetCollider;
        [SerializeField] public double SafetyMargin = 0.01;
        [SerializeField] public double MaximumDistance = 0.5;

        protected override void OnDestroy()
        {
            if (Segment != null && Segment.Character != null)
            {
                // Segment의 Objectives 배열에서 자기 자신을 찾아 제거합니다.
                for (int i = 0; i < Segment.Objectives.Length; i++)
                {
                    if (Segment.Objectives[i] == this)
                    {
                        // 배열에서 해당 요소를 제거하기 위한 간단한 리스트 변환 방식
                        var objectiveList = new System.Collections.Generic.List<BioObjective>(Segment.Objectives);
                        objectiveList.RemoveAt(i);
                        Segment.Objectives = objectiveList.ToArray();
                        break;
                    }
                }

                // 가장 중요한 단계: IK 시스템 전체를 갱신하여 변경사항을 반영합니다.
                if (Application.isPlaying)
                { // 플레이 중일 때만 Refresh를 호출하여 에디터 오류 방지
                    Segment.Character.Refresh();
                }
            }
        }

        public override ObjectiveType GetObjectiveType()
        {
            return ObjectiveType.ShapeDistance;
        }

        public override void UpdateData() {
            // No per-frame data update needed.
        }

        public override double ComputeLoss(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, double[] configuration)
        {
            if (!IsEnabled || TargetCollider == null)
            {
                return 0.0;
            }

            Vector3 currentSegmentPosition = new Vector3((float)WPX, (float)WPY, (float)WPZ);
            Vector3 closestPointOnCollider = TargetCollider.ClosestPoint(currentSegmentPosition);
            double realDistance = Vector3.Distance(currentSegmentPosition, closestPointOnCollider);

            if (realDistance > MaximumDistance)
            {
                return 0.0;
            }

            double effectiveDistance = realDistance - SafetyMargin;

            if (effectiveDistance <= 0.0)
            {
                return Weight * 1000.0;
            }
            else
            {
                double normalizedDistance = effectiveDistance / (MaximumDistance - SafetyMargin);
                double penalty = (1.0 - normalizedDistance);
                return Weight * penalty * penalty;
            }
        }

        public override bool CheckConvergence(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, double[] configuration)
        {
            if (!IsEnabled || TargetCollider == null)
            {
                return true;
            }

            Vector3 currentSegmentPosition = new Vector3((float)WPX, (float)WPY, (float)WPZ);
            Vector3 closestPointOnCollider = TargetCollider.ClosestPoint(currentSegmentPosition);
            double distanceToShape = Vector3.Distance(currentSegmentPosition, closestPointOnCollider);

            return distanceToShape >= SafetyMargin;
        }

        public override double ComputeValue(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, double[] configuration)
        {
            if (!IsEnabled || TargetCollider == null)
            {
                return 0.0;
            }
            Vector3 currentSegmentPosition = new Vector3((float)WPX, (float)WPY, (float)WPZ);
            Vector3 closestPointOnCollider = TargetCollider.ClosestPoint(currentSegmentPosition);
            return Vector3.Distance(currentSegmentPosition, closestPointOnCollider);
        }


    }
}
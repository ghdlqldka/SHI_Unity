using BioIK;
using UnityEngine;

namespace _SHI_BA
{

	// [AddComponentMenu("")]
	public class BA_BioJoint : BioIK._BioJoint
    {
        public class BA_Motion : BioIK._BioJoint._Motion
        {
            public BA_Motion(BA_BioJoint joint, Vector3 axis) : base()
            {
                Joint = joint;
                Axis = axis;
            }

            protected override void UpdateRealistic()
            {
                if (Time.deltaTime == 0f)
                {
                    return;
                }

                // 1. BioIK 컴포넌트에서 전역 최대 속도와 가속도 값을 가져옵니다.
                double maxVel = Joint.JointType == JointType.Rotational ? Utility.Rad2Deg * Joint.Segment.Character.MaximumVelocity : Joint.Segment.Character.MaximumVelocity;
                double maxAcc = Joint.JointType == JointType.Rotational ? Utility.Rad2Deg * Joint.Segment.Character.MaximumAcceleration : Joint.Segment.Character.MaximumAcceleration;

                // =================================================================================
                // ▼▼▼ [핵심 수정] 속도 조절 로직을 메소드 최상단으로 이동합니다. ▼▼▼
                // =================================================================================
                // 만약 현재 처리 중인 관절의 이름이 "x" 라면,
                if (Joint.gameObject.name == "x")
                {
                    // 이 메소드 내에서 사용할 최대 속도(maxVel) 변수 자체의 값을 1/10로 줄입니다.
                    maxVel /= 10.0;
                }
                // =================================================================================
                // ▲▲▲ 이렇게 하면 아래의 모든 로직은 변경된 maxVel 값을 사용하게 됩니다. ▲▲▲
                // =================================================================================

                // 현재 목표치와의 오차 계산
                CurrentError = TargetValue - CurrentValue;

                // maxAcc 값(전역 설정)에 따라 로직 분기
                if (maxAcc > 0.0)
                {
                    // [가속/감속 로직] 이제 이 로직도 'x' 관절일 경우 1/10로 줄어든 maxVel의 영향을 받습니다.

                    double stoppingDistance =
                        System.Math.Abs((CurrentVelocity * CurrentVelocity) / (2.0 * maxAcc * Slowdown))
                        + System.Math.Abs(CurrentAcceleration) / 2.0 * Time.deltaTime * Time.deltaTime
                        + System.Math.Abs(CurrentVelocity) * Time.deltaTime;

                    if (System.Math.Abs(CurrentError) > stoppingDistance)
                    {
                        CurrentAcceleration = System.Math.Sign(CurrentError) * System.Math.Min(System.Math.Abs(CurrentError) / Time.deltaTime, maxAcc * Speedup);
                    }
                    else
                    {
                        if (CurrentError == 0.0)
                        {
                            CurrentAcceleration = -System.Math.Sign(CurrentVelocity) *
                            System.Math.Min(System.Math.Abs(CurrentVelocity) / Time.deltaTime, maxAcc);

                        }
                        else
                        {
                            CurrentAcceleration = -System.Math.Sign(CurrentVelocity) *
                            System.Math.Min(
                                System.Math.Min(System.Math.Abs(CurrentVelocity) / Time.deltaTime, maxAcc),
                                System.Math.Abs((CurrentVelocity * CurrentVelocity) / (2.0 * CurrentError))
                            );
                        }
                    }
                    CurrentVelocity += CurrentAcceleration * Time.deltaTime;
                }
                else
                {
                    // [등속 운동 로직] 이 로직 역시 'x' 관절일 경우 1/10로 줄어든 maxVel의 영향을 받습니다.
                    CurrentAcceleration = 0.0;

                    double tolerance = maxVel * Time.deltaTime;
                    if (System.Math.Abs(CurrentError) <= tolerance)
                    {
                        CurrentValue = TargetValue;
                        CurrentVelocity = 0.0;
                    }
                    else
                    {
                        CurrentVelocity = System.Math.Sign(CurrentError) * maxVel;
                    }
                }

                // 최종 속도 제한. 'x' 관절은 이미 줄어든 maxVel 값으로 제한됩니다.
                if (CurrentVelocity > maxVel)
                {
                    CurrentVelocity = maxVel;
                }
                if (CurrentVelocity < -maxVel)
                {
                    CurrentVelocity = -maxVel;
                }

                // 최종적으로 계산된 속도를 현재 값에 적용
                CurrentValue += CurrentVelocity * Time.deltaTime;
            }
        }

        protected override void Awake() {
            // Debug.LogError("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@=> " + this.gameObject.name);
        }


    }

}
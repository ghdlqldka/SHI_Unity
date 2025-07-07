using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using BioIK;
using _SHI_BA;

[System.Serializable]
public class JointLimit
{
    public float lower;
    public float upper;

    public JointLimit(float lower, float upper)
    {
        this.lower = lower;
        this.upper = upper;
    }
}

[System.Serializable]
public class RobotJointLimits
{
    public Dictionary<string, JointLimit> jointLimits = new Dictionary<string, JointLimit>();

    public RobotJointLimits(Dictionary<string, JointLimit> limits)
    {
        jointLimits = limits;
    }
}

public static class RobotPoses
{
    // Presets 딕셔너리는 변경 없이 그대로 유지합니다.
    public static readonly Dictionary<string, RobotJointLimits> Presets = new Dictionary<string, RobotJointLimits>
    {
        { "home", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(30f, 30f) },
                { "A3", new JointLimit(-30f, -30f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-90f, 90f) },
                { "A6", new JointLimit(-90f, 90f) }
            })
        },
        { "curved", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(20f, 20f) },
                { "A3", new JointLimit(20f, 20f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-90f, 90f) },
                { "A6", new JointLimit(-120f, 120f) }
            })
        },
        { "weaving1", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(20f, 20f) },
                { "A3", new JointLimit(0f, 0f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-90f, 90f) },
                { "A6", new JointLimit(-120f, 120f) }
            })
        },
        { "ready_front", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(60f, 60f) },
                { "A3", new JointLimit(-60f, -60f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-60f, 60f) },
                { "A6", new JointLimit(-90f, 90f) }
            })
        },
        { "ready_diag", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(70f, 80f) },
                { "A3", new JointLimit(-30f, -20f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-60f, 60f) },
                { "A6", new JointLimit(-90f, 90f) }
            })
        },
        { "ready_top", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(-80f, -77f) },
                { "A3", new JointLimit(90f, 90f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-60f, 60f) },
                { "A6", new JointLimit(-90f, 90f) }
            })
        },
        { "ready_top2", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(-90f, -90f) },
                { "A1", new JointLimit(90f, 90f) },
                { "A2", new JointLimit(0f, 0f) },
                { "A3", new JointLimit(60f, 60f) },
                { "A4", new JointLimit(-180f, 180f) },
                { "A5", new JointLimit(-125f, 125f) },
                { "A6", new JointLimit(-90f, 90f) }
            })
        },
        { "ready_deep", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-150f, -30f) },
                { "A2", new JointLimit(-90f, -90f) },
                { "A3", new JointLimit(90f, 90f) },
                { "A4", new JointLimit(-120f, 120f) },
                { "A5", new JointLimit(-60f, 60f) },
                { "A6", new JointLimit(-90f, 90f) }
            })
        },
        {"FreePose", new RobotJointLimits(new Dictionary<string, JointLimit>
            {
                { "down", new JointLimit(0f, 0f) },
                { "A1", new JointLimit(-185f, 185f) },
                { "A2", new JointLimit(-150f, 85f) },
                { "A3", new JointLimit(-75f, 210f) },
                { "A4", new JointLimit(-180f, 180f) },
                { "A5", new JointLimit(-125f, 125f) },
                { "A6", new JointLimit(-180f, 180f) }
            })
        }
    };
}

public class RobotStartPose : MonoBehaviour
{
    [SerializeField]
    protected BA_BioIK bioIK;
    [SerializeField]
    protected string poseName = "ready_front";
    private List<JointValue> _activeJointValues = new List<JointValue>();

    //protected void Awake()
    //{
    //    Debug.Assert(bioIK != null);
    //}

    public void ApplyPoseWithJointValue(RobotJointLimits limits)
    {
        RemoveAllJointValueObjectives();
        bioIK.Refresh();
        
        var jointAxisMap = new Dictionary<string, string> 
        {
            { "A1", "Z" }, { "A2", "Y" }, { "A3", "Y" },
            { "A4", "X" }, { "A5", "Y" }, { "A6", "X" }
        };

        // --- [핵심 수정 1] 관절 한계를 먼저 최대로 풀어줍니다 ---
        // Debug.LogWarning("[RobotStartPose] 관절 이동을 위해 모든 조인트의 Limit를 일시적으로 최대로 설정합니다.");
        foreach (var seg in bioIK.Segments)
        {
            if (seg.Joint == null || !jointAxisMap.ContainsKey(seg.Joint.gameObject.name))
                continue;
            
            const float wideLimit = 1000f; 
            if (seg.Joint.X != null && seg.Joint.X.IsEnabled()) 
            {
                seg.Joint.X.LowerLimit = -wideLimit;
                seg.Joint.X.UpperLimit = wideLimit;
            }
            if (seg.Joint.Y != null && seg.Joint.Y.IsEnabled()) 
            {
                seg.Joint.Y.LowerLimit = -wideLimit;
                seg.Joint.Y.UpperLimit = wideLimit;
            }
            if (seg.Joint.Z != null && seg.Joint.Z.IsEnabled()) 
            {
                seg.Joint.Z.LowerLimit = -wideLimit;
                seg.Joint.Z.UpperLimit = wideLimit;
            }
        }

        // --- [핵심 수정 2] JointValue Objective 설정 ---
        foreach (var seg in bioIK.Segments)
        {
            if (seg.Joint == null || !seg.Joint.enabled) 
                continue;
            string jointName = seg.Joint.gameObject.name;
            if (!jointAxisMap.ContainsKey(jointName) || !limits.jointLimits.ContainsKey(jointName)) 
                continue;
            
            var limit = limits.jointLimits[jointName];
            float targetValue = Mathf.Approximately(limit.lower, limit.upper) ? limit.lower : (limit.lower + limit.upper) * 0.5f;
            string axis = jointAxisMap[jointName];
            JointValue jv = null;
            
            switch (axis)
            {
                case "X":
                    if (seg.Joint.X != null && seg.Joint.X.IsEnabled())
                    {
                        seg.Joint.X.SetTargetValue(targetValue);
                        jv = seg.AddObjective(ObjectiveType.JointValue) as JointValue;
                        if (jv != null)
                        { 
                            jv.SetXMotion(true); 
                            jv.SetYMotion(false); 
                            jv.SetZMotion(false);
                        }
                    }
                    break;
                case "Y":
                    if (seg.Joint.Y != null && seg.Joint.Y.IsEnabled())
                    {
                        seg.Joint.Y.SetTargetValue(targetValue);
                        jv = seg.AddObjective(ObjectiveType.JointValue) as JointValue;
                        if (jv != null)
                        { 
                            jv.SetXMotion(false); 
                            jv.SetYMotion(true); 
                            jv.SetZMotion(false);
                        }
                    }
                    break;
                case "Z":
                    if (seg.Joint.Z != null && seg.Joint.Z.IsEnabled()) 
                    {
                        seg.Joint.Z.SetTargetValue(targetValue);
                        jv = seg.AddObjective(ObjectiveType.JointValue) as JointValue;
                        if (jv != null)
                        { 
                            jv.SetXMotion(false); 
                            jv.SetYMotion(false); 
                            jv.SetZMotion(true);
                        }
                    }
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            if (jv != null) 
            {
                jv.SetTargetValue(targetValue);
                jv.Weight = 100.0f;
                _activeJointValues.Add(jv);
            }
        }

        bioIK.Refresh();
        bioIK.autoIK = true;
    }

    public void ReapplyJointLimits(string poseName)
    {
        if (RobotPoses.Presets.ContainsKey(poseName) == false)
        {
            Debug.LogError($"[RobotStartPose] '{poseName}' preset을 찾을 수 없어 Limit을 재적용할 수 없습니다.");
            return;
        }
        
        var limits = RobotPoses.Presets[poseName].jointLimits;
        // Debug.Log($"[RobotStartPose] '{poseName}' 프리셋의 관절 Limit을 다시 적용합니다.");

        foreach (var seg in bioIK.Segments)
        {
            if (seg.Joint == null || !limits.ContainsKey(seg.Joint.gameObject.name))
                continue;
            
            string jointName = seg.Joint.gameObject.name;
            var limit = limits[jointName];

            if (seg.Joint.X != null && seg.Joint.X.IsEnabled()) 
            {
                seg.Joint.X.LowerLimit = limit.lower;
                seg.Joint.X.UpperLimit = limit.upper;
            }
            if (seg.Joint.Y != null && seg.Joint.Y.IsEnabled()) 
            {
                seg.Joint.Y.LowerLimit = limit.lower;
                seg.Joint.Y.UpperLimit = limit.upper;
            }
            if (seg.Joint.Z != null && seg.Joint.Z.IsEnabled()) 
            {
                seg.Joint.Z.LowerLimit = limit.lower;
                seg.Joint.Z.UpperLimit = limit.upper;
            }
            // Debug.Log($"  - Joint '{jointName}' Limit 재적용: [{limit.lower}, {limit.upper}]");
        }
        bioIK.Refresh(); // 변경사항 적용
    }

    // [수정] PosingSequenceCoroutine에서 Limit 재적용 및 Objective 제거 로직을 포함
    public IEnumerator PosingSequenceCoroutine(string poseName, Vector3? faceNormal = null)
    {
        // Debug.Log($"[RobotStartPose] PosingSequenceCoroutine 시작: {poseName}");
        // 1. 목표 자세 설정 (Limit은 일시적으로 해제됨)
        ApplyPoseByName(poseName);

        // [핵심 수정] IK Solver가 최소 한 프레임 실행될 시간을 보장하기 위해 yield return null을 먼저 호출합니다.
        yield return null; 

        // 2. 목표 자세에 도달할 때까지 대기
        float timeout = 10.0f;
        float timer = 0f;
        // bool poseReached = false;
        // Debug.Log($"[RobotStartPose] IK가 자세를 잡도록 최대 {timeout}초간 대기합니다 (Limit 해제 상태).");

        while (timer < timeout)
        {
            if (IsPoseReached())
            {
                // poseReached = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 3. 자세 도달 후, 원래의 Joint Limit을 다시 적용합니다.
        ReapplyJointLimits(poseName);
        
        // 4. 자세 잡기에 사용된 JointValue Objective들을 제거합니다.
        RemoveAllJointValueObjectives();
    }

    public bool IsPoseReached(float thresholdDeg = 5.0f)
    {
        foreach (var jv in _activeJointValues)
        {
            if (jv == null) 
                continue;
            var motion = ((_JointValue)jv).GetMotion();
            if (motion == null)
                continue;

            float current = 0f;
            // string axis = "";
            if (motion.Joint.X != null && jv.IsXMotion())
            {
                current = (float)motion.Joint.X.GetCurrentValue();
                // axis = "X";
            }
            else if (motion.Joint.Y != null && jv.IsYMotion())
            {
                current = (float)motion.Joint.Y.GetCurrentValue();
                // axis = "Y";
            }
            else if (motion.Joint.Z != null && jv.IsZMotion())
            {
                current = (float)motion.Joint.Z.GetCurrentValue();
                // axis = "Z";
            }
            else
            {
                continue;
            }

            float target = (float)jv.GetTargetValue();
            if (Mathf.Abs(current - target) > thresholdDeg)
            {
                // Debug.Log($"[RobotStartPose] {motion.Joint.gameObject.name}({axis}) 도달 전: 현재={current}, 목표={target}");
                return false;
            }
        }
        // Debug.Log("[RobotStartPose] 모든 JointValue 목표값 도달!");
        return true;
    }

    public void RemoveAllJointValueObjectives()
    {
        // Iterate through the active JointValue objectives that need to be removed.
        foreach (var jv in _activeJointValues)
        {
            if (jv != null && jv.Segment != null)
            {
                // Manually find and remove the objective from its segment's list.
                var objectivesList = new List<BioObjective>(jv.Segment.Objectives);
                if (objectivesList.Remove(jv))
                {
                    jv.Segment.Objectives = objectivesList.ToArray();
                }

                // Use Destroy instead of DestroyImmediate. Destroy is safer during
                // play mode as it removes the object at the end of the frame.
                Destroy(jv);
            }
        }

        // Clear your tracking list.
        _activeJointValues.Clear();

        // After all references are cleared and objects are queued for destruction,
        // refresh the BioIK system once.
        if (bioIK != null)
        {
            bioIK.Refresh();
        }
    }

    public void ApplyPoseByName(string poseName)
    {
        Debug.Log("@@@@@@@@ApplyPoseByName(), poseName : " + poseName);
        this.poseName = poseName;
        if (RobotPoses.Presets.ContainsKey(poseName))
            ApplyPoseWithJointValue(RobotPoses.Presets[poseName]);
        else
            Debug.LogWarning($"[RobotStartPose] '{poseName}' preset이 없습니다.");
    }

    public void ApplyPoseFromInspector()
    {
        ApplyPoseByName(this.poseName);
    }
}


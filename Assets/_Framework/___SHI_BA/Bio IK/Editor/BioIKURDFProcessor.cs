using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Xml; // For URDF parsing
using System.Globalization; // For parsing numbers
using System.Text; // For StringBuilder
using System;
using _SHI_BA;

namespace BioIK
{
    // URDFImporter.cs의 URDFData 및 JointData 클래스 정의가 필요합니다.
    // 간소화를 위해 여기에 필요한 부분만 다시 정의하거나,
    // URDFImporter.cs의 해당 클래스들을 public으로 만들고 직접 참조합니다.
    // 여기서는 필요한 구조체를 간단히 정의합니다.
    public class TempUrdfJointData
    {
        public string Name;
        public string Type;
        public string Parent;
        public string Child;
        public Vector3 OriginXYZ;
        public Vector3 OriginRPY;
        public Vector3 Axis;
        public float LowerLimit;
        public float UpperLimit;
        public float Velocity;
        public float Effort;
    }

    public class BioIKURDFProcessor : EditorWindow
    {
        private GameObject robotRootObject;
        private TextAsset urdfFileAsset; // URDF 파일을 TextAsset으로 받도록 변경

        [MenuItem("Tools/BioIK/Process URDF for BioJoints")]
        static void ShowWindow()
        {
            GetWindow<BioIKURDFProcessor>("BioIK URDF Processor");
        }

        void OnGUI()
        {
            GUILayout.Label("Configure BioJoints from URDF File", EditorStyles.boldLabel);
            robotRootObject = EditorGUILayout.ObjectField("Robot Root (BioIK attached)", robotRootObject, typeof(GameObject), true) as GameObject;
            urdfFileAsset = EditorGUILayout.ObjectField("URDF File Asset", urdfFileAsset, typeof(TextAsset), false) as TextAsset;

            if (GUILayout.Button("Run Configuration"))
            {
                if (!ValidateInputs()) return;

                BioIK bioIKComponent = robotRootObject.GetComponent<BioIK>();
                List<TempUrdfJointData> urdfJoints = ParseUrdf(urdfFileAsset.text);

                if (urdfJoints == null)
                {
                    Debug.LogError("[BioIKURDFProcessor] Failed to parse URDF data.");
                    return;
                }

                ConfigureJoints(bioIKComponent, urdfJoints);
            }
        }

        private bool ValidateInputs()
        {
            if (robotRootObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the Robot Root GameObject.", "OK");
                return false;
            }
            if (robotRootObject.GetComponent<BioIK>() == null)
            {
                EditorUtility.DisplayDialog("Error", "Robot Root GameObject must have a BioIK component.", "OK");
                return false;
            }
            if (urdfFileAsset == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the URDF file TextAsset.", "OK");
                return false;
            }
            return true;
        }

        private void ConfigureJoints(BioIK bioIK, List<TempUrdfJointData> urdfJoints)
        {
            Undo.RecordObject(bioIK, "Auto-Configure BioJoints from URDF");
            bioIK.Refresh(false); // Refresh segments without re-initializing evolution yet

            int configuredCount = 0;
            Dictionary<string, Transform> linkTransforms = new Dictionary<string, Transform>();
            foreach (BioSegment seg in bioIK.Segments)
            {
                if (seg != null && seg.Transform != null && !linkTransforms.ContainsKey(seg.Transform.name))
                {
                    linkTransforms.Add(seg.Transform.name, seg.Transform);
                }
            }

            foreach (TempUrdfJointData urdfJoint in urdfJoints)
            {
                if (string.IsNullOrEmpty(urdfJoint.Child) || !linkTransforms.ContainsKey(urdfJoint.Child))
                {
                    //Debug.LogWarning($"[BioIKURDFProcessor] Child link '{urdfJoint.Child}' defined in URDF joint '{urdfJoint.Name}' not found in BioIK segments. Skipping.");
                    continue;
                }

                Transform childLinkTransform = linkTransforms[urdfJoint.Child];
                BioSegment childSegment = bioIK.FindSegment(childLinkTransform); // BioIK에서 해당 Transform을 가진 Segment 찾기

                if (childSegment == null)
                {
                    //Debug.LogWarning($"[BioIKURDFProcessor] BioSegment for child link '{urdfJoint.Child}' not found. Skipping.");
                    continue;
                }

                BioJoint bioJoint = childSegment.GetComponent<BioJoint>();

                if (urdfJoint.Type.ToLowerInvariant() == "fixed")
                {
                    if (bioJoint != null)
                    {
                        Debug.Log($"[BioIKURDFProcessor] Segment '{childSegment.Transform.name}' connected by FIXED URDF joint '{urdfJoint.Name}'. Disabling BioJoint motions.");
                        Undo.RecordObject(bioJoint, "Disable Fixed BioJoint " + childSegment.Transform.name);
                        SetAllMotionEnabled(bioJoint, false);
                        EditorUtility.SetDirty(bioJoint);
                    }
                    continue;
                }

                if (bioJoint == null)
                {
                    // bioJoint = Undo.AddComponent<BioJoint>(childSegment.gameObject);
                    bioJoint = Undo.AddComponent<BA_BioJoint>(childSegment.gameObject);
                    InitializeNewBioJoint(bioJoint, childSegment); // 새 BioJoint 초기화
                }
                Undo.RecordObject(bioJoint, "Configure BioJoint " + childSegment.Transform.name);

                if (urdfJoint.Type.ToLowerInvariant() == "revolute")
                {
                    //bioJoint.JointType = JointType.Rotational;
                    //bioJoint.SetAnchor(Vector3.zero); // URDF origin은 이미 링크의 localTransform에 적용됨
                    //bioJoint.SetOrientation(Vector3.zero); // URDF axis를 기준으로 BioJoint의 X,Y,Z 중 하나를 활성화

                    SetAllMotionEnabled(bioJoint, false); // 중요: 모든 축을 먼저 비활성화

                    // urdfJoint.Axis는 이미 자식 링크의 로컬 좌표계 기준이라고 가정합니다.
                    BioJoint.Motion activeMotion = GetMatchingMotion(bioJoint, urdfJoint.Axis);

                    if (activeMotion != null)
                    {
                        Undo.RecordObject(activeMotion.Joint, "Enable Motion Axis " + childSegment.Transform.name);
                        activeMotion.SetEnabled(true);
                        activeMotion.Constrained = true; // URDF limit 사용

                        // URDF limit (라디안) -> BioJoint limit (도)
                        float lowerDeg = urdfJoint.LowerLimit * Mathf.Rad2Deg;
                        float upperDeg = urdfJoint.UpperLimit * Mathf.Rad2Deg;
                        activeMotion.SetLowerLimit(lowerDeg);
                        activeMotion.SetUpperLimit(upperDeg);

                        // 초기 목표 값은 중간값 또는 0으로 설정
                        double midRangeTarget = (lowerDeg + upperDeg) / 2.0;
                        // 한계값이 비정상적인 경우 (예: lower > upper) 대비
                        if (upperDeg < lowerDeg)
                        {
                            Debug.LogWarning($"[BioIKURDFProcessor] Joint '{urdfJoint.Name}' on '{childSegment.Transform.name}' has invalid limits (Lower: {lowerDeg:F1}, Upper: {upperDeg:F1}). Setting target to 0.");
                            midRangeTarget = 0;
                        }
                        //activeMotion.SetTargetValue(midRangeTarget);
                        //activeMotion.CurrentValue = midRangeTarget; // 현재 값도 동일하게 초기화

                        Debug.Log($"[BioIKURDFProcessor] Configured BioJoint for '{childSegment.Transform.name}' (URDF: '{urdfJoint.Name}'): Type=Rotational, URDF_Axis={urdfJoint.Axis.ToString("F3")}, BioIK_Active_Axis='{GetMotionAxisName(activeMotion)}', Limits=[{lowerDeg:F1}, {upperDeg:F1}] deg, InitialTarget={midRangeTarget:F1} deg");
                        configuredCount++;
                    }
                    else
                    {
                        // GetMatchingMotion에서 이미 경고 로그를 출력했을 것이므로 여기서는 추가 로그 생략 가능
                        // 또는 여기서도 해당 관절에 대해 축 활성화 실패 로그를 남길 수 있습니다.
                        Debug.LogError($"[BioIKURDFProcessor] FAILED to find matching motion axis for URDF joint '{urdfJoint.Name}' (axis: {urdfJoint.Axis.ToString("F3")}) on segment '{childSegment.Transform.name}'. This joint will NOT contribute to DoF.");
                    }
                }
                // TODO: Add handling for "prismatic" joints if needed

                EditorUtility.SetDirty(bioJoint);
            }

            Debug.Log($"[BioIKURDFProcessor] Configuration finished. {configuredCount} non-fixed joints processed. Refreshing BioIK component to update DoF.");
            bioIK.Refresh(true); // Rebuild model and re-calculate DoF
            EditorUtility.SetDirty(bioIK);
            EditorUtility.DisplayDialog("BioIK URDF Processor", $"{configuredCount} non-fixed joints configured. Check console for details and review BioIK setup.", "OK");
        }

        private void InitializeNewBioJoint(BioJoint bioJoint, BioSegment segment)
        {
            // BioJoint.Create() 메서드와 유사한 초기화 수행
            bioJoint.Segment = segment;
            // segment.Transform.hideFlags = HideFlags.NotEditable; // 에디터에서 직접 수정 방지 (선택 사항)
            // bioJoint.hideFlags = HideFlags.HideInInspector; // 인스펙터에서 숨김 (선택 사항)

            bioJoint.X = new BioJoint.Motion(bioJoint, Vector3.right);
            bioJoint.Y = new BioJoint.Motion(bioJoint, Vector3.up);
            bioJoint.Z = new BioJoint.Motion(bioJoint, Vector3.forward);

            // 기본 프레임을 현재 트랜스폼으로 설정
            bioJoint.SetDefaultFrame(segment.Transform.localPosition, segment.Transform.localRotation);
            bioJoint.SetAnchor(Vector3.zero); // 기본 앵커
            bioJoint.SetOrientation(Vector3.zero); // 기본 방향

            // segment.Joint에도 새 BioJoint 참조 설정
            segment.Joint = bioJoint;
        }


        private void SetAllMotionEnabled(BioJoint bioJoint, bool enabled)
        {
            if (bioJoint.X != null) bioJoint.X.SetEnabled(enabled);
            if (bioJoint.Y != null) bioJoint.Y.SetEnabled(enabled);
            if (bioJoint.Z != null) bioJoint.Z.SetEnabled(enabled);
        }

        private BioJoint.Motion GetMatchingMotion(BioJoint bioJoint, Vector3 urdfAxis)
        {
            if (bioJoint == null)
            {
                Debug.LogError($"[GetMatchingMotion] Called with null bioJoint for segment: {(bioJoint?.Segment?.Transform?.name ?? "Unknown")}", bioJoint?.gameObject);
                return null;
            }
            if (bioJoint.X == null || bioJoint.Y == null || bioJoint.Z == null)
            {
                Debug.LogError($"[GetMatchingMotion] Motion objects (X,Y,Z) are not initialized for BioJoint on {bioJoint.Segment.Transform.name}. Attempting re-initialization.", bioJoint.gameObject);
                if (bioJoint.Segment != null) InitializeNewBioJoint(bioJoint, bioJoint.Segment); // Motion 객체들이 없다면 여기서라도 초기화 시도
                if (bioJoint.X == null || bioJoint.Y == null || bioJoint.Z == null)
                {
                    Debug.LogError($"[GetMatchingMotion] CRITICAL: Motion objects still null after re-init attempt for {bioJoint.Segment.Transform.name}. Cannot match axis.", bioJoint.gameObject);
                    return null;
                }
            }

            Vector3 localX = Vector3.right;
            Vector3 localY = Vector3.up;
            Vector3 localZ = Vector3.forward;

            Vector3 normalizedUrdfAxis = urdfAxis.normalized;

            float dotX = Vector3.Dot(normalizedUrdfAxis, localX);
            float dotY = Vector3.Dot(normalizedUrdfAxis, localY);
            float dotZ = Vector3.Dot(normalizedUrdfAxis, localZ);

            float absDotX = Mathf.Abs(dotX);
            float absDotY = Mathf.Abs(dotY);
            float absDotZ = Mathf.Abs(dotZ);

            // 디버깅 로그는 모든 dot 값 계산 후에 위치
            Debug.Log($"[GetMatchingMotion] For '{bioJoint.Segment.Transform.name}': URDF Axis = {normalizedUrdfAxis.ToString("F3")}, Dots(X,Y,Z) = ({dotX:F2}, {dotY:F2}, {dotZ:F2})");

            float primaryMatchThreshold = 0.95f; // 주 축으로 확실히 매칭되기 위한 임계값
            float fallbackMinThreshold = 0.5f;  // Fallback으로라도 선택되기 위한 최소한의 정렬 임계값

            // 1. 주 축과 거의 평행한지 먼저 확인 (가장 이상적인 경우)
            if (absDotX > primaryMatchThreshold && absDotX >= absDotY && absDotX >= absDotZ)
            {
                Debug.Log($"[GetMatchingMotion] --> Primary Match: Matched URDF axis {normalizedUrdfAxis.ToString("F3")} to BioJoint X-Axis for {bioJoint.Segment.Transform.name}");
                return bioJoint.X;
            }
            if (absDotY > primaryMatchThreshold && absDotY >= absDotX && absDotY >= absDotZ)
            {
                Debug.Log($"[GetMatchingMotion] --> Primary Match: Matched URDF axis {normalizedUrdfAxis.ToString("F3")} to BioJoint Y-Axis for {bioJoint.Segment.Transform.name}");
                return bioJoint.Y;
            }
            if (absDotZ > primaryMatchThreshold && absDotZ >= absDotX && absDotZ >= absDotY)
            {
                Debug.Log($"[GetMatchingMotion] --> Primary Match: Matched URDF axis {normalizedUrdfAxis.ToString("F3")} to BioJoint Z-Axis for {bioJoint.Segment.Transform.name}");
                return bioJoint.Z;
            }

            // 2. "축 무조건 활성화" 옵션 (Fallback 로직): 가장 큰 내적 값을 가진 축 선택 (단, 최소 임계값 이상일 때)
            //    이 부분은 사용자가 "무조건 활성화" 옵션을 UI로 선택했을 때만 동작하도록 할 수도 있습니다.
            //    여기서는 primaryMatchThreshold보다 낮은 fallbackMinThreshold를 사용합니다.
            Debug.LogWarning($"[GetMatchingMotion] Primary match failed for '{bioJoint.Segment.Transform.name}'. URDF Axis: {normalizedUrdfAxis.ToString("F3")}. Trying fallback with threshold {fallbackMinThreshold}.");

            if (absDotX > absDotY && absDotX > absDotZ && absDotX > fallbackMinThreshold)
            {
                Debug.LogWarning($"[GetMatchingMotion] ---> Fallback Match: Matched to BioJoint X-Axis for '{bioJoint.Segment.Transform.name}' (Dot: {dotX:F2})");
                return bioJoint.X;
            }
            if (absDotY > absDotX && absDotY > absDotZ && absDotY > fallbackMinThreshold)
            {
                Debug.LogWarning($"[GetMatchingMotion] ---> Fallback Match: Matched to BioJoint Y-Axis for '{bioJoint.Segment.Transform.name}' (Dot: {dotY:F2})");
                return bioJoint.Y;
            }
            if (absDotZ > absDotX && absDotZ > absDotY && absDotZ > fallbackMinThreshold)
            {
                Debug.LogWarning($"[GetMatchingMotion] ---> Fallback Match: Matched to BioJoint Z-Axis for '{bioJoint.Segment.Transform.name}' (Dot: {dotZ:F2})");
                return bioJoint.Z;
            }

            // Fallback 마저 실패한 경우
            Debug.LogError($"[GetMatchingMotion] CRITICAL FAILURE: Could not reliably match URDF axis {normalizedUrdfAxis.ToString("F3")} for joint on '{bioJoint.Segment.Transform.name}' even with fallback. Dots: X={dotX:F2}, Y={dotY:F2}, Z={dotZ:F2}. No motion axis enabled.");
            return null;
        }

        private string GetMotionAxisName(BioJoint.Motion motion)
        {
            if (motion == null || motion.Joint == null) return "N/A";
            if (motion == motion.Joint.X) return "X";
            if (motion == motion.Joint.Y) return "Y";
            if (motion == motion.Joint.Z) return "Z";
            return "Unknown";
        }

        // URDF 파싱 로직 (URDFImporter.cs의 URDFData 생성자 로직과 유사하게 구현 필요)
        private List<TempUrdfJointData> ParseUrdf(string urdfContent)
        {
            List<TempUrdfJointData> jointDataList = new List<TempUrdfJointData>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(urdfContent);
                XmlNode robotNode = xmlDoc.SelectSingleNode("robot");
                if (robotNode == null)
                {
                    Debug.LogError("[BioIKURDFProcessor] <robot> element not found in URDF.");
                    return null;
                }

                foreach (XmlNode jointNode in robotNode.SelectNodes("joint"))
                {
                    TempUrdfJointData jd = new TempUrdfJointData();
                    jd.Name = jointNode.Attributes["name"]?.Value;
                    jd.Type = jointNode.Attributes["type"]?.Value;

                    XmlNode parentNode = jointNode.SelectSingleNode("parent");
                    jd.Parent = parentNode?.Attributes["link"]?.Value;

                    XmlNode childNode = jointNode.SelectSingleNode("child");
                    jd.Child = childNode?.Attributes["link"]?.Value;

                    XmlNode originNode = jointNode.SelectSingleNode("origin");
                    if (originNode != null)
                    {
                        jd.OriginXYZ = ReadVector3(originNode.Attributes["xyz"]?.Value);
                        jd.OriginRPY = ReadVector3(originNode.Attributes["rpy"]?.Value);
                    }

                    if (jd.Type != "fixed")
                    { // 고정 조인트가 아닐 때만 axis와 limit을 읽음
                        XmlNode axisNode = jointNode.SelectSingleNode("axis");
                        if (axisNode != null)
                        {
                            jd.Axis = ReadVector3(axisNode.Attributes["xyz"]?.Value);
                        }
                        else
                        {
                            jd.Axis = new Vector3(1, 0, 0); // URDF 기본값
                        }


                        XmlNode limitNode = jointNode.SelectSingleNode("limit");
                        if (limitNode != null)
                        {
                            jd.LowerLimit = ReadFloat(limitNode.Attributes["lower"]?.Value);
                            jd.UpperLimit = ReadFloat(limitNode.Attributes["upper"]?.Value);
                            jd.Velocity = ReadFloat(limitNode.Attributes["velocity"]?.Value);
                            jd.Effort = ReadFloat(limitNode.Attributes["effort"]?.Value);
                        }
                    }
                    jointDataList.Add(jd);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[BioIKURDFProcessor] Error parsing URDF: " + e.Message);
                return null;
            }
            return jointDataList;
        }

        // URDFImporter.cs에서 가져온 파싱 헬퍼 함수들 (실제로는 해당 클래스 직접 사용 권장)
        private float ReadFloat(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0f;
            value = FilterValueField(value);
            float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsed);
            return parsed;
        }
        private Vector3 ReadVector3(string value)
        {
            if (string.IsNullOrEmpty(value)) return Vector3.zero;
            value = FilterValueField(value);
            string[] values = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length < 3) return Vector3.zero;
            float.TryParse(values[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x);
            float.TryParse(values[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y);
            float.TryParse(values[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float z);
            return new Vector3(x, y, z);
        }
        private string FilterValueField(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(value.Trim(), @"\s+", " ");
        }
    }
}
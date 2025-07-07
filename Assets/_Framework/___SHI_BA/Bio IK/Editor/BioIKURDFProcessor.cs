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
    // URDFImporter.cs�� URDFData �� JointData Ŭ���� ���ǰ� �ʿ��մϴ�.
    // ����ȭ�� ���� ���⿡ �ʿ��� �κи� �ٽ� �����ϰų�,
    // URDFImporter.cs�� �ش� Ŭ�������� public���� ����� ���� �����մϴ�.
    // ���⼭�� �ʿ��� ����ü�� ������ �����մϴ�.
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
        private TextAsset urdfFileAsset; // URDF ������ TextAsset���� �޵��� ����

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
                BioSegment childSegment = bioIK.FindSegment(childLinkTransform); // BioIK���� �ش� Transform�� ���� Segment ã��

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
                    InitializeNewBioJoint(bioJoint, childSegment); // �� BioJoint �ʱ�ȭ
                }
                Undo.RecordObject(bioJoint, "Configure BioJoint " + childSegment.Transform.name);

                if (urdfJoint.Type.ToLowerInvariant() == "revolute")
                {
                    //bioJoint.JointType = JointType.Rotational;
                    //bioJoint.SetAnchor(Vector3.zero); // URDF origin�� �̹� ��ũ�� localTransform�� �����
                    //bioJoint.SetOrientation(Vector3.zero); // URDF axis�� �������� BioJoint�� X,Y,Z �� �ϳ��� Ȱ��ȭ

                    SetAllMotionEnabled(bioJoint, false); // �߿�: ��� ���� ���� ��Ȱ��ȭ

                    // urdfJoint.Axis�� �̹� �ڽ� ��ũ�� ���� ��ǥ�� �����̶�� �����մϴ�.
                    BioJoint.Motion activeMotion = GetMatchingMotion(bioJoint, urdfJoint.Axis);

                    if (activeMotion != null)
                    {
                        Undo.RecordObject(activeMotion.Joint, "Enable Motion Axis " + childSegment.Transform.name);
                        activeMotion.SetEnabled(true);
                        activeMotion.Constrained = true; // URDF limit ���

                        // URDF limit (����) -> BioJoint limit (��)
                        float lowerDeg = urdfJoint.LowerLimit * Mathf.Rad2Deg;
                        float upperDeg = urdfJoint.UpperLimit * Mathf.Rad2Deg;
                        activeMotion.SetLowerLimit(lowerDeg);
                        activeMotion.SetUpperLimit(upperDeg);

                        // �ʱ� ��ǥ ���� �߰��� �Ǵ� 0���� ����
                        double midRangeTarget = (lowerDeg + upperDeg) / 2.0;
                        // �Ѱ谪�� ���������� ��� (��: lower > upper) ���
                        if (upperDeg < lowerDeg)
                        {
                            Debug.LogWarning($"[BioIKURDFProcessor] Joint '{urdfJoint.Name}' on '{childSegment.Transform.name}' has invalid limits (Lower: {lowerDeg:F1}, Upper: {upperDeg:F1}). Setting target to 0.");
                            midRangeTarget = 0;
                        }
                        //activeMotion.SetTargetValue(midRangeTarget);
                        //activeMotion.CurrentValue = midRangeTarget; // ���� ���� �����ϰ� �ʱ�ȭ

                        Debug.Log($"[BioIKURDFProcessor] Configured BioJoint for '{childSegment.Transform.name}' (URDF: '{urdfJoint.Name}'): Type=Rotational, URDF_Axis={urdfJoint.Axis.ToString("F3")}, BioIK_Active_Axis='{GetMotionAxisName(activeMotion)}', Limits=[{lowerDeg:F1}, {upperDeg:F1}] deg, InitialTarget={midRangeTarget:F1} deg");
                        configuredCount++;
                    }
                    else
                    {
                        // GetMatchingMotion���� �̹� ��� �α׸� ������� ���̹Ƿ� ���⼭�� �߰� �α� ���� ����
                        // �Ǵ� ���⼭�� �ش� ������ ���� �� Ȱ��ȭ ���� �α׸� ���� �� �ֽ��ϴ�.
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
            // BioJoint.Create() �޼���� ������ �ʱ�ȭ ����
            bioJoint.Segment = segment;
            // segment.Transform.hideFlags = HideFlags.NotEditable; // �����Ϳ��� ���� ���� ���� (���� ����)
            // bioJoint.hideFlags = HideFlags.HideInInspector; // �ν����Ϳ��� ���� (���� ����)

            bioJoint.X = new BioJoint.Motion(bioJoint, Vector3.right);
            bioJoint.Y = new BioJoint.Motion(bioJoint, Vector3.up);
            bioJoint.Z = new BioJoint.Motion(bioJoint, Vector3.forward);

            // �⺻ �������� ���� Ʈ���������� ����
            bioJoint.SetDefaultFrame(segment.Transform.localPosition, segment.Transform.localRotation);
            bioJoint.SetAnchor(Vector3.zero); // �⺻ ��Ŀ
            bioJoint.SetOrientation(Vector3.zero); // �⺻ ����

            // segment.Joint���� �� BioJoint ���� ����
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
                if (bioJoint.Segment != null) InitializeNewBioJoint(bioJoint, bioJoint.Segment); // Motion ��ü���� ���ٸ� ���⼭�� �ʱ�ȭ �õ�
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

            // ����� �α״� ��� dot �� ��� �Ŀ� ��ġ
            Debug.Log($"[GetMatchingMotion] For '{bioJoint.Segment.Transform.name}': URDF Axis = {normalizedUrdfAxis.ToString("F3")}, Dots(X,Y,Z) = ({dotX:F2}, {dotY:F2}, {dotZ:F2})");

            float primaryMatchThreshold = 0.95f; // �� ������ Ȯ���� ��Ī�Ǳ� ���� �Ӱ谪
            float fallbackMinThreshold = 0.5f;  // Fallback���ζ� ���õǱ� ���� �ּ����� ���� �Ӱ谪

            // 1. �� ��� ���� �������� ���� Ȯ�� (���� �̻����� ���)
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

            // 2. "�� ������ Ȱ��ȭ" �ɼ� (Fallback ����): ���� ū ���� ���� ���� �� ���� (��, �ּ� �Ӱ谪 �̻��� ��)
            //    �� �κ��� ����ڰ� "������ Ȱ��ȭ" �ɼ��� UI�� �������� ���� �����ϵ��� �� ���� �ֽ��ϴ�.
            //    ���⼭�� primaryMatchThreshold���� ���� fallbackMinThreshold�� ����մϴ�.
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

            // Fallback ���� ������ ���
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

        // URDF �Ľ� ���� (URDFImporter.cs�� URDFData ������ ������ �����ϰ� ���� �ʿ�)
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
                    { // ���� ����Ʈ�� �ƴ� ���� axis�� limit�� ����
                        XmlNode axisNode = jointNode.SelectSingleNode("axis");
                        if (axisNode != null)
                        {
                            jd.Axis = ReadVector3(axisNode.Attributes["xyz"]?.Value);
                        }
                        else
                        {
                            jd.Axis = new Vector3(1, 0, 0); // URDF �⺻��
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

        // URDFImporter.cs���� ������ �Ľ� ���� �Լ��� (�����δ� �ش� Ŭ���� ���� ��� ����)
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
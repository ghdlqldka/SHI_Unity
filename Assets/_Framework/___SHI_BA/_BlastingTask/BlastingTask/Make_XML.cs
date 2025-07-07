using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using BioIK;
using TMPro;

public class Make_XML : MonoBehaviour
{
    public BioIK.BioIK bioIK;
    public BioSegment baseSegment;
    public BioSegment tcpSegment;

    [Header("XML 저장 경로")]
    public string xmlSaveFolder = "Motion_XML";

    /*
    [Header("인풋 필드 설정")]
    public GameObject spdInputField;
    public GameObject accInputField;
    */

    [Header("갠트리 무버 Transform")]
    [Tooltip("씬에 있는 'x_mover' 오브젝트를 여기에 드래그하세요.")]
    public Transform xMoverTransform;
    [Tooltip("씬에 있는 'y_mover' 오브젝트를 여기에 드래그하세요.")]
    public Transform yMoverTransform;
    [Tooltip("씬에 있는 'z' 오브젝트를 여기에 드래그하세요.")]
    public Transform zObjectTransform;

    [System.Serializable]
    public class PoseData
    {
        public int id = 1;
        public CorrectionTypeData correctionType = new CorrectionTypeData();
        public KukaCartesianData kukaCartesian = new KukaCartesianData();
        public KukaAxisSpecificData kukaAxis = new KukaAxisSpecificData();
        public KukaMovOptData kukaMovOpt = new KukaMovOptData();
        public GantryExternalAxisData gantry = new GantryExternalAxisData();
        public ROTIExternalAxisData roti = new ROTIExternalAxisData();
        public WeavingExternalAxisData weaving = new WeavingExternalAxisData(); // 이 클래스의 정의가 변경됩니다.
        public IOStatusData ioStatus = new IOStatusData();
        public FlagsData flags = new FlagsData();
        public CoordinateData coordinate = new CoordinateData();
    }

    [System.Serializable] public class CorrectionTypeData { public int Type = 1; }
    [System.Serializable] public class KukaCartesianData { public float X, Y, Z, A, B, C; public int S, T; }
    [System.Serializable] public class KukaAxisSpecificData { public float A1, A2, A3, A4, A5, A6; }
    [System.Serializable] public class KukaMovOptData { public float SPD = 400.0f, ACC = 200.0f, TIME = 0.0f; }
    [System.Serializable] public class GantryExternalAxisData { public float E1, E2, E3; }
    [System.Serializable] public class ROTIExternalAxisData { public float E4, E5, SPD = 100.0f, ACC = 50.0f; }

    // [수정] WeavingExternalAxisData 클래스 정의 변경
    [System.Serializable]
    public class WeavingExternalAxisData { public float ONOFF = 0.0f, ANGLE_MIN = 0.0f, ANGLE_MAX = 0.0f, ANGLESPD = 0.0f; }

    [System.Serializable] public class IOStatusData { public int MODE, PRE_AFTER_FLOW_TIME; }
    [System.Serializable] public class FlagsData { public int FLAG1, FLAG2; }
    [System.Serializable] public class CoordinateData { public int SELECT = 1; }

    [Header("Pose 리스트")]
    public List<PoseData> poses = new List<PoseData>();

    [Header("Motion ID")]
    public int motionID = 10000;

    /// <summary>
    /// [신규 추가] Pose 리스트를 파일로 저장하는 대신 XML 형식의 문자열로 변환하여 반환합니다.
    /// WebGL 빌드에서 파일 시스템을 사용하지 않기 위해 필요합니다.
    /// </summary>
    /// <param name="poseList">XML로 변환할 Pose 데이터 리스트</param>
    /// <returns>생성된 XML 데이터의 전체 문자열</returns>
    public string ConvertPoseListToXmlString(List<PoseData> poseList)
    {
        if (poseList == null || poseList.Count == 0)
        {
            Debug.LogWarning("[Make_XML] 변환할 Pose 데이터가 없습니다.");
            return string.Empty;
        }

        // XmlDocument를 사용하여 메모리 상에서 XML 구조를 만듭니다.
        XmlDocument doc = new XmlDocument();

        // 최상위 <Motion> 요소 생성
        XmlElement motionElem = doc.CreateElement("Motion");
        motionElem.SetAttribute("ID", motionID.ToString());
        doc.AppendChild(motionElem);

        // 전달받은 모든 Pose 데이터를 순회하며 <Pose> 요소를 만듭니다.
        foreach (var pose in poseList)
        {
            XmlElement poseElem = doc.CreateElement("Pose");
            poseElem.SetAttribute("ID", pose.id.ToString());
            motionElem.AppendChild(poseElem);

            // SavePoseListToXml 메서드와 완전히 동일한 로직으로
            // 각 Pose의 상세 데이터를 XML 요소로 만듭니다.
            XmlElement extElem = doc.CreateElement("Ext");
            poseElem.AppendChild(extElem);

            XmlElement corrElem = doc.CreateElement("CorrectionType");
            corrElem.SetAttribute("Type", pose.correctionType.Type.ToString());
            extElem.AppendChild(corrElem);

            XmlElement posCmdElem = doc.CreateElement("PosCmd");
            extElem.AppendChild(posCmdElem);

            XmlElement cartElem = doc.CreateElement("Kuka_Cartesian");
            cartElem.SetAttribute("X", pose.kukaCartesian.X.ToString("F4"));
            cartElem.SetAttribute("Y", pose.kukaCartesian.Y.ToString("F4"));
            cartElem.SetAttribute("Z", pose.kukaCartesian.Z.ToString("F4"));
            cartElem.SetAttribute("A", pose.kukaCartesian.A.ToString("F2"));
            cartElem.SetAttribute("B", pose.kukaCartesian.B.ToString("F2"));
            cartElem.SetAttribute("C", pose.kukaCartesian.C.ToString("F2"));
            cartElem.SetAttribute("S", pose.kukaCartesian.S.ToString());
            cartElem.SetAttribute("T", pose.kukaCartesian.T.ToString());
            posCmdElem.AppendChild(cartElem);

            XmlElement axisElem = doc.CreateElement("Kuka_AxisSpecific");
            axisElem.SetAttribute("A1", pose.kukaAxis.A1.ToString("F4"));
            axisElem.SetAttribute("A2", pose.kukaAxis.A2.ToString("F4"));
            axisElem.SetAttribute("A3", pose.kukaAxis.A3.ToString("F4"));
            axisElem.SetAttribute("A4", pose.kukaAxis.A4.ToString("F4"));
            axisElem.SetAttribute("A5", pose.kukaAxis.A5.ToString("F4"));
            axisElem.SetAttribute("A6", pose.kukaAxis.A6.ToString("F4"));
            posCmdElem.AppendChild(axisElem);

            XmlElement movOptElem = doc.CreateElement("Kuka_MovOpt");
            movOptElem.SetAttribute("SPD", pose.kukaMovOpt.SPD.ToString("F1"));
            movOptElem.SetAttribute("ACC", pose.kukaMovOpt.ACC.ToString("F1"));
            movOptElem.SetAttribute("TIME", pose.kukaMovOpt.TIME.ToString("F1"));
            posCmdElem.AppendChild(movOptElem);

            XmlElement gantryElem = doc.CreateElement("Gantry_ExternalAxis");
            gantryElem.SetAttribute("E1", pose.gantry.E1.ToString("F4"));
            gantryElem.SetAttribute("E2", pose.gantry.E2.ToString("F4"));
            gantryElem.SetAttribute("E3", pose.gantry.E3.ToString("F4"));
            posCmdElem.AppendChild(gantryElem);

            XmlElement rotiElem = doc.CreateElement("ROTI_ExternalAxis");
            rotiElem.SetAttribute("E4", pose.roti.E4.ToString("F4"));
            rotiElem.SetAttribute("E5", pose.roti.E5.ToString("F4"));
            rotiElem.SetAttribute("SPD", pose.roti.SPD.ToString("F1"));
            rotiElem.SetAttribute("ACC", pose.roti.ACC.ToString("F1"));
            posCmdElem.AppendChild(rotiElem);

            XmlElement weavingElem = doc.CreateElement("Weaving_ExternalAxis");
            weavingElem.SetAttribute("ONOFF", pose.weaving.ONOFF.ToString("F4"));
            weavingElem.SetAttribute("ANGLE_MIN", pose.weaving.ANGLE_MIN.ToString("F1"));
            weavingElem.SetAttribute("ANGLE_MAX", pose.weaving.ANGLE_MAX.ToString("F1"));
            weavingElem.SetAttribute("ANGLESPD", pose.weaving.ANGLESPD.ToString("F1"));
            posCmdElem.AppendChild(weavingElem);

            XmlElement ioStatusElem = doc.CreateElement("IO_Status");
            ioStatusElem.SetAttribute("MODE", pose.ioStatus.MODE.ToString());
            ioStatusElem.SetAttribute("PRE_AFTER_FLOW_TIME", pose.ioStatus.PRE_AFTER_FLOW_TIME.ToString());
            extElem.AppendChild(ioStatusElem);

            XmlElement flagsElem = doc.CreateElement("Flags");
            flagsElem.SetAttribute("FLAG1", pose.flags.FLAG1.ToString());
            flagsElem.SetAttribute("FLAG2", pose.flags.FLAG2.ToString());
            extElem.AppendChild(flagsElem);

            XmlElement coordElem = doc.CreateElement("Coordinate");
            coordElem.SetAttribute("SELECT", pose.coordinate.SELECT.ToString());
            extElem.AppendChild(coordElem);
        }

        // 최종적으로 만들어진 XML 문서를 문자열로 변환하여 반환합니다.
        return doc.OuterXml;
    }
    public void FillPoseFromBioIK(BioIK.BioIK bioIK, BioSegment baseSegment, BioSegment tcpSegment, PoseData pose)
    {
        // 1. FK 값 계산 (Transform 기준)
        if (baseSegment != null && tcpSegment != null)
        {
            Transform baseTransform = baseSegment.Transform;
            Transform tcpTransform = tcpSegment.Transform;
            Vector3 localPosition = baseTransform.InverseTransformPoint(tcpTransform.position);
            Quaternion localRotation = Quaternion.Inverse(baseTransform.rotation) * tcpTransform.rotation;
            pose.kukaCartesian.X = localPosition.x;
            pose.kukaCartesian.Y = localPosition.y;
            pose.kukaCartesian.Z = localPosition.z;
            pose.kukaCartesian.A = localRotation.eulerAngles.x;
            pose.kukaCartesian.B = localRotation.eulerAngles.y;
            pose.kukaCartesian.C = localRotation.eulerAngles.z;
        }

        // 2. Gantry 값 계산 (localPosition 기준)
        if (xMoverTransform != null) { pose.gantry.E1 = xMoverTransform.localPosition.x * -1000f; }
        if (yMoverTransform != null) { pose.gantry.E2 = yMoverTransform.localPosition.x * -1000f; }
        if (zObjectTransform != null) { pose.gantry.E3 = zObjectTransform.localPosition.y * -100f + 2800f; }

        // 3. 6축 관절 및 ROTI 값을 Transform.localEulerAngles에서 직접 읽어와 XML 값으로 변환
        foreach (var segment in bioIK.Segments)
        {
            if (segment == null || segment.Transform == null) continue;

            string jointName = segment.Transform.name;
            Vector3 localEuler = segment.Transform.localEulerAngles;

            // 오일러 각도를 -180 ~ 180 범위로 변환
            float zAngle = localEuler.z > 180 ? localEuler.z - 360 : localEuler.z;
            float yAngle = localEuler.y > 180 ? localEuler.y - 360 : localEuler.y;
            float xAngle = localEuler.x > 180 ? localEuler.x - 360 : localEuler.x;

            // BioIK Joint의 현재 값 (디버깅용)
            float bioIK_zValue = 0, bioIK_yValue = 0, bioIK_xValue = 0;
            if (segment.Joint != null)
            {
                if (segment.Joint.Z != null) bioIK_zValue = (float)segment.Joint.Z.GetCurrentValue();
                if (segment.Joint.Y != null) bioIK_yValue = (float)segment.Joint.Y.GetCurrentValue();
                if (segment.Joint.X != null) bioIK_xValue = (float)segment.Joint.X.GetCurrentValue();
            }

            switch (jointName)
            {
                case "upbox":
                    pose.roti.E4 = -zAngle;
                    break;
                case "down":
                    pose.roti.E5 = -xAngle;
                    break;
                case "A1":
                    // Debug.Log($"[Make_XML Joint: {jointName}] Transform.Z: {zAngle:F4}, BioIK.Z: {bioIK_zValue:F4}");
                    pose.kukaAxis.A1 = bioIK_zValue;
                    break;
                case "A2":
                    pose.kukaAxis.A2 = (bioIK_yValue + 90f) * -1f;
                    break;
                case "A3":
                    pose.kukaAxis.A3 = (bioIK_yValue - 90f) * -1f;
                    break;
                case "A4":
                    pose.kukaAxis.A4 = -1f * bioIK_xValue;
                    break;
                case "A5":
                    pose.kukaAxis.A5 = -1f * bioIK_yValue;
                    break;
                case "A6":
                    pose.kukaAxis.A6 = -1f * bioIK_xValue;
                    break;
            }
        }
    }

    public void FillPoseFromBioIK_weaving(BioIK.BioIK bioIK, BioSegment baseSegment, BioSegment tcpSegment, PoseData pose, float weavingAngle, float spd, float acc) // 파라미터 추가
    {
        // 기존 FillPoseFromBioIK 로직 재활용 (여기서는 직접 호출하지 않고, FillPoseFromBioIK가 이미 모든 데이터를 채우도록 가정합니다.)
        // 혹은 필요한 데이터만 복사하여 채울 수도 있습니다. 여기서는 FillPoseFromBioIK를 호출하여 기본 데이터를 채우고,
        // KukaMovOpt 및 Weaving 데이터를 덮어쓰는 방식으로 구현합니다.
        FillPoseFromBioIK(bioIK, baseSegment, tcpSegment, pose);

        // KukaMovOpt 값을 입력으로 받은 값으로 덮어씁니다.
        pose.kukaMovOpt.SPD = spd;
        pose.kukaMovOpt.ACC = acc;

        // 위빙 관련 설정값을 덮어씁니다.
        pose.weaving.ONOFF = 1.0f; // 위빙 기능 활성화
        pose.weaving.ANGLE_MIN = -weavingAngle; // 최소 각도
        pose.weaving.ANGLE_MAX = weavingAngle;  // 최대 각도
        pose.weaving.ANGLESPD = 90.0f;          // 위빙 속도 (여기서는 하드코딩 유지)
    }

    public void FillPoseFromBioIK_weaving_fixed(BioIK.BioIK bioIK, BioSegment baseSegment, BioSegment tcpSegment, PoseData pose, float weavingAngle, float spd, float acc, float onoff) // 파라미터 추가
    {
        // 기존 FillPoseFromBioIK 로직 재활용 (여기서는 직접 호출하지 않고, FillPoseFromBioIK가 이미 모든 데이터를 채우도록 가정합니다.)
        // 혹은 필요한 데이터만 복사하여 채울 수도 있습니다. 여기서는 FillPoseFromBioIK를 호출하여 기본 데이터를 채우고,
        // KukaMovOpt 및 Weaving 데이터를 덮어쓰는 방식으로 구현합니다.
        FillPoseFromBioIK(bioIK, baseSegment, tcpSegment, pose);

        // KukaMovOpt 값을 입력으로 받은 값으로 덮어씁니다.
        pose.kukaMovOpt.SPD = spd;
        pose.kukaMovOpt.ACC = acc;
        
        // 위빙 관련 설정값을 덮어씁니다.
        pose.weaving.ONOFF = onoff; // 위빙 기능 활성화
        pose.weaving.ANGLE_MIN = weavingAngle; // 최소 각도
        pose.weaving.ANGLE_MAX = weavingAngle;  // 최대 각도
        pose.weaving.ANGLESPD = 90.0f;          // 위빙 속도 (여기서는 하드코딩 유지)
    }

    public void SavePoseListToXml(List<PoseData> poseList, int fileIndex)
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, xmlSaveFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, $"motion_{fileIndex}.xml");

        XmlDocument doc = new XmlDocument();
        XmlElement motionElem = doc.CreateElement("Motion");
        motionElem.SetAttribute("ID", motionID.ToString());
        doc.AppendChild(motionElem);

        foreach (var pose in poseList)
        {
            XmlElement poseElem = doc.CreateElement("Pose");
            poseElem.SetAttribute("ID", pose.id.ToString());
            motionElem.AppendChild(poseElem);

            XmlElement extElem = doc.CreateElement("Ext");
            poseElem.AppendChild(extElem);

            XmlElement corrElem = doc.CreateElement("CorrectionType");
            corrElem.SetAttribute("Type", pose.correctionType.Type.ToString());
            extElem.AppendChild(corrElem);

            XmlElement posCmdElem = doc.CreateElement("PosCmd");
            extElem.AppendChild(posCmdElem);

            XmlElement cartElem = doc.CreateElement("Kuka_Cartesian");
            cartElem.SetAttribute("X", pose.kukaCartesian.X.ToString("F4"));
            cartElem.SetAttribute("Y", pose.kukaCartesian.Y.ToString("F4"));
            cartElem.SetAttribute("Z", pose.kukaCartesian.Z.ToString("F4"));
            cartElem.SetAttribute("A", pose.kukaCartesian.A.ToString("F2"));
            cartElem.SetAttribute("B", pose.kukaCartesian.B.ToString("F2"));
            cartElem.SetAttribute("C", pose.kukaCartesian.C.ToString("F2"));
            cartElem.SetAttribute("S", pose.kukaCartesian.S.ToString());
            cartElem.SetAttribute("T", pose.kukaCartesian.T.ToString());
            posCmdElem.AppendChild(cartElem);

            XmlElement axisElem = doc.CreateElement("Kuka_AxisSpecific");
            axisElem.SetAttribute("A1", pose.kukaAxis.A1.ToString("F4"));
            axisElem.SetAttribute("A2", pose.kukaAxis.A2.ToString("F4"));
            axisElem.SetAttribute("A3", pose.kukaAxis.A3.ToString("F4"));
            axisElem.SetAttribute("A4", pose.kukaAxis.A4.ToString("F4"));
            axisElem.SetAttribute("A5", pose.kukaAxis.A5.ToString("F4"));
            axisElem.SetAttribute("A6", pose.kukaAxis.A6.ToString("F4"));
            posCmdElem.AppendChild(axisElem);

            XmlElement movOptElem = doc.CreateElement("Kuka_MovOpt");
            movOptElem.SetAttribute("SPD", pose.kukaMovOpt.SPD.ToString("F1"));
            movOptElem.SetAttribute("ACC", pose.kukaMovOpt.ACC.ToString("F1"));
            movOptElem.SetAttribute("TIME", pose.kukaMovOpt.TIME.ToString("F1"));
            posCmdElem.AppendChild(movOptElem);

            XmlElement gantryElem = doc.CreateElement("Gantry_ExternalAxis");
            gantryElem.SetAttribute("E1", pose.gantry.E1.ToString("F4"));
            gantryElem.SetAttribute("E2", pose.gantry.E2.ToString("F4"));
            gantryElem.SetAttribute("E3", pose.gantry.E3.ToString("F4"));
            posCmdElem.AppendChild(gantryElem);

            XmlElement rotiElem = doc.CreateElement("ROTI_ExternalAxis");
            rotiElem.SetAttribute("E4", pose.roti.E4.ToString("F4"));
            rotiElem.SetAttribute("E5", pose.roti.E5.ToString("F4"));
            rotiElem.SetAttribute("SPD", pose.roti.SPD.ToString("F1"));
            rotiElem.SetAttribute("ACC", pose.roti.ACC.ToString("F1"));
            posCmdElem.AppendChild(rotiElem);

            // [수정] Weaving_ExternalAxis 엘리먼트의 속성 변경
            XmlElement weavingElem = doc.CreateElement("Weaving_ExternalAxis");
            weavingElem.SetAttribute("ONOFF", pose.weaving.ONOFF.ToString("F4"));
            weavingElem.SetAttribute("ANGLE_MIN", pose.weaving.ANGLE_MIN.ToString("F1")); // ANGLE -> ANGLE_MIN
            weavingElem.SetAttribute("ANGLE_MAX", pose.weaving.ANGLE_MAX.ToString("F1")); // ANGLE_MAX 추가
            weavingElem.SetAttribute("ANGLESPD", pose.weaving.ANGLESPD.ToString("F1"));
            posCmdElem.AppendChild(weavingElem);

            XmlElement ioStatusElem = doc.CreateElement("IO_Status");
            ioStatusElem.SetAttribute("MODE", pose.ioStatus.MODE.ToString());
            ioStatusElem.SetAttribute("PRE_AFTER_FLOW_TIME", pose.ioStatus.PRE_AFTER_FLOW_TIME.ToString());
            extElem.AppendChild(ioStatusElem);

            XmlElement flagsElem = doc.CreateElement("Flags");
            flagsElem.SetAttribute("FLAG1", pose.flags.FLAG1.ToString());
            flagsElem.SetAttribute("FLAG2", pose.flags.FLAG2.ToString());
            extElem.AppendChild(flagsElem);

            XmlElement coordElem = doc.CreateElement("Coordinate");
            coordElem.SetAttribute("SELECT", pose.coordinate.SELECT.ToString());
            extElem.AppendChild(coordElem);
        }

        doc.Save(filePath);
        Debug.Log($"[Make_XML] {poseList.Count}개의 Pose를 motion_{fileIndex}.xml에 저장했습니다.");
    }

    [ContextMenu("Make Single XML")]
    public void MakeSingleXml()
    {
        PoseData pose = new PoseData();
        FillPoseFromBioIK(bioIK, baseSegment, tcpSegment, pose);
        SavePoseListToXml(new List<PoseData> { pose }, 0);
        Debug.Log($"[Make_XML] 단일 XML 파일 저장 완료: motion_0.xml");
    }

    private GameObject GetUpboxGameObject()
    {
        if (bioIK == null || bioIK.Segments == null) return null;
        foreach (var segment in bioIK.Segments)
        {
            if (segment.Joint != null && segment.Joint.gameObject.name == "upbox")
            {
                return segment.Joint.gameObject;
            }
        }
        return null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using _SHI_BA;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Make_XML;

public enum RequestType
{
    Select,   // ��ȸ
    Insert,   // ����
    Update,   // ����
    Delete    // ����
}
public enum RobotState
{
    Working,   // �۵� ��
    Idle,      // ��� ��
    Moving,    // �̵� ��
    Error      // ���� ����
}
public class SystemController : Singleton<SystemController>
{
    [Header("���̾� ����")]
    public LayerMask clickableLayer; // Ŭ�� ������ ���̾�

    [Header("��ũ��Ʈ ����")]
    public BA_Main main;
    public AutoPainter autoPainter;
    public CADLoad cadLoad;
    public XMLSender xMLSender;
    public XML_Importer_ssm xMLImporter;
    public DataRequester dataRequester;
    


    [Header("CAD,BPS,XML ������")]
     public MeshCollider cadCollider;
    [HideInInspector] public Dictionary<int,BPSData> dicBpsDatas;
    [HideInInspector] public Dictionary<int, XMLData> dicXmlDatas;
    [HideInInspector] public List<int> deleteBpsData;
    [HideInInspector] public CADData cadData;
    [HideInInspector] public XMLData xmlData;
    [Header("XML ��û ����")]
    public List<XMLRequestMapping> requestMappings = new List<XMLRequestMapping>();

    [Header("UI ���")]
    public Transform xMLCanvaTrans;
    public Transform bPSCanvaTrans;
    public GameObject tmpPrefab;

    [Header("�κ�")]
    public List<GameObject> Robot_Env_001;


    [Header("�κ�����")]
    public RobotState currentState = RobotState.Idle;

    [Header("�̱��� �ּ�ĳ�̿�")]
    private ViewManager viewManager;

    private LineRenderer lineRenderer;
    public GameObject prefab;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (main == null)
        {
            main = FindFirstObjectByType<BA_Main>();
            Debug.LogWarning("main�� Inspector�� ������� �ʾ� FindFirstObjectByType���� ã��.");
        }

        if (autoPainter == null)
        {
            autoPainter = FindFirstObjectByType<AutoPainter>();
            Debug.LogWarning("autoPainter�� Inspector�� ������� �ʾ� FindFirstObjectByType���� ã��.");
        }

        if (cadLoad == null)
        {
            cadLoad = FindFirstObjectByType<CADLoad>();
            Debug.LogWarning("simpleBlockShow�� Inspector�� ������� �ʾ� FindFirstObjectByType���� ã��.");
        }

    }
    private void Start()
    {
        viewManager = ViewManager.Instance;
    }
    #region �� ������ ����

    /// <summary>
    /// BPS ������ ����
    /// </summary>
    public BPSData CreateBPS()
    {
        string bpsDistance = viewManager.Input_QuadBPS.text;
        string bpsMotionGap = viewManager.Input_MotionBPS.text;
     //   string = viewManager.Input_MotionVertical.text;
        string bpsSpeed = viewManager.Input_SPD.text;
        string bpsAccel = viewManager.Input_ACC.text;
        return new BPSData(bpsDistance, bpsMotionGap, bpsSpeed, bpsAccel);
    }

    /// <summary>
    /// XML ������ ����
    /// </summary>
    public XMLData CreateXML()
    {
        string fileNM = viewManager.InputField_XMLFileName.text;
        if (string.IsNullOrEmpty(fileNM))
        {
            fileNM = "motionData"; // �⺻ ���ϸ� ����
        }
        if (!fileNM.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            fileNM += ".xml";
        }
        string motionRmrk = viewManager.InputField_XMLCerateRmrk.text;
        string motionXmlTextData = xmlData.motionXmlTextData;
        string updateContent = "";
        // int incomBlkSeq = cadData.incomBlkSeq;
        int incomBlkSeq = 20;
        viewManager.InputField_XMLFileName.text = "";
        viewManager.InputField_XMLCerateRmrk.text = "";
        return new XMLData( fileNM,  motionRmrk,  motionXmlTextData , updateContent,incomBlkSeq);
    }

    #endregion

    #region �� ������ ���� ��û

    /// <summary>
    /// ������ ���� ��û
    /// </summary>
    public void DataDelete()
    {
        deleteBpsData.Clear();
        if (ViewManager.Instance.seq == -1) return;
        deleteBpsData.Add(ViewManager.Instance.seq);
    }

    #endregion

    #region �� ���� POST ��û

    /// <summary>
    /// BPS ���� ��û
    /// </summary>
    public void BPSPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester �����ϴ�");
        }
        if(requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.BPSPostRequest(requestMappings[num]);
    }

    /// <summary>
    /// CAD ���� ��û
    /// </summary>
    public void CADPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester �����ϴ�");
        }
        if (requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.CADPostRequest(requestMappings[num]);
    }

    /// <summary>
    /// XML ���� ��û
    /// </summary>
    public void XMLPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester �����ϴ�");
        }
        if (requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.XMLPostRequest(requestMappings[num]);
    }
    public void SingleXMLPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester �����ϴ�");
        }
        if (requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.XMLSinglePostRequest(requestMappings[num]);

    }
    #endregion

    #region �� UI ����Ʈ ����

    /// <summary>
    /// BPS ����Ʈ UI ����
    /// </summary>
    public void SetBPSList()
    {
        foreach (var item in dicBpsDatas)
        {
            //Debug.Log(item);
            GameObject obj = Instantiate(tmpPrefab, bPSCanvaTrans);
            obj.SetActive(true);

            var clickEvent = obj.AddComponent<BPSDataClickEvent>();
            clickEvent.SetData(item.Value);

            Button button = obj.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(clickEvent.BtnClick);
            }
            else
            {
                Debug.LogWarning("Button ������Ʈ�� �����տ� �����ϴ�.");
            }

            TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = $"Seq: {item.Value.bpsDataSeq}";
            }
            else
            {
                Debug.LogWarning("TMP_Text�� ������ �ȿ� �����ϴ�.");
            }
        }

    }

    /// <summary>
    /// XML ����Ʈ UI ����
    /// </summary>
    public void SetXMLList()
    {
        foreach (var item in dicXmlDatas)
        {
            GameObject obj = Instantiate(tmpPrefab, xMLCanvaTrans);
            obj.SetActive(true);

            var clickEvent = obj.AddComponent<XMlDataClickEvent>();
            clickEvent.SetData(item.Value);

            Button button = obj.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(clickEvent.BtnClick);
            }
            else
            {
                Debug.LogWarning("Button ������Ʈ�� �����տ� �����ϴ�.");
            }

            TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = $"Seq: {item.Value.fileNm}";
            }
            else
            {
                Debug.LogWarning("TMP_Text�� ������ �ȿ� �����ϴ�.");
            }
        }

    }
   /* public void SavePoseDataToXmlText(int dataId, List<PoseData> poseList)
    {
        string xmlText = ConvertPoseListToXmlString(poseList); // XML ���ڿ��� ��ȯ
        dicXmlDatas[dataId].motionXmlTextData = xmlText;        // XML �ؽ�Ʈ�� ����
    }
*/
  

    public List<Vector3> ParseMotionPathsFromXML(string xml)
    {
        List<Vector3> positions = new List<Vector3>();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        XmlNodeList poseNodes = doc.SelectNodes("//Pose");

        foreach (XmlNode poseNode in poseNodes)
        {
            var gantryNode = poseNode.SelectSingleNode("Ext/PosCmd/Gantry_ExternalAxis");

            if (gantryNode != null)
            {
                float e1 = float.Parse(gantryNode.Attributes["E1"].Value);
                float e2 = float.Parse(gantryNode.Attributes["E2"].Value);
                float e3 = float.Parse(gantryNode.Attributes["E3"].Value);

                e1 = (e1 / 1000f);
                e2 = (e2 / 1000f)+0.5f;
                e3 = (e3 / 1000f)-1.2f;
                Vector3 pos = new Vector3(e1, e3, e2);

                // Y�� ���� 270�� ȸ�� (180�� + 90��)
                Quaternion rotation = Quaternion.AngleAxis(270f, Vector3.up);
                pos = rotation * pos;

                positions.Add(pos);
            }
        }

        return positions;
    }

    /// <summary>
    /// BPS ����Ʈ �ʱ�ȭ
    /// </summary>
    public void ResetBPS()
    {
        foreach (Transform child in bPSCanvaTrans)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// XML ����Ʈ �ʱ�ȭ
    /// </summary>
    public void ResetXML()
    {
        foreach (Transform child in xMLCanvaTrans)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion

    #region �� ��Ÿ ���
    public void LineLoad(int xMLData)
    {

        main.ClearAllGeneratedDataAndVisuals();
        List<PoseData> poseData = ParsePoseData(dicXmlDatas[xMLData].motionXmlTextData);

        List<Vector3> vector3s = ParseMotionPathsFromXML(dicXmlDatas[xMLData].motionXmlTextData);
        Debug.Log(dicXmlDatas[xMLData].motionXmlTextData);

        FindRoot.DrawPath(vector3s, lineRenderer);

      //  objCreate(vector3s);
    }
    public void objCreate(List<Vector3> vector3s)
    {
        // List<MotionPathGenerator.MotionPath> motionPath = Instance.main.generatedPaths;
        foreach (var path in vector3s)
        {
            Instantiate(prefab, path,Quaternion.identity);
        }
    }
    private static List<PoseData> ParsePoseData(string xmlText)
    {
        List<PoseData> poseList = new List<PoseData>();

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xmlText);

        XmlNodeList poseNodes = doc.GetElementsByTagName("Pose");

        foreach (XmlNode poseNode in poseNodes)
        {
            PoseData poseData = new PoseData();
            poseData.id = int.Parse(poseNode.Attributes["ID"].Value);

            XmlNode extNode = poseNode["Ext"];
            if (extNode == null) continue;

            poseData.correctionType.Type = int.Parse(extNode["CorrectionType"].Attributes["Type"].Value);

            XmlNode posCmdNode = extNode["PosCmd"];
            if (posCmdNode != null)
            {
                XmlNode cartNode = posCmdNode["Kuka_Cartesian"];
                poseData.kukaCartesian.X = float.Parse(cartNode.Attributes["X"].Value);
                poseData.kukaCartesian.Y = float.Parse(cartNode.Attributes["Y"].Value);
                poseData.kukaCartesian.Z = float.Parse(cartNode.Attributes["Z"].Value);
                poseData.kukaCartesian.A = float.Parse(cartNode.Attributes["A"].Value);
                poseData.kukaCartesian.B = float.Parse(cartNode.Attributes["B"].Value);
                poseData.kukaCartesian.C = float.Parse(cartNode.Attributes["C"].Value);
                poseData.kukaCartesian.S = int.Parse(cartNode.Attributes["S"].Value);
                poseData.kukaCartesian.T = int.Parse(cartNode.Attributes["T"].Value);

                XmlNode axisNode = posCmdNode["Kuka_AxisSpecific"];
                poseData.kukaAxis.A1 = float.Parse(axisNode.Attributes["A1"].Value);
                poseData.kukaAxis.A2 = float.Parse(axisNode.Attributes["A2"].Value);
                poseData.kukaAxis.A3 = float.Parse(axisNode.Attributes["A3"].Value);
                poseData.kukaAxis.A4 = float.Parse(axisNode.Attributes["A4"].Value);
                poseData.kukaAxis.A5 = float.Parse(axisNode.Attributes["A5"].Value);
                poseData.kukaAxis.A6 = float.Parse(axisNode.Attributes["A6"].Value);

                XmlNode movOptNode = posCmdNode["Kuka_MovOpt"];
                poseData.kukaMovOpt.SPD = float.Parse(movOptNode.Attributes["SPD"].Value);
                poseData.kukaMovOpt.ACC = float.Parse(movOptNode.Attributes["ACC"].Value);
                poseData.kukaMovOpt.TIME = float.Parse(movOptNode.Attributes["TIME"].Value);

                XmlNode gantryNode = posCmdNode["Gantry_ExternalAxis"];
                poseData.gantry.E1 = float.Parse(gantryNode.Attributes["E1"].Value);
                poseData.gantry.E2 = float.Parse(gantryNode.Attributes["E2"].Value);
                poseData.gantry.E3 = float.Parse(gantryNode.Attributes["E3"].Value);

                XmlNode rotiNode = posCmdNode["ROTI_ExternalAxis"];
                poseData.roti.E4 = float.Parse(rotiNode.Attributes["E4"].Value);
                poseData.roti.E5 = float.Parse(rotiNode.Attributes["E5"].Value);
                poseData.roti.SPD = float.Parse(rotiNode.Attributes["SPD"].Value);
                poseData.roti.ACC = float.Parse(rotiNode.Attributes["ACC"].Value);

                XmlNode weavingNode = posCmdNode["Weaving_ExternalAxis"];
                poseData.weaving.ONOFF = float.Parse(weavingNode.Attributes["ONOFF"].Value);
                poseData.weaving.ANGLE_MIN = float.Parse(weavingNode.Attributes["ANGLE_MIN"].Value);
                poseData.weaving.ANGLE_MAX = float.Parse(weavingNode.Attributes["ANGLE_MAX"].Value);
                poseData.weaving.ANGLESPD = float.Parse(weavingNode.Attributes["ANGLESPD"].Value);
            }

            XmlNode ioNode = extNode["IO_Status"];
            poseData.ioStatus.MODE = int.Parse(ioNode.Attributes["MODE"].Value);
            poseData.ioStatus.PRE_AFTER_FLOW_TIME = int.Parse(ioNode.Attributes["PRE_AFTER_FLOW_TIME"].Value);

            XmlNode flagsNode = extNode["Flags"];
            poseData.flags.FLAG1 = int.Parse(flagsNode.Attributes["FLAG1"].Value);
            poseData.flags.FLAG2 = int.Parse(flagsNode.Attributes["FLAG2"].Value);

            XmlNode coordNode = extNode["Coordinate"];
            poseData.coordinate.SELECT = int.Parse(coordNode.Attributes["SELECT"].Value);

            poseList.Add(poseData);
        }

        return poseList;
    }
    /// <summary>
    /// ���õ� XML ������ ��ȯ
    /// </summary>
    public XMLData SelectXMLData()
    {
        return dicXmlDatas[viewManager.seq];
    }

    /// <summary>
    /// �κ� ���� ����(ȣ���)
    /// </summary>
    public void ChangeState(RobotState robotState)
    {
        switch (robotState)
        {
            case RobotState.Working:
                Debug.Log("�κ� ����: �۵� ��");
                break;
            case RobotState.Idle:
                break;
            case RobotState.Moving:
                break;
            case RobotState.Error:
                Debug.Log("�κ� ����: ���� �߻�");
                break;
        }
        currentState = robotState;
        EnterState(currentState);
    }
    /// <summary>
    /// �κ� ������ �̺�Ʈ
    /// </summary>
    private void EnterState(RobotState robotState)
    {
        switch (robotState)
        {
            case RobotState.Working:
                autoPainter.ShootPaint(true); // �л����
                Debug.Log("�κ� ����: �۵� ��");
                break;
            case RobotState.Idle:
                autoPainter.ShootPaint(false); // �л����
                break;
            case RobotState.Moving:
                break;
            case RobotState.Error:
                Debug.Log("�κ� ����: ���� �߻�");
                break;
        }
    }

    public void ChangeToWorking() // �׽�Ʈ�� 
    {
        ChangeState(RobotState.Working);
    }
    /// <summary>
    /// MeshCollider ����
    /// </summary>
    public void SetCollider(MeshCollider collider)
    {
        cadCollider = collider;
    }

    /// <summary>
    /// MeshCollider ��ȯ
    /// </summary>
    public MeshCollider GetCollider()
    {
        return cadCollider == null ? null : cadCollider;
    }


    #endregion


  
}
[System.Serializable]
public class XMLRequestMapping
{
    public string url;
    public RequestType requestType;
}
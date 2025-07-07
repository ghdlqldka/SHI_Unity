
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
    Select,   // 조회
    Insert,   // 삽입
    Update,   // 수정
    Delete    // 삭제
}
public enum RobotState
{
    Working,   // 작동 중
    Idle,      // 대기 중
    Moving,    // 이동 중
    Error      // 에러 상태
}
public class SystemController : Singleton<SystemController>
{
    [Header("레이어 설정")]
    public LayerMask clickableLayer; // 클릭 가능한 레이어

    [Header("스크립트 참조")]
    public BA_Main main;
    public AutoPainter autoPainter;
    public CADLoad cadLoad;
    public XMLSender xMLSender;
    public XML_Importer_ssm xMLImporter;
    public DataRequester dataRequester;
    


    [Header("CAD,BPS,XML 데이터")]
     public MeshCollider cadCollider;
    [HideInInspector] public Dictionary<int,BPSData> dicBpsDatas;
    [HideInInspector] public Dictionary<int, XMLData> dicXmlDatas;
    [HideInInspector] public List<int> deleteBpsData;
    [HideInInspector] public CADData cadData;
    [HideInInspector] public XMLData xmlData;
    [Header("XML 요청 매핑")]
    public List<XMLRequestMapping> requestMappings = new List<XMLRequestMapping>();

    [Header("UI 요소")]
    public Transform xMLCanvaTrans;
    public Transform bPSCanvaTrans;
    public GameObject tmpPrefab;

    [Header("로봇")]
    public List<GameObject> Robot_Env_001;


    [Header("로봇상태")]
    public RobotState currentState = RobotState.Idle;

    [Header("싱글톤 주소캐싱용")]
    private ViewManager viewManager;

    private LineRenderer lineRenderer;
    public GameObject prefab;
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (main == null)
        {
            main = FindFirstObjectByType<BA_Main>();
            Debug.LogWarning("main이 Inspector에 연결되지 않아 FindFirstObjectByType으로 찾음.");
        }

        if (autoPainter == null)
        {
            autoPainter = FindFirstObjectByType<AutoPainter>();
            Debug.LogWarning("autoPainter가 Inspector에 연결되지 않아 FindFirstObjectByType으로 찾음.");
        }

        if (cadLoad == null)
        {
            cadLoad = FindFirstObjectByType<CADLoad>();
            Debug.LogWarning("simpleBlockShow가 Inspector에 연결되지 않아 FindFirstObjectByType으로 찾음.");
        }

    }
    private void Start()
    {
        viewManager = ViewManager.Instance;
    }
    #region ▶ 데이터 생성

    /// <summary>
    /// BPS 데이터 생성
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
    /// XML 데이터 생성
    /// </summary>
    public XMLData CreateXML()
    {
        string fileNM = viewManager.InputField_XMLFileName.text;
        if (string.IsNullOrEmpty(fileNM))
        {
            fileNM = "motionData"; // 기본 파일명 설정
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

    #region ▶ 데이터 삭제 요청

    /// <summary>
    /// 데이터 삭제 요청
    /// </summary>
    public void DataDelete()
    {
        deleteBpsData.Clear();
        if (ViewManager.Instance.seq == -1) return;
        deleteBpsData.Add(ViewManager.Instance.seq);
    }

    #endregion

    #region ▶ 서버 POST 요청

    /// <summary>
    /// BPS 서버 요청
    /// </summary>
    public void BPSPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester 없습니다");
        }
        if(requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.BPSPostRequest(requestMappings[num]);
    }

    /// <summary>
    /// CAD 서버 요청
    /// </summary>
    public void CADPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester 없습니다");
        }
        if (requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.CADPostRequest(requestMappings[num]);
    }

    /// <summary>
    /// XML 서버 요청
    /// </summary>
    public void XMLPostRequest(int num)
    {
        if (dataRequester == null)
        {
            Debug.LogError("dataRequester 없습니다");
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
            Debug.LogError("dataRequester 없습니다");
        }
        if (requestMappings[num].requestType == RequestType.Delete)
        {
            DataDelete();
        }
        dataRequester.XMLSinglePostRequest(requestMappings[num]);

    }
    #endregion

    #region ▶ UI 리스트 갱신

    /// <summary>
    /// BPS 리스트 UI 세팅
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
                Debug.LogWarning("Button 컴포넌트가 프리팹에 없습니다.");
            }

            TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = $"Seq: {item.Value.bpsDataSeq}";
            }
            else
            {
                Debug.LogWarning("TMP_Text가 프리팹 안에 없습니다.");
            }
        }

    }

    /// <summary>
    /// XML 리스트 UI 세팅
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
                Debug.LogWarning("Button 컴포넌트가 프리팹에 없습니다.");
            }

            TMP_Text tmpText = obj.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = $"Seq: {item.Value.fileNm}";
            }
            else
            {
                Debug.LogWarning("TMP_Text가 프리팹 안에 없습니다.");
            }
        }

    }
   /* public void SavePoseDataToXmlText(int dataId, List<PoseData> poseList)
    {
        string xmlText = ConvertPoseListToXmlString(poseList); // XML 문자열로 변환
        dicXmlDatas[dataId].motionXmlTextData = xmlText;        // XML 텍스트로 저장
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

                // Y축 기준 270도 회전 (180도 + 90도)
                Quaternion rotation = Quaternion.AngleAxis(270f, Vector3.up);
                pos = rotation * pos;

                positions.Add(pos);
            }
        }

        return positions;
    }

    /// <summary>
    /// BPS 리스트 초기화
    /// </summary>
    public void ResetBPS()
    {
        foreach (Transform child in bPSCanvaTrans)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// XML 리스트 초기화
    /// </summary>
    public void ResetXML()
    {
        foreach (Transform child in xMLCanvaTrans)
        {
            Destroy(child.gameObject);
        }
    }

    #endregion

    #region ▶ 기타 기능
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
    /// 선택된 XML 데이터 반환
    /// </summary>
    public XMLData SelectXMLData()
    {
        return dicXmlDatas[viewManager.seq];
    }

    /// <summary>
    /// 로봇 상태 변경(호출용)
    /// </summary>
    public void ChangeState(RobotState robotState)
    {
        switch (robotState)
        {
            case RobotState.Working:
                Debug.Log("로봇 상태: 작동 중");
                break;
            case RobotState.Idle:
                break;
            case RobotState.Moving:
                break;
            case RobotState.Error:
                Debug.Log("로봇 상태: 에러 발생");
                break;
        }
        currentState = robotState;
        EnterState(currentState);
    }
    /// <summary>
    /// 로봇 상태후 이벤트
    /// </summary>
    private void EnterState(RobotState robotState)
    {
        switch (robotState)
        {
            case RobotState.Working:
                autoPainter.ShootPaint(true); // 분사시작
                Debug.Log("로봇 상태: 작동 중");
                break;
            case RobotState.Idle:
                autoPainter.ShootPaint(false); // 분사시작
                break;
            case RobotState.Moving:
                break;
            case RobotState.Error:
                Debug.Log("로봇 상태: 에러 발생");
                break;
        }
    }

    public void ChangeToWorking() // 테스트용 
    {
        ChangeState(RobotState.Working);
    }
    /// <summary>
    /// MeshCollider 설정
    /// </summary>
    public void SetCollider(MeshCollider collider)
    {
        cadCollider = collider;
    }

    /// <summary>
    /// MeshCollider 반환
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
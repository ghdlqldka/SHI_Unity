using System;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ViewManager : Singleton<ViewManager>
{
    private static string LOG_FORMAT = "<color=#BF2525><b>[ViewManager]</b></color> {0}";

    [Header("BPS")]
    public TMP_InputField Input_QuadBPS;
    public TMP_InputField Input_MotionBPS;
    public TMP_InputField Input_MotionVertical;
    public TMP_InputField Input_MotionHorizontal;
    public TMP_InputField Input_SPD;
    public TMP_InputField Input_ACC;

    [Header("XML")]
    public TextMeshProUGUI XMLFileName;
    public TMP_InputField InputField_XMLRmrk;
    public TMP_InputField InputField_XMLFileName;
    public TMP_InputField InputField_XMLCerateRmrk;
    
    /*
    [Header("kukaRot")]
    public Slider A2;
    public Slider A3;
    public Slider A4;
    public Slider A5;
    public Slider A6;
    */

    public int seq = -1;
    private IBpsData currentData;
    private SystemController systemController;
    private void Awake()
    {
        //
    }
    private void Start()
    {
        systemController = SystemController.Instance;


        // A2.onValueChanged.AddListener((v) => systemController.Robot_Env_001[8].transform.localRotation = Quaternion.Euler(0, v, 0));
        // A3.onValueChanged.AddListener((v) => systemController.Robot_Env_001[9].transform.localRotation = Quaternion.Euler(0, v, 0));
        // A4.onValueChanged.AddListener((v) => systemController.Robot_Env_001[10].transform.localRotation = Quaternion.Euler(v, 0, 0));
        // A5.onValueChanged.AddListener((v) => systemController.Robot_Env_001[11].transform.localRotation = Quaternion.Euler(0, v, 0));
        // A6.onValueChanged.AddListener((v) => systemController.Robot_Env_001[12].transform.localRotation = Quaternion.Euler(v, 0, 0));
        // Joint 인덱스를 알고 있다고 가정
    }

    public void ChangeA1(float v)
    {
        Debug.Log("A1 value: " + v);
        var t = systemController.Robot_Env_001[7];
        Debug.Log("Obj: " + t);
        t.transform.localRotation = Quaternion.Euler(-90, 0, v);
    }

    public void ChangeA2(float v)
    {
        systemController.Robot_Env_001[8].transform.localRotation = Quaternion.Euler(0, v, 0);
    }

    public void ChangeA3(float v)
    {
        systemController.Robot_Env_001[9].transform.localRotation = Quaternion.Euler(0, v, 0);
    }

    public void ChangeA4(float v)
    {
        systemController.Robot_Env_001[10].transform.localRotation = Quaternion.Euler(v, 0, 0);
    }

    public void ChangeA5(float v)
    {
        systemController.Robot_Env_001[11].transform.localRotation = Quaternion.Euler(0, v, 0);
    }

    public void ChangeA6(float v)
    {
        systemController.Robot_Env_001[12].transform.localRotation = Quaternion.Euler(v, 0, 0);
    }

    public void biodoff()
    {
        systemController.Robot_Env_001[3].GetComponent<BioIK.BioIK>().enabled = false; // Enable BioIK for the robot arm
    }
    public void biodon()
    {
        systemController.Robot_Env_001[3].GetComponent<BioIK.BioIK>().enabled = true; // Enable BioIK for the robot arm
    }
    
    public void UpdateIfBps(Action<BPSData> update)
    {
        if (currentData is BPSData bps)
            update(bps);
    }

    public void UpdateIfXml(Action<XMLData> update)
    {
        if (currentData is XMLData xml)
            update(xml);
    }



    public void OnQuadBPSEdit(string v)
    {
        UpdateIfBps(delegate (BPSData bps)
        {
            bps.bpsDistance = v;
        });
    }

    public void OnMotionBPSEdit(string v)
    {
        UpdateIfBps(delegate (BPSData bps)
        {
            bps.bpsMotionGap = v;
        });
    }

    public void OnSPDEdit(string v)
    {
        UpdateIfBps(delegate (BPSData bps)
        {
            bps.bpsSpeed = v;
        });
    }

    public void OnACCEdit(string v)
    {
        UpdateIfBps(delegate (BPSData bps)
        {
            bps.bpsAccel = v;
        });
    }

    public void OnXmlRemarkEdit(string v)
    {
        UpdateIfXml(delegate (XMLData xml)
        {
            xml.motionRmrk = v;
        });
    }

    public void SetInput(IBpsData data)
    {
        currentData = data;

        if (data is BPSData bps)
        {
            seq = bps.bpsDataSeq;
            SetInputBps(bps);
        }
        else if (data is XMLData xml)
        {
            systemController.xmlData = xml;
            seq = xml.motionXmlSeq;
            SetInputXml(xml);
        }
    }

    private void SetInputBps(BPSData bps)
    {
        Input_QuadBPS.text = bps.bpsDistance;
        Input_MotionBPS.text = bps.bpsSpeed;
        Input_MotionVertical.text = bps.bpsFaceInfo;
        Input_SPD.text = bps.bpsAccel;
    }

    private void SetInputXml(XMLData xml)
    {
        Debug.LogFormat(LOG_FORMAT, "SendInputXml()");
        InputField_XMLRmrk.text = xml.motionRmrk;
        XMLFileName.text = xml.fileNm;
    }

    public void XMLCreateCancel(GameObject go)
    {
        InputField_XMLFileName.text = "";
        InputField_XMLCerateRmrk.text = "";
        go.SetActive(false);
    }

}

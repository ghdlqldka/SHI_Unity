using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class XML_Importer_ssm : MonoBehaviour
{
    public TMP_Text text_Twindata;

    [Header("Make_XML ����")]
    public Make_XML makeXml;

    // private string lastXmlData = "";
    [Header("�����")]
    public bool debugMode = true;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SocketIO_Init();

    [DllImport("__Internal")]
    private static extern void SocketIO_SendMessage(string message);
#endif
    private SystemController robotSystem ;
    void Start()
    {

        robotSystem = SystemController.Instance;

#if UNITY_WEBGL && !UNITY_EDITOR
        SocketIO_Init(); // WebGL ȯ�濡�� Socket.IO ���� �õ�
#endif

    }

    public void SendMessageToServer(string message)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SocketIO_SendMessage(message);
#endif
    }

    public void OnSocketConnected(string msg)
    {
        Debug.Log("���� ���� ����: " + msg);
    }

    public void OnSocketMessage(string message)
    {
       
        Debug.Log("WebGL���� XML �޽��� ����: " + message);
        try
        {
 
            // 1. JSON �Ľ�
            var incoming = JsonUtility.FromJson<IncomingRobotMessage>(message);
            string xmlString = incoming.XMLdata.Trim();

            // 2. XML �Ľ�
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            
            XmlNode dataNode = xmlDoc.SelectSingleNode("Robot/Data");

            // 3. FK
            XmlNode fkNode = dataNode.SelectSingleNode("ActPos_Kuka_FK");
            var fk = new ActPosKukaFK
            {
                TH1 = double.Parse(fkNode.Attributes["TH1"].Value, CultureInfo.InvariantCulture),
                TH2 = double.Parse(fkNode.Attributes["TH2"].Value, CultureInfo.InvariantCulture),
                TH3 = double.Parse(fkNode.Attributes["TH3"].Value, CultureInfo.InvariantCulture),
                TH4 = double.Parse(fkNode.Attributes["TH4"].Value, CultureInfo.InvariantCulture),
                TH5 = double.Parse(fkNode.Attributes["TH5"].Value, CultureInfo.InvariantCulture),
                TH6 = double.Parse(fkNode.Attributes["TH6"].Value, CultureInfo.InvariantCulture),
            };

            // 4. ExtAxis
            XmlNode extAxisNode = dataNode.SelectSingleNode("ActPos_ExtAxis");
            var extAxis = new ActPosExtAxis
            {
                E1 = double.Parse(extAxisNode.Attributes["E1"].Value, CultureInfo.InvariantCulture),
                E2 = double.Parse(extAxisNode.Attributes["E2"].Value, CultureInfo.InvariantCulture),
                E3 = double.Parse(extAxisNode.Attributes["E3"].Value, CultureInfo.InvariantCulture),
                E4 = double.Parse(extAxisNode.Attributes["E4"].Value, CultureInfo.InvariantCulture),
                E5 = double.Parse(extAxisNode.Attributes["E5"].Value, CultureInfo.InvariantCulture),
                E6 = double.Parse(extAxisNode.Attributes["E6"].Value, CultureInfo.InvariantCulture),
            };
            Robot robotData = new Robot
            {
                Data = new RobotData
                {
                    FK = fk,
                    ExtAxis = extAxis
                }
            };

            // ����: ��ġ �� ȸ�� ���� (���� ���� �״�� ����)


            robotSystem.Robot_Env_001[0].transform.localPosition = SetPosData(0, robotData.Data.ExtAxis.E1, "x");
            robotSystem.Robot_Env_001[1].transform.localPosition = SetPosData(1, robotData.Data.ExtAxis.E2, "y");
            robotSystem.Robot_Env_001[2].transform.localPosition = SetPosData(2, robotData.Data.ExtAxis.E3, "z");
            robotSystem.Robot_Env_001[4].transform.localRotation = Quaternion.Euler(0,0, (float)robotData.Data.ExtAxis.E4);
            robotSystem.Robot_Env_001[5].transform.localRotation = Quaternion.Euler((float)robotData.Data.ExtAxis.E5, 0,0);
            robotSystem.Robot_Env_001[13].transform.localRotation = Quaternion.Euler(0, 0, (float)robotData.Data.ExtAxis.E6);
     
            robotSystem.Robot_Env_001[7].transform.localRotation = Quaternion.Euler(-90, 0, (float)robotData.Data.FK.TH1);
            robotSystem.Robot_Env_001[8].transform.localRotation = Quaternion.Euler(0, (float)robotData.Data.FK.TH2, 0);
            robotSystem.Robot_Env_001[9].transform.localRotation = Quaternion.Euler(0, (float)robotData.Data.FK.TH3, 0);
            robotSystem.Robot_Env_001[10].transform.localRotation = Quaternion.Euler((float)robotData.Data.FK.TH4, 0,0);
            robotSystem.Robot_Env_001[11].transform.localRotation = Quaternion.Euler(0, (float)robotData.Data.FK.TH5, 0);
            robotSystem.Robot_Env_001[12].transform.localRotation = Quaternion.Euler((float)robotData.Data.FK.TH6, 0, 0);



        }
        catch (System.Exception ex)
        {
            Debug.LogError("WebGL���� XML �Ľ� ����: " + ex.Message);
        }
    }

    public void SetRobotData(int i, double value, string axis)
    {
        Transform target = robotSystem.Robot_Env_001[i].transform;
        float angle = (float)value;

        // ���� ����ȭ
        angle = NormalizeAngle(angle);

        // �ַ̱���Ƽ ����(�ʿ��ϸ�)
        if (axis == "x")
        {
            angle = Mathf.Clamp(angle, -85f, 85f);
        }

        // ���� ȸ���� Quaternion���� Euler�� ��ȯ
        Vector3 currentEuler = target.localRotation.eulerAngles;
        // �� �ุ ����
        switch (axis.ToLower())
        {
            case "x":
                currentEuler.x = angle;
                if (currentEuler.x == target.localRotation.eulerAngles.x) return;
                Debug.Log($"Before set: {target.localRotation.eulerAngles.x}, After set: {currentEuler.x}");
                break;
            case "y":
                currentEuler.y = angle;
                if (currentEuler.y == target.localRotation.eulerAngles.y) return;
                Debug.Log($"Before set: {target.localRotation.eulerAngles.y}, After set: {currentEuler.y}");
                break;
            case "z":
                currentEuler.z = angle;
                if (Quaternion.Euler(currentEuler).z == target.localRotation.eulerAngles.z) return;
                Debug.Log($"Before set: {Quaternion.Euler(currentEuler).z}, After set: {currentEuler.x}");
                break;
            default:
                Debug.LogWarning("Unknown axis: " + axis);
                return;
        }


        // ����� Euler ������ Quaternion ���� �� �Ҵ�
        target.localRotation = Quaternion.Euler(currentEuler);

    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;  // 360���� ���� ������ ���ϱ� (������ ó����)

        if (angle > 180f)
            angle -= 360f;
        else if (angle < -180f)
            angle += 360f;

        return angle;
    }
    public Vector3 SetPosData(int i, double value, string str)
    {
        switch (str)
        {
            case "x":
                return new Vector3((float)value, robotSystem.Robot_Env_001[i].transform.localPosition.y, robotSystem.Robot_Env_001[i].transform.localPosition.z);

            case "y":
                return new Vector3(robotSystem.Robot_Env_001[i].transform.localPosition.x, (float)value, robotSystem.Robot_Env_001[i].transform.localPosition.z);

            case "z":
                return new Vector3(robotSystem.Robot_Env_001[i].transform.localPosition.x, robotSystem.Robot_Env_001[i].transform.localPosition.y, (float)value);

            default:
                return new Vector3(0, 0, 0);

        }

    }
    public static Robot ParseRobotXml(string xmlContent)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Robot));
        using (StringReader reader = new StringReader(xmlContent))
        {
            return (Robot)serializer.Deserialize(reader);
        }
    }
}

[System.Serializable]
public class IncomingRobotMessage
{
    public string XMLdata;
    public string timestamp;
    public string type;
}
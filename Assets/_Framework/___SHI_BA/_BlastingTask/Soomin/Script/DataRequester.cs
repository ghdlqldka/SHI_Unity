using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class DataRequester : MonoBehaviour
{
    [Header("기본 서버 설정")]
    private string saveServerUrl = "";
    // private string defaultServerUrl = "http://192.168.0.83:5001/unity/api/xml/dbUpload";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // HTTP 연결 허용 (개발용)
        System.Net.ServicePointManager.ServerCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;
#if UNITY_WEBGL && !UNITY_EDITOR

    string url = Application.absoluteURL;
    Debug.Log("원래 URL: " + url);

    Uri uri = new Uri(url);

    // 5001 포트와 경로로 서버 URL 생성
    saveServerUrl = uri.Scheme + "://" + uri.Host+":5001/";

    Debug.Log("최종 서버 URL: " + saveServerUrl);

#endif
    }
    /// <summary>
    /// BPS 데이터 요청
    /// </summary>
    public void BPSPostRequest(XMLRequestMapping requestMapping)
    {
        string path = saveServerUrl + requestMapping.url;
        StartCoroutine(SendBPSPostRequest(path, requestMapping.requestType));
    }
    /// <summary>
    /// CAD OBJ 파일 요청
    /// </summary>
    public void CADPostRequest(XMLRequestMapping requestMapping)
    {
        string path = saveServerUrl + requestMapping.url;

        StartCoroutine(SendCADPostRequest(path, requestMapping.requestType));
    }
    /// <summary>
    /// XML 데이터 요청
    /// </summary>
    public void XMLPostRequest(XMLRequestMapping requestMapping)
    {
        string path = saveServerUrl + requestMapping.url;
        StartCoroutine(SendXMLPostRequest(path, requestMapping.requestType));
    }
    /// <summary>
    /// XML 데이터 요청
    /// </summary>
    public void XMLSinglePostRequest(XMLRequestMapping requestMapping)
    {
        string path = saveServerUrl + requestMapping.url;
        StartCoroutine(SendSingleXMLPostRequest(path, requestMapping.requestType));
    }
    /// <summary>
    /// BPS 데이터 전송 및 응답 처리
    /// </summary>
    private IEnumerator SendBPSPostRequest(string serverUrl, RequestType type)
    {
#if UNITY_EDITOR
        serverUrl = "http://192.168.0.15:5001/" + serverUrl;
#endif
        // Debug.Log(serverUrl);
        string json;

        if (type == RequestType.Update)
        {
            DataWrapper<BPSData> bpsDataWrapper = new DataWrapper<BPSData>(SystemController.Instance.dicBpsDatas.Values.ToList());
            json = JsonUtility.ToJson(bpsDataWrapper);
            //  Debug.Log(json);
        }
        else if (type == RequestType.Delete)
        {
            DataWrapper<int> deleteBpsData = new DataWrapper<int>(SystemController.Instance.deleteBpsData);
            json = JsonUtility.ToJson(deleteBpsData);
            //  Debug.Log(json);
        }
        else if (type == RequestType.Insert)
        {
            var wrapper = SystemController.Instance.CreateBPS();
            json = JsonUtility.ToJson(wrapper);
        }
        else
        {
            var wrapper = SystemController.Instance.CreateBPS();
            json = JsonUtility.ToJson(wrapper);
            //   Debug.Log(json);

        }

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;

                // JSON 구조: { "data": [ ... ] }

                Wrapper<BPSData> responseWrapper = JsonUtility.FromJson<Wrapper<BPSData>>(responseJson);
                if (responseWrapper != null && responseWrapper.data != null)
                {
                    //  Debug.Log(responseJson);
                    SystemController.Instance.ResetBPS();
                    //  SystemController.Instance.bpsDatas = new List<BPSData>(responseWrapper.data);
                    SystemController.Instance.dicBpsDatas = responseWrapper.data.ToDictionary(x => x.bpsDataSeq);
                    SystemController.Instance.SetBPSList();
                }
                else
                {
                    if (type != RequestType.Select)
                        SystemController.Instance.BPSPostRequest(1);
                    else
                        Debug.Log("JSON 파싱 실패 또는 data가 null입니다.");
                }
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    /// <summary>
    /// CAD OBJ 파일 전송 및 응답 처리
    /// </summary>
    private IEnumerator SendCADPostRequest(string serverUrl, RequestType type)
    {
#if UNITY_EDITOR
        serverUrl = "http://192.168.0.15:5001/" + serverUrl;
#endif
        // string jsonData = JsonUtility.ToJson(type);
        byte[] bodyRaw = new byte[0];

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] responseData = request.downloadHandler.data;  // 바이너리 데이터
                if (responseData == null || responseData.Length == 0)
                {
                    Debug.LogWarning("CAD 응답 데이터가 비어 있습니다.");
                }
                else
                {
                    Debug.Log("Received bytes: " + responseData.Length);
                    SystemController.Instance.cadLoad.ShowBlockLoad(responseData);
                }

            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    /// <summary>
    /// XML 데이터 전송 및 응답 처리
    /// </summary>
    private IEnumerator SendXMLPostRequest(string serverUrl, RequestType type)
    {
#if UNITY_EDITOR
        serverUrl = "http://192.168.0.15:5001/" + serverUrl;
#endif
        // Debug.Log(serverUrl);
        string json;

        if (type == RequestType.Update)
        {
            XMLData xmlDataWrapper = SystemController.Instance.SelectXMLData();
            Debug.Log(xmlDataWrapper.motionRmrk);
            json = JsonUtility.ToJson(xmlDataWrapper);
            Debug.Log(json);
        }
        else if (type == RequestType.Delete)
        {
            DataWrapper<int> deleteBpsData = new DataWrapper<int>(SystemController.Instance.deleteBpsData);
            json = JsonUtility.ToJson(deleteBpsData);
            Debug.Log(json);
        }
        else if (type == RequestType.Insert)
        {
            List<string> allXmlFile = SystemController.Instance.xMLSender.GetAllXmlFiles();
            List<XMLData> xmlData = new List<XMLData>();
            foreach (string fileName in allXmlFile)
            {
                string motionXmlTextData = SystemController.Instance.xMLSender.ReadXmlFileContent(fileName);
                XMLData wrapper = new XMLData(fileName, "", motionXmlTextData, "", 20);
                //20은 수정필요 = cad데이터
                xmlData.Add(wrapper);

            }
            DataWrapper<XMLData> dataWrapper = new DataWrapper<XMLData>(xmlData);
            json = JsonUtility.ToJson(dataWrapper);
            Debug.Log(json);
        }
        else
        {
            var wrapper = new CADData(20);
            json = JsonUtility.ToJson(wrapper);
            //  Debug.Log(json);

        }

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);


        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;

                //xDebug.Log(responseJson);
                Wrapper<XMLData> responseWrapper = JsonUtility.FromJson<Wrapper<XMLData>>(responseJson);
                if (responseWrapper != null && responseWrapper.data != null)
                {
                    SystemController.Instance.ResetXML();
                    //SystemController.Instance.xmlDatas = new List<XMLData>(responseWrapper.data);
                    SystemController.Instance.dicXmlDatas = responseWrapper.data.ToDictionary(x => x.motionXmlSeq);
                    // 2. 각 XML 텍스트를 파싱해 MotionPath 리스트로 변환 후 저장

                    //objCreate();
                    SystemController.Instance.SetXMLList();
                    if (type != RequestType.Select)
                    {
                        SystemController.Instance.XMLPostRequest(5);
                    }
                }
                else
                    Debug.Log("JSON 파싱 실패 또는 data가 null입니다.");
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
    /// <summary>
    /// XML 데이터 전송 및 응답 처리
    /// </summary>
    private IEnumerator SendSingleXMLPostRequest(string serverUrl, RequestType type)
    {
#if UNITY_EDITOR
        serverUrl = "http://192.168.0.15:5001/" + serverUrl;
#endif

        // Debug.Log(serverUrl)
        SystemController.Instance.main.xmlRecorder.RecordCurrentPose(SystemController.Instance.main.makeXmlInstance);

        string finalXmlString = SystemController.Instance.main.makeXmlInstance.ConvertPoseListToXmlString(SystemController.Instance.main.xmlRecorder.AllPoses);
        Debug.Log(finalXmlString);
        List<XMLData> xmlData = new List<XMLData>();
       
   
        XMLData wrapper = new XMLData("SingleMotion_XML", "", finalXmlString, "", 20);
        //20은 수정필요 = cad데이터
        xmlData.Add(wrapper);

        
        DataWrapper<XMLData> dataWrapper = new DataWrapper<XMLData>(xmlData);

        string json = JsonUtility.ToJson(dataWrapper);
        Debug.Log(json);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);


        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;

                //xDebug.Log(responseJson);
                Wrapper<XMLData> responseWrapper = JsonUtility.FromJson<Wrapper<XMLData>>(responseJson);
                if (responseWrapper != null && responseWrapper.data != null)
                {
                    SystemController.Instance.ResetXML();
                    //SystemController.Instance.xmlDatas = new List<XMLData>(responseWrapper.data);
                    SystemController.Instance.dicXmlDatas = responseWrapper.data.ToDictionary(x => x.motionXmlSeq);
                    // 2. 각 XML 텍스트를 파싱해 MotionPath 리스트로 변환 후 저장

                    //objCreate();
                    SystemController.Instance.SetXMLList();
                    if (type != RequestType.Select)
                    {
                        SystemController.Instance.XMLPostRequest(5);
                    }
                }
                else
                    Debug.Log("JSON 파싱 실패 또는 data가 null입니다.");
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }

}
[System.Serializable]
public class DataWrapper<T>
{
    public List<T> data;

    public DataWrapper(List<T> data)
    {
        this.data = data;
    }
}
[System.Serializable]
public class SingleDataWrapper<T>
{
    public T data;

    public SingleDataWrapper(T data)
    {
        this.data = data;
    }
}

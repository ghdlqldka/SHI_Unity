using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Xml;
// XMLRequestData 클래스 정의 (서버에서 요구하는 필드 포함)
[System.Serializable]
public class XMLRequestData
{
    public string motionXmlTextData;
    public string fileNm;
    public string motionRegDt;
    public string robotId;
    public string commandType;
    public string motionRmrk;
}

public class XMLSender : MonoBehaviour
{
    [Header("기본 설정")]
    public string robotId = "robot_01";
    public string commandType = "motion";
    public string Rmrk = "모션 테스트 데이터 입니다.";

    [Header("UI 참조")]
    public TMP_InputField Textbox_SeverPath; // 서버 경로 Input Field
    // public TMP_InputField Input_XMLname; // XML 파일명 Input Field
    public string _ui_fileName;

    [Header("Make_XML 연결")]
    public Make_XML makeXml; // Make_XML 스크립트 참조


    [Header("기본 서버 설정")]
#if UNITY_WEBGL && !UNITY_EDITOR
    private string saveServerUrl = "";
#endif
    private string defaultServerUrl = "http://192.168.0.83:5001/unity/api/xml/dbUpload";

    private string DefaultXmlFilename
    {
        get
        {
            // Input_XMLname 필드가 있고 비어있지 않으면 그 값을 사용
            // if (Input_XMLname != null && !string.IsNullOrEmpty(Input_XMLname.text))
            if (string.IsNullOrEmpty(_ui_fileName) == false)
            {
                string fileName = _ui_fileName;
                // .xml 확장자가 없으면 추가
                if (!fileName.EndsWith(".xml"))
                    fileName += ".xml";
                return fileName;
            }
            return "unity_motion.xml"; // 기본값
        }
    }

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

    // #region XML 파일 읽기

    /// <summary>
    /// 지정된 XML 파일의 내용을 읽어오기
    /// </summary>
    public string ReadXmlFileContent(string fileName)
    {
        if (makeXml == null)
        {
            Debug.LogError("[XMLSender] Make_XML이 연결되지 않았습니다!");
            return null;
        }

        string folderPath = Path.Combine(Application.streamingAssetsPath, makeXml.xmlSaveFolder);
        string filePath = Path.Combine(folderPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"[XMLSender] 파일을 찾을 수 없습니다: {filePath}");
            return null;
        }

        try
        {
            string xmlContent = File.ReadAllText(filePath);
            Debug.Log($"[XMLSender] XML 파일 내용 읽기 성공: {fileName}");
            return xmlContent;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[XMLSender] XML 파일 읽기 실패: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 가장 최근에 생성된 XML 파일 찾기
    /// </summary>
    private string GetLatestXmlFile()
    {
        if (makeXml == null) return null;

        string folderPath = Path.Combine(Application.streamingAssetsPath, makeXml.xmlSaveFolder);

        if (!Directory.Exists(folderPath)) return null;

        var xmlFiles = Directory.GetFiles(folderPath, "*.xml")
                               .OrderByDescending(f => File.GetLastWriteTime(f))
                               .ToArray();

        return xmlFiles.Length > 0 ? Path.GetFileName(xmlFiles[0]) : null;
    }
    public void ClearExistingXmlFiles()
    {
        if (makeXml == null)
        {
            Debug.LogError("[XmlPathRecorder] Make_XML 참조가 없어 파일 삭제를 진행할 수 없습니다.");
            return;
        }

        string folderPath = Path.Combine(Application.streamingAssetsPath, makeXml.xmlSaveFolder);

        if (Directory.Exists(folderPath))
        {
            try
            {
                string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");
                foreach (string filePath in xmlFiles)
                {
                    File.Delete(filePath);
                }

                if (xmlFiles.Length > 0)
                {
                    Debug.Log($"[XmlPathRecorder] {folderPath} 폴더의 기존 XML 파일 {xmlFiles.Length}개를 모두 삭제했습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[XmlPathRecorder] XML 파일 삭제 중 오류 발생: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"[XmlPathRecorder] {folderPath} 폴더가 존재하지 않아 새로 생성합니다.");
            Directory.CreateDirectory(folderPath);
        }
    }
    /// <summary>
    /// XML 폴더의 모든 XML 파일 찾기 (파일명 순으로 정렬)
    /// </summary>
    public List<string> GetAllXmlFiles()
    {
        List<string> xmlFileNames = new List<string>();

        if (makeXml == null) return xmlFileNames;

        string folderPath = Path.Combine(Application.streamingAssetsPath, makeXml.xmlSaveFolder);

        if (!Directory.Exists(folderPath)) return xmlFileNames;

        var xmlFiles = Directory.GetFiles(folderPath, "*.xml")
                               .OrderBy(f => Path.GetFileName(f))
                               .ToArray();

        foreach (string filePath in xmlFiles)
        {
            xmlFileNames.Add(Path.GetFileName(filePath));
        }
        return xmlFileNames;
    }

    /// <summary>
    /// 번호가 붙은 파일명 생성 (motion.xml → motion_1.xml)
    /// </summary>
    private string GetNumberedFileName(int number)
    {
        string baseName = "";

        // Input_XMLname 필드가 있고 비어있지 않으면 그 값을 사용
        // if (Input_XMLname != null && !string.IsNullOrEmpty(Input_XMLname.text))
        if (string.IsNullOrEmpty(_ui_fileName) == false)
        {
            baseName = _ui_fileName;
        }
        else
        {
            baseName = "unity_motion";
        }

        // .xml 확장자 제거
        if (baseName.EndsWith(".xml"))
        {
            baseName = baseName.Substring(0, baseName.Length - 4);
        }

        return $"{baseName}_{number}.xml";
    }

    // #endregion

    // #region 서버 전송 메서드

    /// <summary>
    /// 서버 URL 가져오기
    /// </summary>
    private string GetServerUrl()
    {
        if (Textbox_SeverPath == null || string.IsNullOrEmpty(Textbox_SeverPath.text))
        {
            Debug.LogWarning("[XMLSender] 서버 경로가 설정되지 않았습니다. 기본 경로를 사용합니다.");
            return defaultServerUrl;
        }

        return Textbox_SeverPath.text;
    }

    /// <summary>
    /// 기본 XML 전송 (기존 SendDefaultXML과 호환) - 최신 XML 파일의 내용을 전송
    /// </summary>
    public void SendDefaultXML()
    {
        string latestFile = GetLatestXmlFile();

        if (string.IsNullOrEmpty(latestFile))
        {
            Debug.LogError("[XMLSender] 전송할 XML 파일이 없습니다!");
            return;
        }

        string xmlContent = ReadXmlFileContent(latestFile);

        if (string.IsNullOrEmpty(xmlContent))
        {
            Debug.LogError($"[XMLSender] {latestFile} 파일을 읽을 수 없습니다!");
            return;
        }

        // Input_XMLname 필드의 값을 파일명으로 사용
        string fileName = DefaultXmlFilename;

        SendXMLToRobot(xmlContent, fileName);
    }

    /// <summary>
    /// 모든 XML 파일 전송 - XML 폴더의 모든 파일을 번호를 붙여서 전송
    /// </summary>
    public void SendAllXML()
    {
        List<string> allXmlFiles = GetAllXmlFiles();

        if (allXmlFiles.Count == 0)
        {
            Debug.LogError("[XMLSender] 전송할 XML 파일이 없습니다!");
            return;
        }

        Debug.Log($"[XMLSender] 총 {allXmlFiles.Count}개의 XML 파일을 전송합니다.");

        // 모든 파일을 순차적으로 전송
        StartCoroutine(SendAllXMLCoroutine(allXmlFiles));
    }

    /// <summary>
    /// 모든 XML 파일을 순차적으로 전송하는 코루틴
    /// </summary>
    private IEnumerator SendAllXMLCoroutine(List<string> xmlFiles)
    {
        for (int i = 0; i < xmlFiles.Count; i++)
        {
            string currentFile = xmlFiles[i];
            string xmlContent = ReadXmlFileContent(currentFile);

            if (string.IsNullOrEmpty(xmlContent))
            {
                Debug.LogError($"[XMLSender] {currentFile} 파일을 읽을 수 없습니다!");
                continue;
            }

            // 번호가 붙은 파일명 생성 (motion_1.xml, motion_2.xml...)
            string numberedFileName = GetNumberedFileName(i + 1);

            Debug.Log($"[XMLSender] ({i + 1}/{xmlFiles.Count}) 전송 중: {currentFile} → {numberedFileName}");

            // XML 전송
            yield return StartCoroutine(SendXMLCoroutine(xmlContent, numberedFileName));

            // 다음 전송 전 잠시 대기 (서버 부하 방지)
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"[XMLSender] ✅ 모든 XML 파일 전송 완료! 총 {xmlFiles.Count}개 파일");
    }

    /// <summary>
    /// XML 내용을 서버로 전송
    /// </summary>
    public void SendXMLToRobot(string xmlContent, string filename = "unity_motion.xml")
    {
        StartCoroutine(SendXMLCoroutine(xmlContent, filename));
    }
   
   
    /// <summary>
    /// XML 데이터를 서버로 전송하는 코루틴
    /// </summary>
    public IEnumerator SendXMLCoroutine(string xmlContent, string filename)
    {
        string serverUrl = GetServerUrl();
        Debug.Log($"[XMLSender] 사용할 서버 URL: {serverUrl}");

        // 서버에서 요구하는 JSON 형식으로 데이터 구성
        var requestData = new XMLRequestData
        {
            motionXmlTextData = xmlContent, // XML 파일의 텍스트 내용
            fileNm = filename, // Input_XMLname 필드의 값
            motionRegDt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            robotId = robotId,
            commandType = commandType,
            motionRmrk = Rmrk
        };

        string jsonData = JsonUtility.ToJson(requestData);

        // 디버그용: 전송할 JSON 데이터 출력
        Debug.Log($"[XMLSender] 전송할 데이터 - 파일명: {filename}");
        Debug.Log($"[XMLSender] XML 내용 길이: {xmlContent.Length} 문자");

        using (UnityWebRequest request = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ XML 전송 성공: {filename}");
                Debug.Log($"서버 응답: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"❌ XML 전송 실패: {request.error}");
                Debug.LogError($"응답 코드: {request.responseCode}");
                Debug.LogError($"응답 내용: {request.downloadHandler.text}");
            }
        }
    }

    // #endregion

    // #region 현재 포즈 전송 (Make_XML 연동)

    /// <summary>
    /// 현재 BioIK 상태를 XML로 생성해서 즉시 전송
    /// </summary>
    public void SendCurrentPoseXML()
    {
        if (makeXml == null)
        {
            Debug.LogError("[XMLSender] Make_XML이 연결되지 않았습니다!");
            return;
        }

        // 현재 상태로 임시 XML 생성
        makeXml.MakeSingleXml();

        // 잠시 대기 후 생성된 파일의 내용을 읽어서 전송
        StartCoroutine(SendCurrentPoseDelayed());
    }

    private IEnumerator SendCurrentPoseDelayed()
    {
        yield return new WaitForSeconds(0.1f); // 파일 생성 대기

        string singleXmlFile = "motion_single.xml";
        string xmlContent = ReadXmlFileContent(singleXmlFile);

        if (!string.IsNullOrEmpty(xmlContent))
        {
            string fileName = DefaultXmlFilename;
            SendXMLToRobot(xmlContent, fileName);
        }
        else
        {
            Debug.LogError("[XMLSender] 현재 포즈 XML을 생성할 수 없습니다!");
        }
    }

    /// <summary>
    /// Make_XML의 poses 리스트를 XML로 생성해서 전송
    /// </summary>
    public void SendPosesListXML()
    {
        if (makeXml == null || makeXml.poses == null || makeXml.poses.Count == 0)
        {
            Debug.LogError("[XMLSender] 전송할 포즈 데이터가 없습니다!");
            return;
        }

        // poses 리스트를 XML로 저장
        makeXml.SavePoseListToXml(makeXml.poses, 1);

        // 잠시 대기 후 생성된 파일의 내용을 읽어서 전송
        StartCoroutine(SendPosesListDelayed());
    }

    private IEnumerator SendPosesListDelayed()
    {
        yield return new WaitForSeconds(0.1f); // 파일 생성 대기

        string latestFile = GetLatestXmlFile();

        if (!string.IsNullOrEmpty(latestFile))
        {
            string xmlContent = ReadXmlFileContent(latestFile);
            if (!string.IsNullOrEmpty(xmlContent))
            {
                string fileName = DefaultXmlFilename;
                SendXMLToRobot(xmlContent, fileName);
            }
            else
            {
                Debug.LogError("[XMLSender] 포즈 리스트 XML을 읽을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("[XMLSender] 생성된 XML 파일을 찾을 수 없습니다!");
        }
    }

    // #endregion

    // #region 유틸리티 메서드

    /// <summary>
    /// 서버 URL 설정
    /// </summary>
    public void SetServerUrl(string newUrl)
    {
        if (Textbox_SeverPath != null)
        {
            Textbox_SeverPath.text = newUrl;
            Debug.Log($"[XMLSender] 서버 URL 변경됨: {newUrl}");
        }
    }

#if false //
    /// <summary>
    /// XML 파일명 설정
    /// </summary>
    public void SetXmlFileName(string newName)
    {
        if (Input_XMLname != null)
        {
            Input_XMLname.text = newName;
            Debug.Log($"[XMLSender] XML 파일명 변경됨: {newName}");
        }
    }
#endif

    /// <summary>
    /// 현재 서버 URL 가져오기
    /// </summary>
    public string GetCurrentServerUrl()
    {
        return GetServerUrl();
    }

    /// <summary>
    /// 현재 XML 파일명 가져오기
    /// </summary>
    public string GetCurrentXmlFileName()
    {
        return DefaultXmlFilename;
    }

    /// <summary>
    /// XML 폴더 열기 (에디터에서만 동작)
    /// </summary>
    [ContextMenu("XML 폴더 열기")]
    public void OpenXmlFolder()
    {
        if (makeXml == null) return;

        string folderPath = Path.Combine(Application.streamingAssetsPath, makeXml.xmlSaveFolder);

        if (Directory.Exists(folderPath))
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(folderPath);
#else
            System.Diagnostics.Process.Start(folderPath);
#endif
        }
        else
        {
            Debug.LogWarning($"[XMLSender] 폴더가 존재하지 않습니다: {folderPath}");
        }
    }

    // #endregion

    // #region 황윤성 추가 메서드

    /// <summary>
    /// [V3 수정] 하나의 큰 XML 문자열을 받아, 에디터에서는 파일로 저장하고 WebGL에서는 분할 전송합니다.
    /// </summary>
    public void SendLargeXmlInChunks(string largeXmlContent, string baseFileName)
    {
        if (string.IsNullOrEmpty(largeXmlContent))
        {
            Debug.LogError("[XMLSender] 전송/저장할 XML 내용이 없습니다.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL 환경일 경우: 서버로 분할 전송
        StartCoroutine(SendChunksCoroutine(largeXmlContent, baseFileName));
#else
        // 에디터 또는 PC 빌드 환경일 경우: 로컬 파일로 저장
        SaveChunksToLocalFiles(largeXmlContent, baseFileName);
#endif
    }

    /// <summary>
    /// [V4 수정] 에디터/PC 환경에서 XML을 분할하여 "가독성 좋게" 로컬 파일로 저장하는 메서드.
    /// </summary>
    private void SaveChunksToLocalFiles(string largeXmlContent, string baseFileName)
    {
        Debug.Log("[XMLSender] 에디터 환경 감지. XML 데이터를 로컬 파일로 저장합니다.");
        string folderPath = Path.Combine(Application.streamingAssetsPath, makeXml.xmlSaveFolder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        try
        {
            XmlDocument originalDoc = new XmlDocument();
            originalDoc.LoadXml(largeXmlContent);
            XmlNodeList allPoseNodes = originalDoc.SelectNodes("//Pose");

            if (allPoseNodes == null || allPoseNodes.Count == 0)
            {
                // Pose가 없으면 단일 파일로 예쁘게 저장
                string filePath = Path.Combine(folderPath, $"{baseFileName}_1.xml");
                originalDoc.Save(filePath); // XmlDocument.Save는 기본적으로 들여쓰기를 적용합니다.
                Debug.Log($"[XMLSender] 파일 저장 완료: {filePath}");
                return;
            }

            int totalPoses = allPoseNodes.Count;
            int fileCounter = 1;
            const int chunkSize = 50;

            for (int i = 0; i < totalPoses; i += chunkSize)
            {
                XmlDocument chunkDoc = new XmlDocument();
                string motionId = makeXml != null ? makeXml.motionID.ToString() : "10000";

                XmlElement motionElem = chunkDoc.CreateElement("Motion");
                motionElem.SetAttribute("ID", motionId);
                chunkDoc.AppendChild(motionElem);

                for (int j = 0; j < chunkSize && (i + j) < totalPoses; j++)
                {
                    XmlNode importedNode = chunkDoc.ImportNode(allPoseNodes[i + j], true);
                    motionElem.AppendChild(importedNode);
                }

                string chunkFileName = $"{baseFileName}_{fileCounter++}.xml";
                string filePath = Path.Combine(folderPath, chunkFileName);

                // ▼▼▼ 가독성을 위한 XmlWriter 설정 ▼▼▼
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true, // 들여쓰기 활성화
                    IndentChars = ("  "), // 탭 대신 스페이스 2칸으로 들여쓰기
                    NewLineOnAttributes = false, // 속성은 줄바꿈 안함
                    Encoding = Encoding.UTF8
                };

                // XmlWriter를 사용하여 포맷에 맞춰 파일 저장
                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    chunkDoc.Save(writer);
                }
                // ▲▲▲ 수정 완료 ▲▲▲

                Debug.Log($"[XMLSender] 파일 저장 완료: {filePath} ({motionElem.ChildNodes.Count}개 Pose)");
            }

            Debug.Log($"✅ [XMLSender] 총 {fileCounter - 1}개의 XML 파일 저장 완료!");
        }
        catch (Exception e)
        {
            Debug.LogError($"[XMLSender] 로컬 파일 저장 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// WebGL 환경에서 XML을 분할하여 서버로 전송하는 코루틴.
    /// </summary>
    private IEnumerator SendChunksCoroutine(string largeXmlContent, string baseFileName)
    {
        Debug.Log("[XMLSender] WebGL 환경 감지. XML 데이터를 분할하여 전송합니다.");

        // 기존 try 블록 시작 부분 제거
        XmlDocument originalDoc = new XmlDocument();
        originalDoc.LoadXml(largeXmlContent);
        XmlNodeList allPoseNodes = originalDoc.SelectNodes("//Pose");

        if (allPoseNodes == null || allPoseNodes.Count == 0)
        {
            Debug.LogWarning("[XMLSender] 전송할 Pose 데이터가 없습니다. 단일 빈 Motion 태그를 전송합니다.");
            yield return StartCoroutine(SendXMLCoroutine("<Motion ID=\"" + (makeXml != null ? makeXml.motionID.ToString() : "10000") + "\"></Motion>", $"{baseFileName}_1.xml"));
            yield break;
        }

        int totalPoses = allPoseNodes.Count;
        int fileCounter = 1;
        const int chunkSize = 50; // 한 번에 보낼 Pose의 개수

        for (int i = 0; i < totalPoses; i += chunkSize)
        {
            XmlDocument chunkDoc = new XmlDocument();
            string motionId = makeXml != null ? makeXml.motionID.ToString() : "10000";

            XmlElement motionElem = chunkDoc.CreateElement("Motion");
            motionElem.SetAttribute("ID", motionId);
            chunkDoc.AppendChild(motionElem);

            for (int j = 0; j < chunkSize && (i + j) < totalPoses; j++)
            {
                XmlNode importedNode = chunkDoc.ImportNode(allPoseNodes[i + j], true);
                motionElem.AppendChild(importedNode);
            }

            string chunkFileName = $"{baseFileName}_{fileCounter++}.xml";
            string chunkXmlContent = chunkDoc.OuterXml;

            Debug.Log($"[XMLSender] ({fileCounter - 1} / {Mathf.CeilToInt((float)totalPoses / chunkSize)}) 전송 중: {chunkFileName} ({motionElem.ChildNodes.Count}개 Pose)");
            yield return StartCoroutine(SendXMLCoroutine(chunkXmlContent, chunkFileName));

            // 서버 과부하 방지를 위한 딜레이
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"✅ [XMLSender] 총 {fileCounter - 1}개의 XML 청크 전송 완료!");
        // 기존 catch 블록 제거
    }
    // #endregion

}

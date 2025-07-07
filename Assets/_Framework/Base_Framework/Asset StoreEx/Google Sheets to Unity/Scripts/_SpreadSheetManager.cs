using System.Collections;
using System.Collections.Generic;
using System.Text;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using TinyJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Linq;

#pragma warning disable 0618

namespace GoogleSheetsToUnity
{

    public delegate void _OnSpreadSheetLoaded(GstuSpreadSheet sheet);

    /// <summary>
    /// Partial class for the spreadsheet manager to handle all private functions
    /// </summary>
    public class _SpreadSheetManager : SpreadsheetManager
    {
        private static string LOG_FORMAT = "<color=#94B530><b>[_SpreadSheetManager]</b></color> {0}";

        public static string _GSTU_ConfigPath = "_GSTU_Config";

        public static new _GoogleSheetsToUnityConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = (_GoogleSheetsToUnityConfig)Resources.Load(_GSTU_ConfigPath);
                    Debug.LogWarningFormat(LOG_FORMAT, "Resources.Load(\"<b><color=yellow>" + _GSTU_ConfigPath + "</color></b>\")");
                }

                return _config as _GoogleSheetsToUnityConfig;
            }
            set
            {
                _config = value;
            }
        }

        protected static new IEnumerator CheckForRefreshToken()
        {
            if (Application.isPlaying)
            {
                yield return new GoogleSheetsToUnity.ThirdPary.Task(_GoogleAuthrisationHelper.CheckForRefreshOfToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(_GoogleAuthrisationHelper.CheckForRefreshOfToken());
            }
#endif
        }

        public static void ReadPublicSpreadSheet(_GSTU_Search search, _OnSpreadSheetLoaded callback)
        {
            Debug.LogFormat(LOG_FORMAT, "ReadPublicSpreadSheet(), worksheetName : <b>" + search.worksheetName + "</b>, startCell : <b>" + search.startCell + "</b>, endCell : <b>" + search.endCell + "</b>");

            if (string.IsNullOrEmpty(Config.API_Key) == true)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Missing API Key, please enter this in the confie settings");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell + ":" + search.endCell);
            sb.Append("?key=" + Config.API_Key);

            string url = sb.ToString();
            Debug.LogWarningFormat(LOG_FORMAT, "url : " + url);

            if (Application.isPlaying)
            {
                new GoogleSheetsToUnity.ThirdPary.Task(_Read(new WWW(url), search.titleColumn, search.titleRow, callback));
            }
#if UNITY_EDITOR
            else
            {
                GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(_Read(new WWW(url), search.titleColumn, search.titleRow, callback));
            }
#endif
        }

        public static void Read(_GSTU_Search search, UnityAction<GstuSpreadSheet> callback, bool containsMergedCells = false)
        {
            Debug.LogFormat(LOG_FORMAT, "<b><color=magenta>Read</color></b>()");

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell + ":" + search.endCell);
            sb.Append("?access_token=" + Config.gdr.access_token);

            string url = sb.ToString();
            Debug.LogWarningFormat(LOG_FORMAT, "url : <b>" + url + "</b>");

            UnityWebRequest request = UnityWebRequest.Get(url);

            if (Application.isPlaying == true)
            {
                new GoogleSheetsToUnity.ThirdPary.Task(_Read(request, search, containsMergedCells, callback));
            }
#if UNITY_EDITOR
            else
            {
                GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(_Read(request, search, containsMergedCells, callback));
            }
#endif
        }

        private static IEnumerator _Read(WWW www, string titleColumn, int titleRow, _OnSpreadSheetLoaded callback)
        {
            Debug.LogFormat(LOG_FORMAT, "_Read()");
            yield return www;

            ValueRange rawData = JSON.Load(www.text).Make<ValueRange>();
            GSTU_SpreadsheetResponce responce = new GSTU_SpreadsheetResponce(rawData);

            _GstuSpreadSheet spreadSheet = new _GstuSpreadSheet(responce, titleColumn, titleRow);

            if (callback != null)
            {
                callback(spreadSheet);
            }
        }

        private static IEnumerator _Read(UnityWebRequest request, _GSTU_Search search, bool containsMergedCells, UnityAction<GstuSpreadSheet> callback)
        {
            if (Application.isPlaying)
            {
                yield return new GoogleSheetsToUnity.ThirdPary.Task(CheckForRefreshToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(CheckForRefreshToken());
            }
#endif

            using (request)
            {
                yield return request.SendWebRequest();

                Debug.LogFormat(LOG_FORMAT, "request.downloadHandler.text : <color=yellow>" + request.downloadHandler.text + "</color>");
                if (string.IsNullOrEmpty(request.downloadHandler.text) || request.downloadHandler.text == "{}")
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "Unable to Retreive data from google sheets");
                    yield break;
                }

                ValueRange rawData = JSON.Load(request.downloadHandler.text).Make<ValueRange>();
                GSTU_SpreadsheetResponce responce = new GSTU_SpreadsheetResponce(rawData);

                //if it contains merged cells then process a second set of json data to know what these cells are
                if (containsMergedCells == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
                    sb.Append("/" + search.sheetId);
                    sb.Append("?access_token=" + Config.gdr.access_token);

                    string url = sb.ToString();
                    Debug.LogWarningFormat(LOG_FORMAT, "url : " + url);
                    UnityWebRequest uwr = UnityWebRequest.Get(url);

                    // yield return uwr.SendWebRequest();

                    Debug.LogWarningFormat(LOG_FORMAT, "uwr.url:" + uwr.url);
                    yield return uwr.SendWebRequest();

                    if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : " + uwr.result + ", Error While Sending: " + uwr.error);
                        if (uwr.error == "Malformed URL")
                        {
                            // Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                        }
                        // listener(uwr.error, null);
                    }
                    else
                    {
                        // listener(uwr.error, uwr.downloadHandler);
                        SheetsRootObject root = JSON.Load(uwr.downloadHandler.text).Make<SheetsRootObject>();
                        responce.sheetInfo = root.sheets.FirstOrDefault(x => x.properties.title == search.worksheetName);
                    }

                    uwr.Dispose();
                }

                if (callback != null)
                {
                    callback(new _GstuSpreadSheet(responce, search.titleColumn, search.titleRow));
                }
            }
        }

        public static void Append(_GSTU_Search search, ValueRange inputData, UnityAction callback)
        {
            Debug.LogFormat(LOG_FORMAT, "<b><color=magenta>Append</color></b>()");

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell);
            sb.Append(":append");
            sb.Append("?valueInputOption=USER_ENTERED");
            sb.Append("&access_token=" + Config.gdr.access_token);

            string json = JSON.Dump(inputData, EncodeOptions.NoTypeHints);

            string url = sb.ToString();
            Debug.LogWarningFormat(LOG_FORMAT, "url : " + url);
            UnityWebRequest request = UnityWebRequest.Post(url, "");

            //have to do this cause unitywebrequest post will nto accept json data corrently...
            byte[] bodyRaw = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            if (Application.isPlaying)
            {
                new GoogleSheetsToUnity.ThirdPary.Task(Append(request, callback));
            }
#if UNITY_EDITOR
            else
            {
                GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(Append(request, callback));
            }
#endif
        }

        public static void Write(_GSTU_Search search, ValueRange inputData, UnityAction callback)
        {
            Debug.LogFormat(LOG_FORMAT, "<b><color=magenta>Write</color></b>()");

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell + ":" + search.endCell);
            sb.Append("?valueInputOption=USER_ENTERED");
            sb.Append("&access_token=" + Config.gdr.access_token);

            string json = JSON.Dump(inputData, EncodeOptions.NoTypeHints);
            byte[] bodyRaw = new UTF8Encoding().GetBytes(json);

            string url = sb.ToString();
            Debug.LogWarningFormat(LOG_FORMAT, "url : " + url);
            UnityWebRequest request = UnityWebRequest.Put(url, bodyRaw);

            if (Application.isPlaying)
            {
                new GoogleSheetsToUnity.ThirdPary.Task(Write(request, callback));
            }
#if UNITY_EDITOR
            else
            {
                GoogleSheetsToUnity.ThirdPary.EditorCoroutineRunner.StartCoroutine(Write(request, callback));
            }
#endif
        }
    }
}
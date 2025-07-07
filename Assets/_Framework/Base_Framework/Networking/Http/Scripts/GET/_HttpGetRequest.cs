using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace _Base_Framework
{
    public class _HttpGetRequest : _HttpBaseRequest
    {
        private static string LOG_FORMAT = "<b><color=#61AFFE>[_Http</color><color=red>Get</color><color=#61AFFE>Request]</color></b> {0}";

        public delegate void onCompleteRequestDownloadFileListener(UnityWebRequest.Result result, string error, string filePath);

        public override string Uri
        {
            set
            {
                _uri = value;
                Debug.LogWarningFormat(LOG_FORMAT, "Uri : <b><color=yellow>" + value + "</color></b>");
            }
        }

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            RequestType = _RequestType.GET;
        }

        protected virtual void Start()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Start()");

            // http://server.partdb.com:7780/westarApi/openapi/1/member/login.do?ID=user1&PASSWORD=1234
            // StartCoroutine(GetRequest(_uri, GetRequestDone));
        }

        public void DoRequest(onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(_uri, null, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(_uri, null, listener));
            }
        }

        public void DoRequest(string uri, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, null, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, null, listener));
            }
        }

        public void DoRequestWithHeaders(string uri, List<HttpHeader> headerList, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, headerList, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, headerList, listener));
            }
        }

        public void DoRequestDownloadFile(string uri, string filePath, onCompleteRequestDownloadFileListener listener = null)
        {
            Debug.LogFormat(LOG_FORMAT, "DoRequestDownloadFile(), uri : <b><color=yellow>" + uri + "</color></b>, filePath : " + filePath);

            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest_DownloadFile(uri, filePath, OnDefaultCompleteRequestDownloadFile));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest_DownloadFile(uri, filePath, listener));
            }
        }

        protected virtual IEnumerator PostRequest(string uri, List<HttpHeader> headerList, onCompleteRequestListener listener)
        {
            Debug.Assert(listener != null);

            UnityWebRequest uwr = UnityWebRequest.Get(uri);

            // Indicates whether the UnityWebRequest system should employ the HTTP/1.1 chunked-transfer encoding method.
            Debug.LogFormat(LOG_FORMAT, "chunkedTransfer : " + uwr.chunkedTransfer);
            // Sets UnityWebRequest to attempt to abort after the number of seconds in timeout have passed.
            Debug.LogFormat(LOG_FORMAT, "timeout : " + uwr.timeout);

            // Headers
            if (headerList != null && headerList.Count > 0)
            {
                for (int i = 0; i < headerList.Count; i++)
                {
                    Debug.LogFormat(LOG_FORMAT, "Header name : <b>" + headerList[i].key + "</b>, value : <b>" + headerList[i].value + "</b>");
                    uwr.SetRequestHeader(headerList[i].key, headerList[i].value);
                }
            }

            yield return uwr.SendWebRequest();

            // Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
            Debug.LogFormat(LOG_FORMAT, "isDone : <b>" + uwr.isDone + "</b>");
            // The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
            Debug.LogFormat(LOG_FORMAT, "responseCode : <b>" + uwr.responseCode + "</b>"); //
            // uwr.isNetworkError = Returns true after this UnityWebRequest encounters a system error. (Read Only)
            // uwr.isHttpError - Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)
            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : <color=red>" + uwr.result + "</color>, Error While Sending : <color=red>" + uwr.error + "</color>");
                if (uwr.error == "Malformed URL")
                {
                    Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                }

                // error - A human-readable string describing any system errors encountered by this UnityWebRequest object 
                //         while handling HTTP requests or responses. (Read Only)
                listener(uwr.result, uwr.error, uwr.downloadHandler);
            }
            else
            {
                // Returns the number of bytes of body data the system has downloaded from the remote server. (Read Only)
                Debug.LogFormat(LOG_FORMAT, "downloadedBytes : " + uwr.downloadedBytes);
                // Debug.LogFormat(LOG_FORMAT, "<b>Received : " + uwr.downloadHandler.text + "</b>");
                listener(uwr.result, uwr.error, uwr.downloadHandler);
            }

            ReqCoroutine = null; // <========================================
        }

        // filePath = "string path = Path.Combine(Application.persistentDataPath, "unity3d.html");"
        protected virtual IEnumerator PostRequest_DownloadFile(string url, string filePath, onCompleteRequestDownloadFileListener listener)
        {
            Debug.Assert(listener != null);

            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == false)
            {
                // UnityWebRequest uwr = UnityWebRequest.Get(uri);
                UnityWebRequest uwr = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);

                // Indicates whether the UnityWebRequest system should employ the HTTP/1.1 chunked-transfer encoding method.
                Debug.LogFormat(LOG_FORMAT, "chunkedTransfer:" + uwr.chunkedTransfer);
                // Sets UnityWebRequest to attempt to abort after the number of seconds in timeout have passed.
                Debug.LogFormat(LOG_FORMAT, "timeout:" + uwr.timeout);
                uwr.downloadHandler = new DownloadHandlerFile(filePath);
                yield return uwr.SendWebRequest();

                // Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
                Debug.LogFormat(LOG_FORMAT, "isDone : " + uwr.isDone);
                // The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
                Debug.LogFormat(LOG_FORMAT, "responseCode : " + uwr.responseCode);
                // uwr.isNetworkError = Returns true after this UnityWebRequest encounters a system error. (Read Only)
                // uwr.isHttpError - Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : <color=red>" + uwr.result + "</color>, Error While Sending : <color=red>" + uwr.error + "</color>");
                    if (uwr.error == "Malformed URL")
                    {
                        Debug.LogFormat(LOG_FORMAT, "URL:" + url);
                    }

                    // error - A human-readable string describing any system errors encountered by this UnityWebRequest object 
                    //         while handling HTTP requests or responses. (Read Only)
                    listener(uwr.result, uwr.error, null);
                }
                else
                {
                    // Returns the number of bytes of body data the system has downloaded from the remote server. (Read Only)
                    Debug.LogFormat(LOG_FORMAT, "downloadedBytes : " + uwr.downloadedBytes);
                    Debug.LogFormat(LOG_FORMAT, "<b>removeFileOnAbort: " + ((DownloadHandlerFile)uwr.downloadHandler).removeFileOnAbort + "</b>");
                    listener(uwr.result, uwr.error, filePath);
                }
            }
            else
            {
                listener(UnityWebRequest.Result.DataProcessingError, "Already exists filePath:" + filePath, ""); // <========= Carefully use!!!!!
                // Debug.LogErrorFormat(LOG_FORMAT, "filePath:" + filePath);
            }

            ReqCoroutine = null; // <========================================
        }

        protected virtual void OnDefaultCompleteRequest(UnityWebRequest.Result result, string error, DownloadHandler downloadHandler)
        {
            if (result == UnityWebRequest.Result.Success)
            {
                Debug.LogFormat(LOG_FORMAT, "Received: " + downloadHandler.text);

                _JsonParser parser = new _JsonParser(downloadHandler.text);
                // JsonParser parser = new JsonParser("{ \"totalcount\":\"2\", \"data\":[{\"IDX\":\"2\", \"MEMBER_ID\":\"123123\", \"APIKEY\":\"22\", \"CREATE_TM\":\"2019-06-24 20:30:10.0\"}, {\"IDX\":\"1\", \"MEMBER_ID\":\"1234\", \"APIKEY\":\"11\", \"CREATE_TM\":\"2019-06-24 20:30:05.0\"}]}");

                JSONNode nodeArray = (JSONNode)parser.GetObject("data");
                if (nodeArray.IsArray == false)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "");
                }
                else
                {
                    //
                }

                for (int i = 0; i < nodeArray.Count; i++)
                {
                    JSONNode item = (JSONNode)parser.GetItemObject(nodeArray, i);
                    JSONNode.KeyEnumerator keyEnumerator = item.Keys;

                    while (keyEnumerator.MoveNext())
                    {
                        string key = keyEnumerator.Current;
                        Debug.LogFormat(LOG_FORMAT, "key:" + key);
                    }

                    Debug.LogFormat(LOG_FORMAT, "" + item["IDX"].Value);
                }
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Error While Sending : <b>" + error + "</b>");
            }
        }

        protected virtual void OnDefaultCompleteRequestDownloadFile(UnityWebRequest.Result result, string error, string filePath)
        {
            if (result == UnityWebRequest.Result.Success)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Downloaded filePath : " + filePath);
            }
            else
            {
                if (result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "error : " + error);
                }
                else
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "error : " + error);
                }
            }
        }
    }
}
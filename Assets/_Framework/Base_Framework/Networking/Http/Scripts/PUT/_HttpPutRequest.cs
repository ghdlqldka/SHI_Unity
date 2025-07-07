using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace _Base_Framework
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class _HttpPutRequest : _HttpBaseRequest
    {
        private static string LOG_FORMAT = "<b><color=#FCA130>[_Http</color><color=red>Put</color><color=#FCA130>Request]</color></b> {0}";

        public delegate void onCompleteRequestDownloadFileListener(string error, string filePath);

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

            RequestType = _RequestType.PUT;
        }

        protected virtual void Start()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Start()");

            // http://server.partdb.com:7780/westarApi/openapi/1/member/login.do?ID=user1&PASSWORD=1234
            // Uri = "https://www.my-server.com/upload";
            // DoRequest("This is some test data", null);
        }

        public void DoRequest(string bodyData, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(_uri, bodyData, null, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(_uri, bodyData, null, listener));
            }
        }

        public void DoRequest(string uri, string bodyData, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, bodyData, null, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, bodyData, null, listener));
            }
        }

        public void DoRequestWithHeaders(string uri, string bodyData, List<HttpHeader> headerList, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, bodyData, headerList, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, bodyData, headerList, listener));
            }
        }

        protected virtual IEnumerator PostRequest(string uri, string bodyData, List<HttpHeader> headerList, onCompleteRequestListener listener)
        {
            Debug.Assert(listener != null);

            // byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
            // WebRequest.Put(string url, string data);

            UnityWebRequest uwr = UnityWebRequest.Put(uri, bodyData);

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
                    Debug.LogError("");
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
                Debug.LogErrorFormat(LOG_FORMAT, "Error While Sending:<b>" + error + "</b>");
            }
        }
    }
}
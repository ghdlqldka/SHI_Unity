using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// #pragma warning disable 0618

namespace _Base_Framework
{
    public class _HttpPostRequest : _HttpBaseRequest
    {
        private static string LOG_FORMAT = "<b><color=#49CC90>[_Http</color><color=red>Post</color><color=#49CC90>Request]</color></b> {0}";

        public override string Uri
        {
            set
            {
                _uri = value;
                Debug.LogWarningFormat(LOG_FORMAT, "Uri : <b><color=yellow>" + value + "</color></b>");
            }
        }

        [SerializeField]
        protected string _postData;
        public string postData
        {
            protected get
            {
                return postData;
            }
            set
            {
                postData = value;
                Debug.LogFormat(LOG_FORMAT, "PostData:" + _postData);
            }
        }

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            RequestType = _RequestType.POST;
        }

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            // http://server.partdb.com:7780/westarApi/openapi/1/member/login.do?ID=user1&PASSWORD=1234
            // https://arsns.k3i.co.kr/api/writePhrase/writePhraseList.do?bldg_se_cd=ALL
            // StartCoroutine(PostRequest(_uri, PostRequestDone));

            /*
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            // formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
            formData.Add(new MultipartFormFileSection("secnNm", "삼성"));
            formData.Add(new MultipartFormFileSection("ServiceKey", "k66p/GbQ/t5P50HgvqBZV2x2CX4vEeVF8DerpzMtRbUR+v/2NQSSWeSUUwMa1JSlxMPKSqGukbcyALN9WbRT0Q=="));
            */

            /*
            WWWForm formData = new WWWForm();
            formData.AddField("bldg_se_cd", "ALL");
            DoRequest("http://arsns.k3i.co.kr/api/writePhrase/writePhraseList.do", formData);
            */
        }

        public virtual void DoRequest(onCompleteRequestListener listener = null)
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

        public virtual void DoRequest(string uri, onCompleteRequestListener listener = null)
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

        public virtual void DoRequest(string uri, List<IMultipartFormSection> formData, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, formData, null, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, formData, null, listener));
            }
        }

        public virtual void DoRequest(string uri, WWWForm formData, onCompleteRequestListener listener = null)
        {
            Debug.LogFormat(LOG_FORMAT, "DoRequest(), uri : <b><color=yellow>" + uri + "</color></b>");

            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, formData, null, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, formData, null, listener));
            }
        }

        public void DoRequestWithHeaders(string uri, WWWForm formData, List<HttpHeader> headerList, onCompleteRequestListener listener = null)
        {
            Debug.LogFormat(LOG_FORMAT, "DoRequest(), uri : <b><color=yellow>" + uri + "</color></b>");

            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, formData, headerList, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest(uri, formData, headerList, listener));
            }
        }

        public void DoRequestUploadFile(string uri, string filePath, onCompleteRequestListener listener = null)
        {
            if (IsRunning == true)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Already, PROCESSING SendWebRequest!!!!!");
                return;
            }

            if (listener == null)
            {
                ReqCoroutine = StartCoroutine(PostRequest_UploadFile(uri, filePath, OnDefaultCompleteRequest));
            }
            else
            {
                ReqCoroutine = StartCoroutine(PostRequest_UploadFile(uri, filePath, listener));
            }
        }

        protected virtual IEnumerator PostRequest(string uri, List<HttpHeader> headerList, onCompleteRequestListener listener)
        {
            Debug.Assert(listener != null);

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));
            formData.Add(new MultipartFormFileSection("my file data", "myfile.txt"));

            /*
            // Debug.LogFormat(LOG_FORMAT, "PostRequest(), uri:" + uri);
            WWWForm form = new WWWForm();
            form.AddField("ID", "user1");
            form.AddField("PASSWORD", "1234");

            UnityWebRequest uwr = UnityWebRequest.Post(uri, form);
            */
            UnityWebRequest uwr = UnityWebRequest.Post(uri, formData);
            // uwr.chunkedTransfer = false;
            for (int i = 0; i < formData.Count; i++)
            {
                Debug.LogFormat(LOG_FORMAT, "formData:" + formData[i].contentType + ", " + formData[i].fileName + ", " + formData[i].sectionName + ", " + formData[i].sectionData);
            }
            
            Debug.LogWarningFormat(LOG_FORMAT, "uwr.url:" + uwr.url);

            // Headers
            if (headerList != null && headerList.Count > 0)
            {
                for (int i = 0; i < headerList.Count; i++)
                {
                    uwr.SetRequestHeader(headerList[i].key, headerList[i].value);
                }
            }

            yield return uwr.SendWebRequest();

            // Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
            Debug.LogFormat(LOG_FORMAT, "isDone : <b><color=yellow>" + uwr.isDone + "</color></b>");
            // The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
            // 500: 디코딩 오류. 헤더값 확인이 필요합니다.
            // 204 : Undocumented
            Debug.LogFormat(LOG_FORMAT, "responseCode : <b><color=yellow>" + uwr.responseCode + "</color></b>"); //
            // uwr.isNetworkError = Returns true after this UnityWebRequest encounters a system error. (Read Only)
            // uwr.isHttpError - Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : <color=red>" + uwr.result + "</color>, Error While Sending : <color=red>" + uwr.error + "</color>");
                if (uwr.error == "Malformed URL")
                {
                    Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                }
            }

            listener(uwr.result, uwr.error, uwr.downloadHandler);

            uwr.Dispose();
            ReqCoroutine = null; // <========================================
        }

        protected virtual IEnumerator PostRequest(string uri, List<IMultipartFormSection> formData, List<HttpHeader> headerList, onCompleteRequestListener listener)
        {
            Debug.Assert(listener != null);

            UnityWebRequest uwr = UnityWebRequest.Post(uri, formData);
            // uwr.chunkedTransfer = false;
            for (int i = 0; i < formData.Count; i++)
            {
                Debug.LogFormat(LOG_FORMAT, "formData:" + formData[i].contentType + ", " + formData[i].fileName + ", " + formData[i].sectionName + ", " + formData[i].sectionData);
            }

            Debug.LogWarningFormat(LOG_FORMAT, "uwr.url:" + uwr.url);

            // Headers
            if (headerList != null && headerList.Count > 0)
            {
                for (int i = 0; i < headerList.Count; i++)
                {
                    uwr.SetRequestHeader(headerList[i].key, headerList[i].value);
                }
            }

            yield return uwr.SendWebRequest();

            // Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
            Debug.LogFormat(LOG_FORMAT, "isDone : <b><color=yellow>" + uwr.isDone + "</color></b>");
            // The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
            // 500: 디코딩 오류. 헤더값 확인이 필요합니다.
            // 204 : Undocumented
            Debug.LogFormat(LOG_FORMAT, "responseCode : <b><color=yellow>" + uwr.responseCode + "</color></b>"); //
            // uwr.isNetworkError = Returns true after this UnityWebRequest encounters a system error. (Read Only)
            // uwr.isHttpError - Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : <color=red>" + uwr.result + "</color>, Error While Sending : <color=red>" + uwr.error + "</color>");
                if (uwr.error == "Malformed URL")
                {
                    Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                }
            }

            listener(uwr.result, uwr.error, uwr.downloadHandler);

            uwr.Dispose();
            ReqCoroutine = null; // <========================================
        }

        protected virtual IEnumerator PostRequest(string uri, WWWForm formData, List<HttpHeader> headerList, onCompleteRequestListener listener)
        {
            Debug.Assert(listener != null);

            // Debug.LogFormat(LOG_FORMAT, "PostRequest(), uri:" + uri);
            /*
            WWWForm formData = new WWWForm();
            formData.AddField("ID", "user1");
            formData.AddField("PASSWORD", "1234");
            */

            UnityWebRequest uwr = UnityWebRequest.Post(uri, formData);

            Debug.LogWarningFormat(LOG_FORMAT, "PostRequest(), uwr.url : <color=yellow>" + uwr.url + "</color>");

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
            Debug.LogFormat(LOG_FORMAT, "isDone : <b><color=yellow>" + uwr.isDone + "</color></b>");
            // The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
            // 500: 디코딩 오류. 헤더값 확인이 필요합니다.
            // 204 : Undocumented
            Debug.LogFormat(LOG_FORMAT, "responseCode : <b><color=yellow>" + uwr.responseCode + "</color></b>"); //
            // uwr.isNetworkError = Returns true after this UnityWebRequest encounters a system error. (Read Only)
            // uwr.isHttpError - Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : <color=red>" + uwr.result + "</color>, Error While Sending : <color=red>" + uwr.error + "</color>");
                if (uwr.error == "Malformed URL")
                {
                    Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                }
            }

            listener(uwr.result, uwr.error, uwr.downloadHandler);

            uwr.Dispose();
            ReqCoroutine = null; // <========================================
        }

        protected virtual IEnumerator PostRequest_UploadFile(string uri, string filePath, onCompleteRequestListener listener)
        {
            Debug.Assert(listener != null);
            Debug.LogFormat(LOG_FORMAT, "uri:" + uri);
            
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists == true)
            {
                Debug.LogFormat(LOG_FORMAT, "fileInfo.Name:" + fileInfo.Name);

                byte[] contents = FileUtils.LoadFileData(filePath);

                /*
                WWWForm form = new WWWForm();
                form.AddBinaryData("FILE_FNAME", contents, fileInfo.Name);
                UnityWebRequest uwr = UnityWebRequest.Post(uri, form);
                */
                List<IMultipartFormSection> formData = new List<IMultipartFormSection> {
             new MultipartFormFileSection("file", contents, fileInfo.Name, "image/png")
            };

                UnityWebRequest uwr = UnityWebRequest.Post(uri, formData);

                // yield return uwr.Send();
                yield return uwr.SendWebRequest();

                // Returns true after the UnityWebRequest has finished communicating with the remote server. (Read Only)
                Debug.LogFormat(LOG_FORMAT, "isDone : <b><color=yellow>" + uwr.isDone + "</color></b>");
                // The numeric HTTP response code returned by the server, such as 200, 404 or 500. (Read Only)
                // 500: 디코딩 오류. 헤더값 확인이 필요합니다.
                // 204 : Undocumented
                Debug.LogFormat(LOG_FORMAT, "responseCode : <b><color=yellow>" + uwr.responseCode + "</color></b>"); //
                                                                                                                     // uwr.isNetworkError = Returns true after this UnityWebRequest encounters a system error. (Read Only)
                                                                                                                     // uwr.isHttpError - Returns true after this UnityWebRequest receives an HTTP response code indicating an error. (Read Only)

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "uwr.result : <color=red>" + uwr.result + "</color>, Error While Sending : <color=red>" + uwr.error + "</color>");
                    if (uwr.error == "Malformed URL")
                    {
                        Debug.LogFormat(LOG_FORMAT, "URL:" + uri);
                    }
                }

                listener(uwr.result, uwr.error, uwr.downloadHandler);

                uwr.Dispose();
            }
            else
            {
                listener(UnityWebRequest.Result.DataProcessingError, "Upload filePath:" + filePath + " does not EXISTS!!!!!", null); // <============= Carefully use!!!!!
                // Debug.LogError("filePath:" + filePath);
            }
            
            ReqCoroutine = null; // <========================================
        }

        protected virtual void OnDefaultCompleteRequest(UnityWebRequest.Result result, string error, DownloadHandler downloadHandler)
        {
            if (result == UnityWebRequest.Result.Success)
            {
                Debug.LogFormat(LOG_FORMAT, "Received: <b>" + downloadHandler.text + "</b>");
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Error While Sending:<b>" + error + "</b>");
            }
        }

    }
}
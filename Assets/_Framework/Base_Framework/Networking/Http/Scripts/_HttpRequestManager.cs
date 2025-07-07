using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Base_Framework
{
    public class _HttpRequestManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#FF4097><b>[_HttpRequestManager]</b></color> {0}";

        protected static _HttpRequestManager _instance;
        public static _HttpRequestManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

#if UNITY_EDITOR
        [ReadOnly]
#endif
        [SerializeField]
        protected string _uri;
        public virtual string Uri
        {
            get
            {
                return _uri;
            }
            // protected set
            set
            {
                _uri = value;
                Debug.LogWarningFormat(LOG_FORMAT, "Uri : <b><color=yellow>" + value + "</color></b>");

                httpGetRequest.Uri = _uri;
                httpPostRequest.Uri = _uri;
                httpPutRequest.Uri = _uri;
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected _HttpGetRequest _httpGetRequest;
        public _HttpGetRequest httpGetRequest
        {
            get
            {
                return _httpGetRequest;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected _HttpPostRequest _httpPostRequest;
        public _HttpPostRequest httpPostRequest
        {
            get
            {
                return _httpPostRequest;
            }
        }
        [ReadOnly]
        [SerializeField]
        protected _HttpPutRequest _httpPutRequest;
        public _HttpPutRequest httpPutRequest
        {
            get
            {
                return _httpPutRequest;
            }
        }

        public bool IsBusy
        {
            get
            {
                bool isBusy = (httpGetRequest.IsRunning || httpPostRequest.IsRunning || httpPutRequest.IsRunning);
#if DEBUG
                if (isBusy == true)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "<color=cyan>IsBusy</color> : <color=red><b>" + isBusy + "</b></color>");
                }
#endif
                return isBusy;
            }
        }

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                if (_Base_Framework_Config.Product == _Base_Framework_Config._Product.Base_Framework)
                {
                    if (_GlobalObjectUtilities.Instance == null)
                    {
                        GameObject prefab = Resources.Load<GameObject>(_GlobalObjectUtilities.prefabPath);
                        Debug.Assert(prefab != null);
                        GameObject obj = Instantiate(prefab);
                        Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilities</b>\"</color>");
                        obj.name = prefab.name;
                    }
                }
                else
                {
                    Debug.Assert(false);
                }

                if (this.gameObject.GetComponent<_HttpGetRequest>() == null)
                {
                    _httpGetRequest = this.gameObject.AddComponent<_HttpGetRequest>();
                }
                if (this.gameObject.GetComponent<_HttpPostRequest>() == null)
                {
                    _httpPostRequest = this.gameObject.AddComponent<_HttpPostRequest>();
                }
                if (this.gameObject.GetComponent<_HttpPutRequest>() == null)
                {
                    _httpPutRequest = this.gameObject.AddComponent<_HttpPutRequest>();
                }

                Uri = "http://192.168.0.81:8080/westarApi/openapi/2/view/apikey/idx/desc/10/1/select.do";
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void Start()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Start()");

            /*
            // http://192.168.0.81:8080/westarApi/openapi/2/table/file/writer/fileType/upload.do
            Uri = "http://192.168.0.81:8080/westarApi/openapi/2/table/file/westar1/0/upload.do";
            if (IsBusy == false)
            {
                httpGetRequest.DoRequest();
            }
            // httpPostRequest.DoRequest(Uri);
            // httpPostRequest.DoRequestUploadFile(Uri, Application.dataPath + "/" + "Icon.png");
            */

            /*
            // http://192.168.0.81:8080/westarApi/fileDown.do?path=../resources/westarApi/upload/1561642780649.xml
            Uri = "http://192.168.0.81:8080/westarApi/fileDown.do?path=" + "../resources/westarApi/upload/1562037654704.png";
            if (IsBusy == false)
            {
                httpGetRequest.DoRequestDownloadFile(Uri, Application.dataPath + "/" + "Download.png");
            }
            if (IsBusy == false)
            {
                httpGetRequest.DoRequestDownloadFile(Uri, Application.dataPath + "/" + "Download.png");
            }
            */
        }
    }
}
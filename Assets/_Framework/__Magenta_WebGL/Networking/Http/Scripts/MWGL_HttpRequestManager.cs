using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_HttpRequestManager : _Magenta_Framework.HttpRequestManagerEx
    {
        private static string LOG_FORMAT = "<color=#FF4097><b>[HttpRequestManagerEx]</b></color> {0}";

        public static new MWGL_HttpRequestManager Instance
        {
            get
            {
                return _instance as MWGL_HttpRequestManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        public new MWGL_HttpGetRequest httpGetRequest
        {
            get
            {
                return _httpGetRequest as MWGL_HttpGetRequest;
            }
        }
        
        public new MWGL_HttpPostRequest httpPostRequest
        {
            get
            {
                return _httpPostRequest as MWGL_HttpPostRequest;
            }
        }
        
        public new MWGL_HttpPutRequest httpPutRequest
        {
            get
            {
                return _httpPutRequest as MWGL_HttpPutRequest;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                if (_Magenta_WebGL_Config.Product == _Base_Framework._Base_Framework_Config._Product.Magenta_WebGL)
                {
                    if (MWGL_GlobalObjectUtilities.Instance == null)
                    {
                        GameObject prefab = Resources.Load<GameObject>(MWGL_GlobalObjectUtilities.prefabPath);
                        Debug.Assert(prefab != null);
                        GameObject obj = Instantiate(prefab);
                        Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>MWGL_GlobalObjectUtilities</b>\"</color>");
                        obj.name = prefab.name;
                    }
                }
                else
                {
                    Debug.Assert(false);
                }

                if (this.gameObject.GetComponent<MWGL_HttpGetRequest>() == null)
                {
                    _httpGetRequest = this.gameObject.AddComponent<MWGL_HttpGetRequest>();
                }
                if (this.gameObject.GetComponent<MWGL_HttpPostRequest>() == null)
                {
                    _httpPostRequest = this.gameObject.AddComponent<MWGL_HttpPostRequest>();
                }
                if (this.gameObject.GetComponent<MWGL_HttpPutRequest>() == null)
                {
                    _httpPutRequest = this.gameObject.AddComponent<MWGL_HttpPutRequest>();
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

        protected override void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

    }
}
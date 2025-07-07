using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Magenta_Framework
{
    public class HttpRequestManagerEx : _Base_Framework._HttpRequestManager
    {
        private static string LOG_FORMAT = "<color=#FF4097><b>[HttpRequestManagerEx]</b></color> {0}";

        public static new HttpRequestManagerEx Instance
        {
            get
            {
                return _instance as HttpRequestManagerEx;
            }
            protected set
            {
                _instance = value;
            }
        }

        public new HttpGetRequestEx httpGetRequest
        {
            get
            {
                return _httpGetRequest as HttpGetRequestEx;
            }
        }
        
        public new HttpPostRequestEx httpPostRequest
        {
            get
            {
                return _httpPostRequest as HttpPostRequestEx;
            }
        }
        
        public new HttpPutRequestEx httpPutRequest
        {
            get
            {
                return _httpPutRequest as HttpPutRequestEx;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

                if (_Magenta_Framework_Config.Product == _Base_Framework._Base_Framework_Config._Product.Magenta_Framework)
                {
                    if (GlobalObjectUtilitiesEx.Instance == null)
                    {
                        GameObject prefab = Resources.Load<GameObject>(GlobalObjectUtilitiesEx.prefabPath);
                        Debug.Assert(prefab != null);
                        GameObject obj = Instantiate(prefab);
                        Debug.LogFormat(LOG_FORMAT, "<color=magenta>Instantiate \"<b>GlobalObjectUtilitiesEx</b>\"</color>");
                        obj.name = prefab.name;
                    }
                }
                else
                {
                    Debug.Assert(false);
                }

                if (this.gameObject.GetComponent<HttpGetRequestEx>() == null)
                {
                    _httpGetRequest = this.gameObject.AddComponent<HttpGetRequestEx>();
                }
                if (this.gameObject.GetComponent<HttpPostRequestEx>() == null)
                {
                    _httpPostRequest = this.gameObject.AddComponent<HttpPostRequestEx>();
                }
                if (this.gameObject.GetComponent<HttpPutRequestEx>() == null)
                {
                    _httpPutRequest = this.gameObject.AddComponent<HttpPutRequestEx>();
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
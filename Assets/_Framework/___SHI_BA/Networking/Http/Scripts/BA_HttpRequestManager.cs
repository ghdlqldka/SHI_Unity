using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _SHI_BA
{
    public class BA_HttpRequestManager : _Magenta_WebGL.MWGL_HttpRequestManager
    {
        private static string LOG_FORMAT = "<color=#FF4097><b>[BA_HttpRequestManager]</b></color> {0}";

        public static new BA_HttpRequestManager Instance
        {
            get
            {
                return _instance as BA_HttpRequestManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        public new BA_HttpGetRequest httpGetRequest
        {
            get
            {
                return _httpGetRequest as BA_HttpGetRequest;
            }
        }
        
        public new BA_HttpPostRequest httpPostRequest
        {
            get
            {
                return _httpPostRequest as BA_HttpPostRequest;
            }
        }
        
        public new BA_HttpPutRequest httpPutRequest
        {
            get
            {
                return _httpPutRequest as BA_HttpPutRequest;
            }
        }

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            if (Instance == null)
            {
                Instance = this;

#if false //
                if (_SamsungWebController_Config.Product == _Base_Framework._Base_Framework_Config._Product.SamsungWebController)
                {
                    if (SWC_GlobalObjectUtilities.Instance == null)
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
#endif

                if (this.gameObject.GetComponent<BA_HttpGetRequest>() == null)
                {
                    _httpGetRequest = this.gameObject.AddComponent<BA_HttpGetRequest>();
                }
                if (this.gameObject.GetComponent<BA_HttpPostRequest>() == null)
                {
                    _httpPostRequest = this.gameObject.AddComponent<BA_HttpPostRequest>();
                }
                if (this.gameObject.GetComponent<BA_HttpPutRequest>() == null)
                {
                    _httpPutRequest = this.gameObject.AddComponent<BA_HttpPutRequest>();
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
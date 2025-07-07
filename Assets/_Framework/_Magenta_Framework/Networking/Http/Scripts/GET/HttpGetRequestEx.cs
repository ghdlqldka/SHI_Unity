using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace _Magenta_Framework
{
    public class HttpGetRequestEx : _Base_Framework._HttpGetRequest
    {
        private static string LOG_FORMAT = "<b><color=#61AFFE>[Http</color><color=red>Get</color><color=#61AFFE>RequestEx]</color></b> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            RequestType = _RequestType.GET;
        }

        protected override void Start()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Start()");

            // http://server.partdb.com:7780/westarApi/openapi/1/member/login.do?ID=user1&PASSWORD=1234
            // StartCoroutine(GetRequest(_uri, GetRequestDone));
        }

    }
}
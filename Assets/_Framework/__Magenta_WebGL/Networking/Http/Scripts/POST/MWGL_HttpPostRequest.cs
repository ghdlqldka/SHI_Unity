using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// #pragma warning disable 0618

namespace _Magenta_WebGL
{
    public class MWGL_HttpPostRequest : _Magenta_Framework.HttpPostRequestEx
    {
        private static string LOG_FORMAT = "<b><color=#49CC90>[MWGL_Http</color><color=red>Post</color><color=#49CC90>Request]</color></b> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            RequestType = _RequestType.POST;
        }

        protected override void Start()
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
    }
}
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace _Magenta_WebGL
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class MWGL_HttpPutRequest : _Magenta_Framework.HttpPutRequestEx
    {
        private static string LOG_FORMAT = "<b><color=#FCA130>[MWGL_Http</color><color=red>Put</color><color=#FCA130>Request]</color></b> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            RequestType = _RequestType.PUT;
        }
    }
}
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618

namespace _Magenta_Framework
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class HttpPutRequestEx : _Base_Framework._HttpPutRequest
    {
        private static string LOG_FORMAT = "<b><color=#FCA130>[Http</color><color=red>Put</color><color=#FCA130>RequestEx]</color></b> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            RequestType = _RequestType.PUT;
        }
    }
}
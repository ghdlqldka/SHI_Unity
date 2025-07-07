using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace _Magenta_Framework
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class HttpPutRequestSampleEx : _Base_Framework._HttpPutRequestSample
    {
        private static string LOG_FORMAT = "<color=#66FF40><b>[HttpPutRequestSampleEx]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            StartCoroutine(Upload());
        }

    }
}
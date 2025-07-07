using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace _Magenta_WebGL
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class MWGL_HttpPutRequestSample : _Magenta_Framework.HttpPutRequestSampleEx
    {
        private static string LOG_FORMAT = "<color=#66FF40><b>[MWGL_HttpPutRequestSample]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            StartCoroutine(Upload());
        }

    }
}
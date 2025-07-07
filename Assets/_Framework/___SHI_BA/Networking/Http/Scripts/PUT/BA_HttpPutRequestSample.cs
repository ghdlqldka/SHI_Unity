using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace _SHI_BA
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class BA_HttpPutRequestSample : _Magenta_WebGL.MWGL_HttpPutRequestSample
    {
        private static string LOG_FORMAT = "<color=#66FF40><b>[BA_HttpPutRequestSample]</b></color> {0}";

        protected override void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            StartCoroutine(Upload());
        }

    }
}
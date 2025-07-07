using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace _Base_Framework
{
    // https://docs.unity3d.com/kr/2021.3/Manual/UnityWebRequest-UploadingRawData.html
    public class _HttpPutRequestSample : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#66FF40><b>[_HttpPutRequestSample]</b></color> {0}";

        protected virtual void Start()
        {
            Debug.LogFormat(LOG_FORMAT, "Start()");

            StartCoroutine(Upload());
        }

        protected virtual IEnumerator Upload()
        {
            byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
            // WebRequest.Put(string url, string data);
            UnityWebRequest www = UnityWebRequest.Put("https://www.my-server.com/upload", myData);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogErrorFormat(LOG_FORMAT, www.error);
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, "Upload complete!");
            }
        }
    }
}
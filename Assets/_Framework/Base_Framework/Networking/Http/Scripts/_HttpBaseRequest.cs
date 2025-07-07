using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace _Base_Framework
{
    public abstract class _HttpBaseRequest : MonoBehaviour
    {
        private static string LOG_FORMAT = "<color=#40FF5E><b>[_HttpBaseRequest]</b></color> {0}";

        public enum _RequestType
        {
            GET,
            POST,
            PUT,
        }

        public _RequestType RequestType
        {
            get;
            protected set;
        }

        // UnityWebRequest.SetRequestHeader
        public class HttpHeader
        {
            public string key;
            public string value;

            public HttpHeader(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        [SerializeField]
        protected string _uri;
        public virtual string Uri
        {
            set
            {
                _uri = value;
                Debug.LogWarningFormat(LOG_FORMAT, "Uri : <b><color=yellow>" + value + "</color></b>");
            }
        }

        public delegate void onCompleteRequestListener(UnityWebRequest.Result result, string error, DownloadHandler downloadHandler);
        // public abstract void DoRequest(onCompleteRequestListener listener = null);
        // public abstract void DoRequest(string uri, onCompleteRequestListener listener = null);

        [ReadOnly]
        [SerializeField]
        bool _isRunning = false;
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                _isRunning = value;
            }
        }

        protected Coroutine _reqCoroutine = null;
        protected Coroutine ReqCoroutine
        {
            get
            {
                return _reqCoroutine;
            }
            set
            {
                _reqCoroutine = value;
                if (_reqCoroutine == null)
                {
                    IsRunning = false;
                }
                else
                {
                    IsRunning = true;
                }
            }
        }
        // protected abstract IEnumerator PostRequest(string uri, onCompleteRequestListener listener);
    }
}
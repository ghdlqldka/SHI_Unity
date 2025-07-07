using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace _Base_Framework
{
    public class _NetworkManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<b><color=#D500FF>[_NetworkManager]</color></b> {0}";

        // [ReadOnly]
        // [SerializeField]
        protected IPAddress _ipAddress;
        public IPAddress ipAddress
        {
            get
            {
                return _ipAddress;
            }
        }

        // protected NetworkReachability _internetReachability = NetworkReachability.NotReachable;
        protected NetworkReachability _internetReachability = (NetworkReachability)(-1);
        public NetworkReachability InternetReachability
        {
            get
            {
                return _internetReachability;
            }
            protected set
            {
                if (_internetReachability != value)
                {
                    Debug.LogWarningFormat(LOG_FORMAT, "InternetReachability : <b><color=red>" + value + "</color></b>");
                    _internetReachability = value;
                    Invoke_OnChangedInternetReachability(value);
                }
            }
        }

        public delegate void ChangedInternetReachability(NetworkReachability internetReachability);
        public static event ChangedInternetReachability OnChangedInternetReachability;
        protected void Invoke_OnChangedInternetReachability(NetworkReachability internetReachability)
        {
            if (OnChangedInternetReachability != null)
            {
                OnChangedInternetReachability(internetReachability);
            }
        }

#if DEBUG
        [Header("==========> For DEBUGGING <==========")]
        [ReadOnly]
        [SerializeField]
        protected string DEBUG_ipAddress;
#endif

        protected virtual void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            _ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
#if DEBUG
            DEBUG_ipAddress = _ipAddress.ToString();
            Debug.LogFormat(LOG_FORMAT, "_ipAddress : <b><color=red>" + _ipAddress + "</color></b>");
#endif
        }

        /*
        void Update()
        {
            //Output the network reachability to the console window
            Debug.Log("Internet : " + m_ReachabilityText);
            //Check if the device cannot reach the internet
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //Change the Text
                m_ReachabilityText = "Not Reachable.";
            }
            //Check if the device can reach the internet via a carrier data network
            else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                m_ReachabilityText = "Reachable via carrier data network.";
            }
            //Check if the device can reach the internet via a LAN
            else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                m_ReachabilityText = "Reachable via Local Area Network.";
            }
        }
        */

        protected virtual void OnEnable()
        {
            StartCoroutine(PostOnEnable());
        }

        protected virtual IEnumerator PostOnEnable()
        {
            while (true)
            {
                InternetReachability = Application.internetReachability;

                yield return new WaitForSeconds(1.0f);
            }
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
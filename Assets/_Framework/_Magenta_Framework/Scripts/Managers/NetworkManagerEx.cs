using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace _Magenta_Framework
{
    public class NetworkManagerEx : _Base_Framework._NetworkManager
    {
        private static string LOG_FORMAT = "<b><color=#D500FF>[NetworkManagerEx]</color></b> {0}";

        protected override void Awake()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "Awake()");

            _ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
#if DEBUG
            DEBUG_ipAddress = _ipAddress.ToString();
            Debug.LogFormat(LOG_FORMAT, "_ipAddress : <b><color=red>" + _ipAddress + "</color></b>");
#endif
        }

    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

namespace _Magenta_WebGL
{
    public class MWGL_NetworkManager : _Magenta_Framework.NetworkManagerEx
    {
        private static string LOG_FORMAT = "<b><color=#D500FF>[MWGL_NetworkManager]</color></b> {0}";

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
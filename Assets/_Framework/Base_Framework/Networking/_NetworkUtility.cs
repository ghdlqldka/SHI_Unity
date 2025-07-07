using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace _Base_Framework
{

    public class _NetworkUtility
    {
        // private static string LOG_FORMAT = "<color=magenta><b>[_NetworkUtility]</b></color> {0}";

#if false //
        public enum ADDRESS_FAMILY
        {
            IPv4, IPv6
        }

        public static string GetIPAddress()
        {
#if true
            throw new System.NotSupportedException("Check below code!!!!!!");
#else
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                GatewayIPAddressInformation addr = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();
                GatewayIPAddressInformation gate = ni.GetIPProperties().GatewayAddresses.FirstOrDefault();

                if (addr != null && addr.Address.ToString().Equals("0.0.0.0") == false)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            return string.Empty;
#endif
        }

        public static string GetIP(ADDRESS_FAMILY addressFamily)
        {
#if true
            throw new System.NotSupportedException("Check below code!!!!!!");
#else
            // Return null if ADDRESSFAM is Ipv6 but Os does not support it
            if (addressFamily == ADDRESS_FAMILY.IPv6 && Socket.OSSupportsIPv6 == false)
            {
                return null;
            }

            string output = "0.0.0.0";

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (addressFamily == ADDRESS_FAMILY.IPv4)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                if (ip.Address.ToString() != "127.0.0.1" && ip.Address.ToString().Contains("192.168.0"))
                                {
                                    output = ip.Address.ToString();
                                }
                            }
                        }
                        else if (addressFamily == ADDRESS_FAMILY.IPv6)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                if (ip.Address.ToString() != "127.0.0.1" && ip.Address.ToString().Contains("192.168.0"))
                                {
                                    output = ip.Address.ToString();
                                }
                            }
                        }
                        else
                        {
                            Debug.LogErrorFormat(LOG_FORMAT, "Unhandled addressFamily : " + addressFamily);
                        }
                    }
                }
            }

            return output;
#endif
        }
#endif
    }
}
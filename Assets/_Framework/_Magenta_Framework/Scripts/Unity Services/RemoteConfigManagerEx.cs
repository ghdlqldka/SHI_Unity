using UnityEngine;

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT // #if ENABLE_UNITY_GAME_SERVICES_ANALYTICS_SUPPORT
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif

using System.Threading.Tasks;

namespace _Magenta_Framework
{
    public class RemoteConfigManagerEx : _Base_Framework._RemoteConfigManager
    {
        private static string LOG_FORMAT = "<b><color=#00E011>[RemoteConfigManagerEx]</color></b> {0}";

        protected override void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
            _ = AwakeAsync(); // Fire and forget
#endif
        }
    }
}
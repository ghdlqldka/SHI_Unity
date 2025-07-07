// NOTE: You need to deploy the new-remote-config.rc file before being able to run this sample,
// to do so, open the deployment window and deploy the new-remote-config.rc file.
// See README.md for more details.

using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;

#if INPUT_SYSTEM_PRESENT
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.UI;
#endif

namespace Sample.Deployment.RemoteConfigSample
{
    public class _RemoteConfig : RemoteConfig
    {
        private static string LOG_FORMAT = "<color=#9EFF00><b>[_RemoteConfig]</b></color> {0}";

        // [SerializeField]
        protected userAttributes _userAttributes;
        protected appAttributes _appAttributes;

        protected override async void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");

#if INPUT_SYSTEM_PRESENT
            m_DefaultInputModule.enabled = false;
            m_DefaultInputModule.gameObject.AddComponent<InputSystemUIInputModule>();
            TouchSimulation.Enable();
#endif
            // Remote Config needs to be initialized and then the user must sign in.
            await InitializeServicesEx();
            await SignInAnonymouslyEx();

            // RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
            RemoteConfigService.Instance.FetchCompleted += OnFetchCompleted;

            await FetchConfigsAsync();
        }

        protected override void ApplyRemoteConfig(ConfigResponse obj)
        {
            throw new NotSupportedException("");
        }

        protected virtual void OnFetchCompleted(ConfigResponse obj)
        {
            Debug.LogFormat(LOG_FORMAT, "ApplyRemoteConfig()");

            CashedConfig = RemoteConfigService.Instance.appConfig;

            OnRemoteConfigUpdated?.Invoke(CashedConfig);
        }

        protected static async Task InitializeServicesEx()
        {
            Debug.LogFormat(LOG_FORMAT, "InitializeServicesEx(), UnityServices.State : <b><color=yellow>" + UnityServices.State + "</color></b>");
            // await RemoteConfig.InitializeServices();

            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
        }

        protected static async Task SignInAnonymouslyEx()
        {
            Debug.LogFormat(LOG_FORMAT, "InitializeServicesEx(), AuthenticationService.Instance.IsSignedIn : <b><color=yellow>" + 
                AuthenticationService.Instance.IsSignedIn + "</color></b>");

            if (AuthenticationService.Instance.IsSignedIn == false)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(null);
            }
        }

        public override async Task FetchConfigsAsync()
        {
            Debug.LogFormat(LOG_FORMAT, "FetchConfigsAsync()");

            _userAttributes = new userAttributes();
            _appAttributes = new appAttributes();
            await RemoteConfigService.Instance.FetchConfigsAsync(_userAttributes, _appAttributes);

            Debug.LogFormat(LOG_FORMAT, "_userAttributes.expansionFlag : " + _userAttributes.expansionFlag);
            Debug.LogFormat(LOG_FORMAT, "_appAttributes.level : " + _appAttributes.level + ", score : " + _appAttributes.score + ", appVersion : " + _appAttributes.appVersion);
        }
    }
}

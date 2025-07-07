// NOTE: You need to deploy the new-remote-config.rc file before being able to run this sample,
// to do so, open the deployment window and deploy the new-remote-config.rc file.
// See README.md for more details.

using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using UnityEngine.EventSystems;
#if INPUT_SYSTEM_PRESENT
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.UI;
#endif

namespace Sample.Deployment.RemoteConfigSample
{
    public class RemoteConfig : MonoBehaviour
    {
        [SerializeField]
        protected StandaloneInputModule m_DefaultInputModule;
        
        public RuntimeConfig CashedConfig { get; protected set; }

        public Action<RuntimeConfig> OnRemoteConfigUpdated;
        public struct userAttributes
        {
            // Optionally declare variables for any custom user attributes:
            public bool expansionFlag;
        }

        public struct appAttributes
        {
            // Optionally declare variables for any custom app attributes:
            public int level;
            public int score;
            public string appVersion;
        }

        protected virtual async void Awake()
        {
#if INPUT_SYSTEM_PRESENT
            m_DefaultInputModule.enabled = false;
            m_DefaultInputModule.gameObject.AddComponent<InputSystemUIInputModule>();
            TouchSimulation.Enable();
#endif
            // Remote Config needs to be initialized and then the user must sign in.
            await InitializeServices();
            await SignInAnonymously();

            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;

            await FetchConfigsAsync();
        }

        public virtual async Task FetchConfigsAsync()
        {
            await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
        }

        protected virtual void ApplyRemoteConfig(ConfigResponse obj)
        {
            CashedConfig = RemoteConfigService.Instance.appConfig;

            OnRemoteConfigUpdated?.Invoke(CashedConfig);
        }

        protected static async Task InitializeServices()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
        }

        protected static async Task SignInAnonymously()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
    }
}

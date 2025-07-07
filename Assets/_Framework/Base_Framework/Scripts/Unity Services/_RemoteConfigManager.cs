using UnityEngine;

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT // #if ENABLE_UNITY_GAME_SERVICES_ANALYTICS_SUPPORT
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif

using System.Threading.Tasks;

namespace _Base_Framework
{
    public class _RemoteConfigManager : MonoBehaviour
    {
        private static string LOG_FORMAT = "<b><color=#00E011>[_RemoteConfigManager]</color></b> {0}";

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
        public struct userAttributes 
        {
            // Optionally declare variables for any custom user attributes:
            // These variables can be updated as the game progresses and then used in Campaign Audience Targeting!

            // public int score;
        }
        public struct appAttributes 
        {
            // Optionally declare variables for any custom app attributes:

            // i.e
            // public int level
            // public string appVersion
        }
        public struct filterAttributes
        {
            // Optionally declare variables for attributes to filter on any of following parameters:
            // public string[] key;
            // public string[] type;
            // public string[] schemaId;
        }

        // Optionally declare a unique assignmentId if you need it for tracking:
        [ReadOnly]
        [SerializeField]
        protected string assignmentId;

        // [SerializeField]
        protected userAttributes uaStruct;
        // [SerializeField]
        protected appAttributes aaStruct;
        // [SerializeField]
        protected filterAttributes fAttributes;

        // Declare any Settings variables you¡¯ll want to configure remotely:
        [ReadOnly]
        [SerializeField]
        protected bool graphicsSettings_postProcessing;

        public delegate void ConfigRequestStatusChanged(ConfigRequestStatus requestStatus);
		public static event ConfigRequestStatusChanged OnConfigRequestStatusChanged;
#endif


        protected virtual void Awake()
        {
            Debug.LogFormat(LOG_FORMAT, "Awake()");
#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
            _ = AwakeAsync(); // Fire and forget
#endif
        }

#if ENABLE_UNITY_GAME_SERVICES_REMOTE_CONFIG_SUPPORT
        // Retrieve and apply the current key-value pairs from the service on Awake:
        protected virtual async Task AwakeAsync()
        {
            Debug.LogFormat(LOG_FORMAT, "AwakeAsync(), RemoteConfigService.Instance.requestStatus : <b>" + RemoteConfigService.Instance.requestStatus + "</b>");

            // initialize Unity's authentication and core services, however check for internet connection
            // in order to fail gracefully without throwing exception if connection does not exist
            if (Unity.Services.RemoteConfig.Utilities.CheckForInternetConnection() == true)
            {
                Debug.LogFormat(LOG_FORMAT, "Internet Connected");
                await InitializeRemoteConfigAsync();
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Utilities.CheckForInternetConnection() == false");
            }

            // Add a listener to apply settings when successfully retrieved:
            RemoteConfigService.Instance.FetchCompleted += OnRemoteConfigFetchCompleted;

            // you can set the user¡¯s unique ID:
            // RemoteConfigService.Instance.SetCustomUserID("some-user-id");

            // you can set the environment ID:
            // RemoteConfigService.Instance.SetEnvironmentID("an-env-id");

            // Fetch configuration settings from the remote service, they must be called with the attributes structs (empty or with custom attributes) to initiate the WebRequest.
            uaStruct = new userAttributes();
            aaStruct = new appAttributes();
            // await RemoteConfigService.Instance.FetchConfigsAsync<userAttributes, appAttributes>(uaStruct, aaStruct);

            // Example on how to fetch configuration settings using filter attributes:
            fAttributes = new filterAttributes();
            // fAttributes.key = new string[] { "sword","cannon" };
            await RemoteConfigService.Instance.FetchConfigsAsync(uaStruct, aaStruct, fAttributes);

            // Example on how to fetch configuration settings if you have dedicated configType:
            // var configType = "specialConfigType";
            // Fetch configs of that configType
            // RemoteConfigService.Instance.FetchConfigs(configType, new userAttributes(), new appAttributes());
            // Configuration can be fetched with both configType and fAttributes passed
            // RemoteConfigService.Instance.FetchConfigs(configType, new userAttributes(), new appAttributes(), fAttributes);

            // All examples from above will also work asynchronously, returning Task<RuntimeConfig>
            // await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
            // await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes(), fAttributes);
            // await RemoteConfigService.Instance.FetchConfigsAsync(configType, new userAttributes(), new appAttributes());
            // await RemoteConfigService.Instance.FetchConfigsAsync(configType, new userAttributes(), new appAttributes(), fAttributes);
        }

        // The Remote Config package depends on Unity's authentication and core services.
        // These dependencies require a small amount of user code for proper configuration.
        protected virtual async Task InitializeRemoteConfigAsync()
        {
            // Debug.LogFormat(LOG_FORMAT, "InitializeRemoteConfigAsync(), AuthenticationService.Instance.IsSignedIn : " + AuthenticationService.Instance.IsSignedIn);

            // options can be passed in the initializer, e.g if you want to set AnalyticsUserId or an EnvironmentName use the lines from below:
            // InitializationOptions options = new InitializationOptions()
            // .SetEnvironmentName("testing")
            // .SetAnalyticsUserId("test-user-id-12345");
            // await UnityServices.InitializeAsync(options);

            // initialize handlers for unity game services
            InitializationOptions options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);

            // remote config requires authentication for managing environment information
            if (AuthenticationService.Instance.IsSignedIn == false)
            {
                Debug.LogFormat(LOG_FORMAT, "AuthenticationService.Instance.SignInAnonymouslyAsync()");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }

        protected virtual void OnRemoteConfigFetchCompleted(ConfigResponse configResponse)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "OnRemoteConfigFetchCompleted(), configResponse.requestOrigin : <b>" + configResponse.requestOrigin + "</b>" +
                ", configResponse.status : <b>" + configResponse.status + "</b>");

            // Conditionally update settings, depending on the response's origin:
            switch (configResponse.requestOrigin)
            {
                case ConfigOrigin.Default:
                    // Debug.LogFormat(LOG_FORMAT, "No settings loaded this session and no local cache file exists; using default values.");
                    break;
                case ConfigOrigin.Cached:
                    // Debug.LogFormat(LOG_FORMAT, "No settings loaded this session; using cached values from a previous session.");
                    break;
                case ConfigOrigin.Remote:
                    // Debug.LogFormat(LOG_FORMAT, "New settings loaded this session; update values accordingly.");
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            switch (configResponse.status)
            {
                case ConfigRequestStatus.None:
                    break;
                case ConfigRequestStatus.Failed:
                    break;
                case ConfigRequestStatus.Success:
                    break;
                case ConfigRequestStatus.Pending:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            // production
            Debug.LogFormat(LOG_FORMAT, "graphicsSettings_postProcessing : <b>" + RemoteConfigService.Instance.appConfig.GetBool("graphicsSettings_postProcessing") + "</b>");

            assignmentId = RemoteConfigService.Instance.appConfig.assignmentId;

            // These calls could also be used with the 2nd optional arg to provide a default value, e.g:
            // enemyVolume = RemoteConfigService.Instance.appConfig.GetInt("enemyVolume", 100);
            graphicsSettings_postProcessing = RemoteConfigService.Instance.appConfig.GetBool("graphicsSettings_postProcessing");

            Invoke_OnConfigRequestStatusChanged(RemoteConfigService.Instance.requestStatus);
        }

        protected void Invoke_OnConfigRequestStatusChanged(ConfigRequestStatus requestStatus)
        {
            if (OnConfigRequestStatusChanged != null)
            {
                OnConfigRequestStatusChanged(requestStatus);
            }
        }
#endif
    }
}